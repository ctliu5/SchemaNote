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

    document.addEventListener('DOMContentLoaded', function () {
        renderSavedConnections();
        maybePromptRememberConnection();
    });
})();
