/* =========================================================
   Overview 頁面專用 JS
   - 搜尋（模糊/精確）與搜尋項目切換
   - 標籤（FLAGS）過濾（Dropdown 核取 + 徽章）
   - 匯出（Excel / Markdown / 擴充屬性 SQL）
   依賴 site.js 提供的共用工具：ForeachObj。
   Razor 注入資料改由 #overview-data 的 data-* 屬性讀取。
   ========================================================= */
(function () {
    function fuzzy(txt, compareStr) { return txt.indexOf(compareStr) > -1; }
    function exact(txt, compareStr) { return txt === compareStr; }

    var Overview = {};
    var CurrentIndex;
    var CompareMethod = fuzzy;
    var Iterator;
    var SearchTextBox;

    // 標籤過濾：已選標籤（大寫）集合，以及各 accordion 對應的標籤（大寫）對照表
    var OverviewFlags = { byAccordion: {}, all: [] };
    var SelectedFlags = [];
    var FlagUpperByAccordion = {};
    var NoneFlagLabel = '';
    var ExportModal;
    var CurrentExportUrl = '';
    var CurrentExportMode = '';
    var ExportBaseItems = [];
    var ExportTableCandidates = [];

    function SetIndex() {
        CurrentIndex = this.value; Iterator();
    }
    function SetMethod() {
        switch (this.value) {
            case "fuzzy": CompareMethod = fuzzy; break;
            case "exact": CompareMethod = exact; break;
            default: CompareMethod = fuzzy; break;
        }
        Iterator();
    }

    // 由 #overview-data 的 data-* 屬性讀取伺服器注入的資料
    function loadServerData() {
        var el = document.getElementById('overview-data');
        if (!el) return;
        Overview = {
            'objName': { 'name_cht': el.getAttribute('data-objname-label'), 'json': JSON.parse(el.getAttribute('data-objname-json')) },
            'colName': { 'name_cht': el.getAttribute('data-colname-label'), 'json': JSON.parse(el.getAttribute('data-colname-json')) },
            'description': { 'name_cht': el.getAttribute('data-description-label'), 'json': JSON.parse(el.getAttribute('data-description-json')) },
            'remark': { 'name_cht': el.getAttribute('data-remark-label'), 'json': JSON.parse(el.getAttribute('data-remark-json')) }
        };
        OverviewFlags = {
            'byAccordion': JSON.parse(el.getAttribute('data-flags')),
            'all': JSON.parse(el.getAttribute('data-allflags'))
        };
        NoneFlagLabel = el.getAttribute('data-none-flag') || '';
    }

    function initialOption() {
        Iterator = Iterator_js_JsonObj;
        // Bootstrap 5 原生 Tooltip 初始化（不再依賴 jQuery）
        if (window.bootstrap && bootstrap.Tooltip) {
            var tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
            Array.prototype.slice.call(tooltipTriggerList).forEach(function (el) {
                new bootstrap.Tooltip(el);
            });
        }
        var i = 0;
        var choose = document.getElementById('choose');
        choose.addEventListener("change", SetIndex, false);
        for (var cName in Overview) {
            if (++i == 1)
                CurrentIndex = cName;
            if (Overview.hasOwnProperty(cName)) {
                var option = document.createElement('option');
                var textnode = document.createTextNode(Overview[cName].name_cht);
                option.appendChild(textnode);
                option.setAttribute("value", cName);
                choose.appendChild(option);
            }
        }
        var myswitch = document.getElementById('switch');
        myswitch.addEventListener("change", SetMethod, false);
        SearchTextBox = document.getElementById('SearchTextBox');
        SearchTextBox.addEventListener("keyup", Iterator, false);
    }

    function Iterator_js_JsonObj() {
        var compareStr = typeof SearchTextBox.value === 'string' ? SearchTextBox.value.trim() : '';
        compareStr = compareStr.toUpperCase();
        if (compareStr) {
            var flag;
            ForeachObj(Overview[CurrentIndex].json,
                function (obj, key) {
                    flag = true;
                    var o = obj[key];
                    for (var i = 0, len = o.length; flag && i < len; i++) {
                        if (o[i]) {
                            if (CompareMethod(o[i], compareStr)) {
                                flag = false;
                            }
                        } else {
                            console.log(o);
                            console.log(o[i]);
                        }
                    }
                    // flag=false 代表文字符合；再套用標籤過濾（OR）
                    if (!flag && MatchFlags(key)) {
                        document.getElementById(key).style.cssText = 'display:block;';
                    } else {
                        document.getElementById(key).style.cssText = 'display:none;';
                    }
                }
            );
        } else {
            ForeachObj(Overview[CurrentIndex].json,
                function (obj, key) {
                    document.getElementById(key).style.cssText = MatchFlags(key) ? 'display:block;' : 'display:none;';
                }
            );
        }
    }

    // 標籤過濾判斷：未選標籤時全部符合；已選標籤採 OR（含任一即符合）
    function MatchFlags(accordionKey) {
        if (!SelectedFlags || SelectedFlags.length === 0) return true;
        var flags = FlagUpperByAccordion[accordionKey] || [];
        for (var i = 0; i < SelectedFlags.length; i++) {
            if (flags.indexOf(SelectedFlags[i]) > -1) return true;
        }
        return false;
    }

    // 初始化 Overview 標籤過濾下拉選單（徽章 + Dropdown 核取）
    function initialFlagsFilter() {
        // 建立各 accordion 的大寫標籤對照表
        FlagUpperByAccordion = {};
        ForeachObj(OverviewFlags.byAccordion, function (obj, key) {
            FlagUpperByAccordion[key] = (obj[key] || []).map(function (f) { return f.toUpperCase(); });
        });

        var menu = document.getElementById('flags-menu');
        if (!menu) return;
        var all = OverviewFlags.all || [];
        if (all.length === 0) {
            menu.innerHTML = '<span class="dropdown-item-text text-muted">（無標籤）</span>';
            return;
        }
        all.forEach(function (flag, idx) {
            var id = 'flag-chk-' + idx;
            var wrapper = document.createElement('div');
            wrapper.className = 'form-check';
            var chk = document.createElement('input');
            chk.className = 'form-check-input';
            chk.type = 'checkbox';
            chk.id = id;
            chk.value = flag;
            chk.addEventListener('change', onFlagFilterChange);
            var label = document.createElement('label');
            label.className = 'form-check-label';
            label.setAttribute('for', id);
            label.textContent = flag;
            wrapper.appendChild(chk);
            wrapper.appendChild(label);
            menu.appendChild(wrapper);
        });
    }

    function onFlagFilterChange() {
        var menu = document.getElementById('flags-menu');
        var checks = menu.querySelectorAll('input[type="checkbox"]');
        SelectedFlags = [];
        var selectedLabels = [];
        for (var i = 0; i < checks.length; i++) {
            if (checks[i].checked) {
                SelectedFlags.push(checks[i].value.toUpperCase());
                selectedLabels.push(checks[i].value);
            }
        }
        renderSelectedFlagBadges(selectedLabels);
        if (Iterator) Iterator();
    }

    function renderSelectedFlagBadges(labels) {
        var container = document.getElementById('flags-selected');
        if (!container) return;
        container.innerHTML = '';
        labels.forEach(function (label) {
            var badge = document.createElement('span');
            badge.className = 'badge bg-primary d-inline-flex align-items-center';
            badge.textContent = label;
            var close = document.createElement('button');
            close.type = 'button';
            close.className = 'btn-close btn-close-white ms-1';
            close.style.fontSize = '.6rem';
            close.setAttribute('aria-label', 'remove');
            close.addEventListener('click', function () {
                var menu = document.getElementById('flags-menu');
                var checks = menu.querySelectorAll('input[type="checkbox"]');
                for (var i = 0; i < checks.length; i++) {
                    if (checks[i].value === label) { checks[i].checked = false; break; }
                }
                onFlagFilterChange();
            });
            badge.appendChild(close);
            container.appendChild(badge);
        });
    }

    /* ===== 匯出 ===== */

    // 透過動態建立 form 送出 POST，讓瀏覽器原生處理檔案下載。
    // 後端以檔案（FileResult）回傳並帶 Content-Disposition，可正確處理文字與二進位檔案。
    function postDownload(url) {
        postForm(url);
    }

    // 動態建立 form 並以 POST 送出，帶上目前搜尋/過濾後仍顯示的 Table/View 之 OBJECT_ID。
    function postForm(url, selectedObjectIds) {
        var form = document.createElement('form');
        form.method = 'POST';
        form.action = url;
        form.style.display = 'none';

        // 帶上防偽權杖，供後端 AutoValidateAntiforgeryToken 全域驗證。
        appendAntiforgeryToken(form);

        // 收集目前搜尋/過濾後仍顯示的 Table/View 之 OBJECT_ID，僅處理這些項目。
        var objectIds = selectedObjectIds === undefined ? getVisibleObjectIds() : selectedObjectIds;
        if (selectedObjectIds !== undefined && objectIds.length === 0) return;
        for (var i = 0; i < objectIds.length; i++) {
            var input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'objectIds';
            input.value = objectIds[i];
            form.appendChild(input);
        }

        document.body.appendChild(form);
        form.submit();
        document.body.removeChild(form);
    }

    // 取得目前畫面上（未被搜尋/標籤過濾隱藏）的 accordion 對應之 OBJECT_ID 陣列。
    function getVisibleObjectIds() {
        var ids = [];
        var accordions = document.querySelectorAll('.accordion[data-object-id]');
        for (var i = 0; i < accordions.length; i++) {
            var ele = accordions[i];
            if (ele.style.display !== 'none') {
                var id = ele.getAttribute('data-object-id');
                if (id !== null && id !== '') {
                    ids.push(id);
                }
            }
        }
        return ids;
    }

    function getVisibleObjectItems() {
        var items = [];
        var accordions = document.querySelectorAll('.accordion[data-object-id]');
        for (var i = 0; i < accordions.length; i++) {
            var element = accordions[i];
            if (element.style.display === 'none') continue;
            var nameElement = element.querySelector('.objName');
            var name = nameElement ? nameElement.textContent.trim() : '';
            var schema = element.getAttribute('data-object-schema') || '';
            var flags = OverviewFlags.byAccordion[element.id] || [];
            items.push({
                id: element.getAttribute('data-object-id'),
                name: schema ? schema + '.' + name : name,
                type: element.getAttribute('data-object-type') || '',
                flags: flags
            });
        }
        return items;
    }

    function initialExportModal() {
        var modalElement = document.getElementById('exportFilterModal');
        if (!modalElement || !window.bootstrap || !bootstrap.Modal) return;
        ExportModal = bootstrap.Modal.getOrCreateInstance(modalElement);

        document.getElementById('exportDownloadAll').addEventListener('click', function () {
            ExportModal.hide();
            postDownload(CurrentExportUrl);
        });
        document.getElementById('exportExcludeSelected').addEventListener('click', function () {
            beginExportFiltering('exclude');
        });
        document.getElementById('exportIncludeSelected').addEventListener('click', function () {
            beginExportFiltering('include');
        });
        document.getElementById('exportStepBack').addEventListener('click', function () {
            if (!document.getElementById('exportTablesStep').classList.contains('d-none')) {
                showExportStep('flags');
            } else {
                showExportStep('mode');
            }
        });
        document.getElementById('exportStepNext').addEventListener('click', moveToExportTablesStep);
        document.getElementById('exportStepConfirm').addEventListener('click', confirmFilteredExport);
    }

    function beginExportFiltering(mode) {
        CurrentExportMode = mode;
        ExportBaseItems = getVisibleObjectItems();
        document.getElementById('exportFlagsStepTitle').textContent = mode === 'include'
            ? '步驟 1/2：選擇要包含的標籤'
            : '步驟 1/2：選擇要排除的標籤';
        renderExportFlags();
        showExportStep('flags');
    }

    function showExportStep(step) {
        document.getElementById('exportModeStep').classList.toggle('d-none', step !== 'mode');
        document.getElementById('exportFlagsStep').classList.toggle('d-none', step !== 'flags');
        document.getElementById('exportTablesStep').classList.toggle('d-none', step !== 'tables');
        document.getElementById('exportStepBack').classList.toggle('d-none', step === 'mode');
        document.getElementById('exportStepNext').classList.toggle('d-none', step !== 'flags');
        document.getElementById('exportStepConfirm').classList.toggle('d-none', step !== 'tables');

        var title = document.getElementById('exportFilterModalLabel');
        if (step === 'mode') {
            title.textContent = '匯出選項';
        } else if (step === 'flags') {
            title.textContent = '選擇標籤';
        } else {
            title.textContent = '選擇資料表格';
        }
    }

    function renderExportFlags() {
        var container = document.getElementById('exportFlagsOptions');
        container.innerHTML = '';
        var flags = [];
        var seenFlags = [];
        var noneUpper = NoneFlagLabel.toUpperCase();

        ExportBaseItems.forEach(function (item) {
            item.flags.forEach(function (flag) {
                var upper = flag.toUpperCase();
                if (upper !== noneUpper && seenFlags.indexOf(upper) === -1) {
                    seenFlags.push(upper);
                    flags.push(flag);
                }
            });
        });
        flags.sort(function (a, b) { return a.localeCompare(b); });
        flags.unshift(NoneFlagLabel);

        flags.forEach(function (flag, index) {
            appendExportCheckbox(container, 'export-flag-' + index, flag, flag, flag.toUpperCase() === noneUpper);
        });
    }

    function appendExportCheckbox(container, id, value, labelText, isNoneFlag) {
        var wrapper = document.createElement('div');
        wrapper.className = 'form-check';
        var checkbox = document.createElement('input');
        checkbox.className = 'form-check-input';
        checkbox.type = 'checkbox';
        checkbox.id = id;
        checkbox.value = value;
        if (isNoneFlag) checkbox.dataset.noneFlag = 'true';
        var label = document.createElement('label');
        label.className = 'form-check-label';
        label.setAttribute('for', id);
        label.textContent = labelText;
        wrapper.appendChild(checkbox);
        wrapper.appendChild(label);
        container.appendChild(wrapper);
    }

    function getSelectedExportFlags() {
        var checks = document.querySelectorAll('#exportFlagsOptions input[type="checkbox"]:checked');
        var selected = [];
        for (var i = 0; i < checks.length; i++) {
            selected.push({
                value: checks[i].value.toUpperCase(),
                isNone: checks[i].dataset.noneFlag === 'true'
            });
        }
        return selected;
    }

    function matchesExportFlags(item, selectedFlags) {
        for (var i = 0; i < selectedFlags.length; i++) {
            if (selectedFlags[i].isNone) {
                if (item.flags.length === 0 || item.flags.some(function (flag) {
                    return flag.toUpperCase() === NoneFlagLabel.toUpperCase();
                })) return true;
            } else if (item.flags.some(function (flag) {
                return flag.toUpperCase() === selectedFlags[i].value;
            })) {
                return true;
            }
        }
        return false;
    }

    function moveToExportTablesStep() {
        var selectedFlags = getSelectedExportFlags();
        if (selectedFlags.length === 0) {
            window.alert('請至少選擇一個標籤。');
            return;
        }

        ExportTableCandidates = ExportBaseItems.filter(function (item) {
            var matches = matchesExportFlags(item, selectedFlags);
            return !matches;
        });
        renderExportTables();
        document.getElementById('exportTablesStepTitle').textContent = CurrentExportMode === 'include'
            ? '步驟 2/2：選擇要額外包含的資料表格'
            : '步驟 2/2：選擇要排除的資料表格';
        showExportStep('tables');
    }

    function renderExportTables() {
        var container = document.getElementById('exportTablesOptions');
        var emptyMessage = document.getElementById('exportNoTablesMessage');
        container.innerHTML = '';
        emptyMessage.textContent = CurrentExportMode === 'include'
            ? '沒有未選標籤下的資料表格；仍會匯出已選標籤下的資料表格。'
            : '排除所選標籤後，沒有剩餘的資料表格。';
        emptyMessage.classList.toggle('d-none', ExportTableCandidates.length !== 0);

        ExportTableCandidates.forEach(function (item, index) {
            var label = item.name;
            if (item.type) label += '（' + item.type + '）';
            appendExportCheckbox(container, 'export-table-' + index, item.id, label, false);
        });
    }

    function confirmFilteredExport() {
        var checks = document.querySelectorAll('#exportTablesOptions input[type="checkbox"]:checked');
        var selectedIds = [];
        for (var i = 0; i < checks.length; i++) {
            selectedIds.push(checks[i].value);
        }

        var candidateIds = ExportTableCandidates.map(function (item) { return item.id; });
        var objectIds;
        if (CurrentExportMode === 'include') {
            var selectedFlags = getSelectedExportFlags();
            var selectedFlagIds = ExportBaseItems
                .filter(function (item) { return matchesExportFlags(item, selectedFlags); })
                .map(function (item) { return item.id; });
            objectIds = selectedFlagIds.concat(selectedIds);
        } else {
            objectIds = candidateIds.filter(function (id) { return selectedIds.indexOf(id) === -1; });
        }
        if (objectIds.length === 0) {
            window.alert('目前條件沒有可匯出的資料表格。');
            return;
        }

        ExportModal.hide();
        postForm(CurrentExportUrl, objectIds);
    }

    function openExportOptions(url) {
        if (!ExportModal) return;
        CurrentExportUrl = url;
        CurrentExportMode = '';
        showExportStep('mode');
        ExportModal.show();
    }

    // View 中以 onclick 呼叫，需掛在全域。
    window.ExportExtendedPropScript = function () { openExportOptions('/Home/ExportExtendedPropScript'); };
    window.ExportMarkdown = function () { openExportOptions('/Home/ExportMarkdown'); };
    window.ExportExcel = function () { openExportOptions('/Home/ExportExcel'); };

    // 刪除目前畫面上（搜尋/標籤過濾後仍顯示）的物件之所有擴充屬性。破壞性操作，先確認。
    window.DropAllExtendedProps = function () {
        if (!window.confirm('確定要刪除目前顯示的所有物件之全部擴充屬性嗎？此動作無法復原。')) {
            return;
        }
        postForm('/Home/DropAllExtendedProps');
    };

    // 僅刪除目前畫面上（搜尋/標籤過濾後仍顯示）的物件之所有標籤（FLAGS）擴充屬性。破壞性操作，先確認。
    window.DropAllFlags = function () {
        if (!window.confirm('確定要刪除目前顯示的所有物件之全部標籤嗎？此動作無法復原。')) {
            return;
        }
        postForm('/Home/DropAllFlags');
    };

    window.addEventListener('load', function () {
        loadServerData();
        initialOption();
        initialFlagsFilter();
        initialExportModal();
    });
})();
