/* =========================================================
   已儲存的 DB 連線（localStorage）— 全站共用模組
   - 儲存成功連線並命名
   - 進入 Overview（GET）時，若目前連線不在清單則彈出「記住DB連線」視窗
   - 於導覽列列出已存連線，可直接連線或刪除
   相關 UI 位於共用 partial（_nav.cshtml、_remDbModal.cshtml），由 _Layout 全站載入。
   ========================================================= */
(function () {
    var SAVED_CONN_KEY = 'schemaNote.savedConnections';

    function getSavedConnections() {
        try {
            var raw = localStorage.getItem(SAVED_CONN_KEY);
            if (!raw) return [];
            var arr = JSON.parse(raw);
            return Array.isArray(arr) ? arr : [];
        } catch (e) {
            return [];
        }
    }

    function setSavedConnections(list) {
        localStorage.setItem(SAVED_CONN_KEY, JSON.stringify(list));
    }

    // 以 Server Address 與 Database Name 作為唯一判斷依據（不分大小寫）。
    function isSameConnection(a, b) {
        if (!a || !b) return false;
        var s1 = (a.server || '').toLowerCase();
        var s2 = (b.server || '').toLowerCase();
        var d1 = (a.database || '').toLowerCase();
        var d2 = (b.database || '').toLowerCase();
        return s1 === s2 && d1 === d2;
    }

    function findSavedConnection(server, database) {
        var list = getSavedConnections();
        for (var i = 0; i < list.length; i++) {
            if (isSameConnection(list[i], { server: server, database: database })) {
                return list[i];
            }
        }
        return null;
    }

    function renderSavedConnections() {
        var container = document.getElementById('savedConnList');
        if (!container) return;
        var list = getSavedConnections();
        container.innerHTML = '';
        for (var i = 0; i < list.length; i++) {
            (function (conn) {
                var group = document.createElement('div');
                group.className = 'btn-group btn-group-sm saved-conn-chip';
                group.setAttribute('role', 'group');

                var connectBtn = document.createElement('button');
                connectBtn.type = 'button';
                connectBtn.className = 'btn btn-outline-primary';
                connectBtn.textContent = conn.name;
                connectBtn.title = (conn.server || '') + ' / ' + (conn.database || '');
                connectBtn.addEventListener('click', function () {
                    connectSavedConnection(conn);
                });

                var removeBtn = document.createElement('button');
                removeBtn.type = 'button';
                removeBtn.className = 'btn btn-outline-danger saved-conn-remove';
                removeBtn.setAttribute('aria-label', '刪除');
                removeBtn.title = '刪除此連線';
                removeBtn.innerHTML = '&times;';
                removeBtn.addEventListener('click', function (e) {
                    e.stopPropagation();
                    removeSavedConnection(conn);
                });

                group.appendChild(connectBtn);
                group.appendChild(removeBtn);
                container.appendChild(group);
            })(list[i]);
        }
    }

    // 按下已儲存連線 => 直接以其（AES 密文）連線字串送出，後端解密後連線（走 PRG）。
    function connectSavedConnection(conn) {
        var form = document.getElementById('savedConnConnectForm');
        var input = document.getElementById('savedConnConnectInput');
        if (!form || !input) return;
        input.value = conn.connectionString || '';
        form.submit();
    }

    function removeSavedConnection(conn) {
        var list = getSavedConnections();
        list = list.filter(function (item) {
            return !isSameConnection(item, conn);
        });
        setSavedConnections(list);
        renderSavedConnections();
    }

    // 進入 Overview（GET）後：若目前成功連線不在清單中，彈出「記住DB連線」視窗。
    function maybePromptRememberConnection() {
        var info = document.getElementById('currentConnInfo');
        if (!info) return;
        var server = info.getAttribute('data-server') || '';
        var database = info.getAttribute('data-database') || '';
        var connString = info.getAttribute('data-connstring') || ''; // AES 密文，前端只儲存密文
        if (!connString) return; // 沒有目前連線就不處理

        if (findSavedConnection(server, database)) return; // 已存在則不彈出

        var modalEl = document.getElementById('rememberDbModal');
        if (!modalEl || typeof bootstrap === 'undefined') return;

        document.getElementById('rememberDbServer').textContent = server;
        document.getElementById('rememberDbDatabase').textContent = database;

        var nameInput = document.getElementById('rememberDbName');
        nameInput.value = '';
        nameInput.classList.remove('is-invalid');

        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);

        var saveBtn = document.getElementById('rememberDbSaveBtn');
        saveBtn.onclick = function () {
            var name = (nameInput.value || '').trim();
            if (!name) {
                nameInput.classList.add('is-invalid');
                nameInput.focus();
                return;
            }
            var list = getSavedConnections();
            list.push({
                name: name,
                server: server,
                database: database,
                connectionString: connString
            });
            setSavedConnections(list);
            renderSavedConnections();
            modal.hide();
            alert('儲存成功');
        };

        modal.show();
    }

    // ===== 連線清單管理：匯出 / 匯入 / 移除 =====

    // 將單一 CSV 欄位值加上必要的雙引號跳脫。
    function csvEscape(value) {
        var s = (value === null || value === undefined) ? '' : String(value);
        if (/[",\r\n]/.test(s)) {
            return '"' + s.replace(/"/g, '""') + '"';
        }
        return s;
    }

    var CSV_HEADER = ['name', 'server', 'database', 'connectionString'];

    // 將 localStorage 連線清單序列化為 CSV（含標題列）。
    function toCsv(list) {
        var lines = [CSV_HEADER.join(',')];
        for (var i = 0; i < list.length; i++) {
            var c = list[i];
            lines.push([
                csvEscape(c.name),
                csvEscape(c.server),
                csvEscape(c.database),
                csvEscape(c.connectionString)
            ].join(','));
        }
        return lines.join('\r\n');
    }

    // 解析一行 CSV（支援雙引號跳脫），回傳欄位陣列。
    function parseCsvLine(line) {
        var result = [];
        var field = '';
        var inQuotes = false;
        for (var i = 0; i < line.length; i++) {
            var ch = line[i];
            if (inQuotes) {
                if (ch === '"') {
                    if (line[i + 1] === '"') { field += '"'; i++; }
                    else { inQuotes = false; }
                } else {
                    field += ch;
                }
            } else {
                if (ch === '"') { inQuotes = true; }
                else if (ch === ',') { result.push(field); field = ''; }
                else { field += ch; }
            }
        }
        result.push(field);
        return result;
    }

    // 解析整份 CSV 文字為連線物件陣列（自動略過標題列與空白列）。
    function parseCsv(text) {
        var rows = [];
        var lines = text.replace(/\r\n/g, '\n').replace(/\r/g, '\n').split('\n');
        for (var i = 0; i < lines.length; i++) {
            var line = lines[i];
            if (line.trim() === '') continue;
            var cols = parseCsvLine(line);
            var name = (cols[0] || '').trim();
            // 略過標題列
            if (i === 0 && name.toLowerCase() === 'name') continue;
            if (!name) continue;
            rows.push({
                name: name,
                server: (cols[1] || '').trim(),
                database: (cols[2] || '').trim(),
                connectionString: (cols[3] || '').trim()
            });
        }
        return rows;
    }

    // 匯出：顯示 CSV 內容並可複製到 Clipboard。
    function setupExport() {
        var btn = document.getElementById('exportConnBtn');
        var modalEl = document.getElementById('exportConnModal');
        if (!btn || !modalEl || typeof bootstrap === 'undefined') return;

        var textArea = document.getElementById('exportConnText');
        var copyBtn = document.getElementById('exportConnCopyBtn');
        var copiedHint = document.getElementById('exportConnCopied');

        function copyToClipboard(text) {
            if (navigator.clipboard && navigator.clipboard.writeText) {
                return navigator.clipboard.writeText(text);
            }
            // 後備方案
            textArea.removeAttribute('readonly');
            textArea.select();
            document.execCommand('copy');
            textArea.setAttribute('readonly', 'readonly');
            return Promise.resolve();
        }

        btn.addEventListener('click', function (e) {
            e.preventDefault();
            var csv = toCsv(getSavedConnections());
            textArea.value = csv;
            copiedHint.classList.add('d-none');
            var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
            modal.show();
            // 開啟時自動複製一次
            copyToClipboard(csv).then(function () {
                copiedHint.classList.remove('d-none');
            }).catch(function () { });
        });

        copyBtn.addEventListener('click', function () {
            copyToClipboard(textArea.value).then(function () {
                copiedHint.classList.remove('d-none');
            }).catch(function () {
                alert('複製失敗，請手動選取並複製。');
            });
        });
    }

    // 匯入：以連線名稱為識別值，整併入現有清單（同名覆蓋）。
    function setupImport() {
        var btn = document.getElementById('importConnBtn');
        var modalEl = document.getElementById('importConnModal');
        if (!btn || !modalEl || typeof bootstrap === 'undefined') return;

        var textArea = document.getElementById('importConnText');
        var confirmBtn = document.getElementById('importConnConfirmBtn');

        btn.addEventListener('click', function (e) {
            e.preventDefault();
            textArea.value = '';
            textArea.classList.remove('is-invalid');
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        });

        confirmBtn.addEventListener('click', function () {
            var incoming = parseCsv(textArea.value || '');
            if (incoming.length === 0) {
                textArea.classList.add('is-invalid');
                textArea.focus();
                return;
            }
            textArea.classList.remove('is-invalid');

            var list = getSavedConnections();
            var indexByName = {};
            for (var i = 0; i < list.length; i++) {
                indexByName[(list[i].name || '').toLowerCase()] = i;
            }

            var added = 0, updated = 0;
            for (var j = 0; j < incoming.length; j++) {
                var item = incoming[j];
                var key = item.name.toLowerCase();
                if (indexByName.hasOwnProperty(key)) {
                    list[indexByName[key]] = item;
                    updated++;
                } else {
                    indexByName[key] = list.length;
                    list.push(item);
                    added++;
                }
            }
            setSavedConnections(list);
            renderSavedConnections();
            bootstrap.Modal.getOrCreateInstance(modalEl).hide();
            alert('匯入完成：新增 ' + added + ' 筆，更新 ' + updated + ' 筆。');
        });
    }

    // 移除：清除 localStorage 中所有連線，需勾選確認。
    function setupRemove() {
        var btn = document.getElementById('removeConnBtn');
        var modalEl = document.getElementById('removeConnModal');
        if (!btn || !modalEl || typeof bootstrap === 'undefined') return;

        var confirmCheck = document.getElementById('removeConnConfirmCheck');
        var confirmBtn = document.getElementById('removeConnConfirmBtn');

        btn.addEventListener('click', function (e) {
            e.preventDefault();
            confirmCheck.checked = false;
            confirmBtn.disabled = true;
            bootstrap.Modal.getOrCreateInstance(modalEl).show();
        });

        confirmCheck.addEventListener('change', function () {
            confirmBtn.disabled = !confirmCheck.checked;
        });

        confirmBtn.addEventListener('click', function () {
            localStorage.removeItem(SAVED_CONN_KEY);
            renderSavedConnections();
            bootstrap.Modal.getOrCreateInstance(modalEl).hide();
            alert('已清除全部連線清單。');
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        renderSavedConnections();
        maybePromptRememberConnection();
        setupExport();
        setupImport();
        setupRemove();
    });
})();
