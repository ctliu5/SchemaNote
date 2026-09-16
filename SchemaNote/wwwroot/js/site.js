function fuzzy(txt, compareStr) { return txt.indexOf(compareStr) > -1; }
function exact(txt, compareStr) { return txt === compareStr; }
var Overview = {}, CurrentIndex, CompareMethod = fuzzy, Iterator, SearchTextBox = document.getElementById('SearchTextBox')/*, counting*/;
// 標籤過濾（Overview）：已選標籤（大寫）集合，以及各 accordion 對應的標籤（大寫）對照表
var OverviewFlags = { byAccordion: {}, all: [] };
var SelectedFlags = [];
var FlagUpperByAccordion = {};
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
    };
    var myswitch = document.getElementById('switch');
    myswitch.addEventListener("change", SetMethod, false);
    var SearchTextBox = document.getElementById('SearchTextBox');
    SearchTextBox.addEventListener("keyup", Iterator, false);

    //效能比較
    /*testIterator();*/
};

function testIterator() {
    SearchTextBox.value = 'TBL';
    function test(func, cName) {
        var d = Date.now();
        CurrentIndex = cName;
        for (var i = 300; i > 0; i--) {
            func();
        }
        console.log(func.name + " spend time:" + (Date.now() - d));
    }
    for (var cName in Overview) {
        if (Overview.hasOwnProperty(cName)) {
            test(Iterator_js_querySelector, cName);// 3nd
            test(Iterator_js_ClassName, cName);// 2rd
            test(Iterator_js_JsonObj, cName);// 1st
            console.log('================================');
        }
    }
}

function Iterator_js_querySelector() {
    //counting = 0;
    var compareStr = typeof SearchTextBox.value === 'string' ? SearchTextBox.value.trim() : '';
    var className = '.' + CurrentIndex;
    compareStr = compareStr.toUpperCase();
    var htmlCollection = document.querySelectorAll('.accordion');
    if (compareStr) {
        var flag;
        for (var i = 0, length = htmlCollection.length; i < length; i++) {
            flag = true;
            var eles = htmlCollection[i].querySelectorAll(className);
            //for (var ele of eles) {
            for (var j = 0, len = eles.length; j < len; j++) {
                var ele = eles[j];
                if (ele.textContent) {
                    //counting++;
                    if (CompareMethod(ele.textContent.trim().toUpperCase(), compareStr)) {
                        htmlCollection[i].style.cssText = 'display:block;';
                        flag = false;
                        break;
                    }
                } else {
                    console.log(eles);
                    console.log(ele);
                }
            }
            if (flag) {
                htmlCollection[i].style.cssText = 'display:none;';
            }
        }
    } else {
        for (var i = 0, length = htmlCollection.length; i < length; i++) {
            htmlCollection[i].style.cssText = 'display:block;';
        }
    }
    //console.log(counting);
}

function Iterator_js_ClassName() {
    //counting = 0;
    var compareStr = typeof SearchTextBox.value === 'string' ? SearchTextBox.value.trim() : '';
    compareStr = compareStr.toUpperCase();
    var htmlCollection = document.getElementsByClassName('accordion');
    if (compareStr) {
        var flag;
        for (var i = 0, length = htmlCollection.length; i < length; i++) {
            flag = true;
            var eles = htmlCollection[i].getElementsByClassName(CurrentIndex);
            //for (var ele of eles) {
            for (var j = 0, len = eles.length; j < len; j++) {
                if (ele.textContent) {
                    //counting++;
                    if (CompareMethod(ele.textContent.trim().toUpperCase(), compareStr)) {
                        htmlCollection[i].style.cssText = 'display:block;';
                        flag = false;
                        break;
                    }
                } else {
                    console.log(eles);
                    console.log(ele);
                }
            }
            if (flag) {
                htmlCollection[i].style.cssText = 'display:none;';
            }
        }
    } else {
        for (var i = 0, length = htmlCollection.length; i < length; i++) {
            htmlCollection[i].style.cssText = 'display:block;';
        }
    }
    //console.log(counting);
}

function Iterator_js_JsonObj() {
    //counting = 0;
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
                        //counting++;
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
    //console.log(counting);
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
    if (typeof OverviewFlags === 'undefined') return;
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

function ForeachObj(obj, func) {
    //for...in... support break statement, forEach does not.
    //for...in... support object and array, forEach only for array.
    for (var key in obj) {
        if (obj.hasOwnProperty(key)) {
            func(obj, key);
        }
    }
}

function EmptyString(DefaultValue) {
    // Change empty string into default value!
    /*
    var htmlCollection = document.getElementsByClassName('NoteController');
    if (htmlCollection.length > 0) {
        var arr = Array.prototype.slice.call(htmlCollection);
        arr.forEach(
            function (item) {
                if (item.value === "")
                    item.value = DefaultValue;
            }
        )
    }
    */
}

function changeElement(e) {
    this.removeEventListener("dblclick", changeElement, false);
    //var columnID = this.dataset.column_id;
    var sortNum = this.dataset.sortnum;
    var field = this.dataset.field;
    var EleType = '';
    switch (field) {
        case 'REMARK':
            EleType = "textarea";
            break;
        default:
            EleType = "input";
            break;
    }

    var content = document.createElement(EleType);
    //content.name = (columnID > 0 ? '[' + columnID + '].' : '[0].') + field;
    content.name = (sortNum > 0 ? '[' + sortNum + '].' : '[0].') + field;
    content.value = this.dataset.content;
    content.setAttribute('class', this.dataset.class + ' NoteController');
    if (EleType === "textarea")
        content.setAttribute('rows', 3);

    while (this.firstChild) {
        this.removeChild(this.firstChild);
    }

    this.insertBefore(content, this.childNodes[0]);

    content/*.focus()*/.select();

    document.getElementById('submit').style.cssText = 'display:initial;';
}

// 透過動態建立 form 送出 POST，讓瀏覽器原生處理檔案下載。
// 後端以檔案（FileResult）回傳並帶 Content-Disposition，可正確處理文字與二進位檔案。
function postDownload(url) {
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = url;
    form.style.display = 'none';

    // 收集目前搜尋/過濾後仍顯示的 Table/View 之 OBJECT_ID，僅匯出這些項目。
    var objectIds = getVisibleObjectIds();
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

function ExportExtendedPropScript() {
    postDownload('/Home/ExportExtendedPropScript');
}

function ExportMarkdown() {
    postDownload('/Home/ExportMarkdown');
}

function ExportExcel() {
    postDownload('/Home/ExportExcel');
}

function download(filename, text) {

    text = '\ufeff' + text; //for windows OS, convert『UTF-8』 to 『UTF-8 with bom』,see https://stackoverflow.com/questions/17879198/adding-utf-8-bom-to-string-blob

    // var url = 'data:text/plain;charset=UTF-8,' + encodeURIComponent(text);
    var blob = new Blob([text], { type: 'text/plain;charset=UTF-8' });
    var url = window.URL.createObjectURL(blob);

    var element = document.createElement('a');

    element.setAttribute('href', url);
    element.setAttribute('download', filename);

    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);
}

/* =========================================================
   已儲存的 DB 連線（localStorage）
   - 儲存成功連線並命名
   - 進入 Overview（GET）時，若目前連線不在清單則彈出「記住DB連線」視窗
   - 於輸入區旁列出已存連線，可直接連線或刪除
   ========================================================= */
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
