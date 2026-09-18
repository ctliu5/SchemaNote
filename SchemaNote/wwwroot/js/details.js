/* =========================================================
   Details 頁面專用 JS
   - NoteField 雙擊改為可編輯欄位（含「編輯全部」）
   - 標籤（FLAGS）編輯器：Select2 tagging 風格的動態標籤建立
   Razor 注入資料改由 #flags-editor 的 data-* 屬性讀取。
   ========================================================= */

// 送出前將空字串轉回預設值（View 以 onsubmit 呼叫，需全域）。
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
    content.name = (sortNum > 0 ? '[' + sortNum + '].' : '[0].') + field;
    content.value = this.dataset.content;
    content.setAttribute('class', this.dataset.class + ' NoteController');
    if (EleType === "textarea")
        content.setAttribute('rows', 3);

    while (this.firstChild) {
        this.removeChild(this.firstChild);
    }

    this.insertBefore(content, this.childNodes[0]);

    content.select();

    document.getElementById('submit').style.cssText = 'display:initial;';
}

// 「編輯全部」：對所有 NoteField 觸發 dblclick（View 以 onclick 呼叫，需全域）。
function EditAll() {
    var htmlCollection = document.getElementsByClassName("NoteField");
    if (htmlCollection.length > 0) {
        var arr = Array.prototype.slice.call(htmlCollection);
        arr.forEach(
            function (item) {
                var doubleClickEvent = document.createEvent('MouseEvents');
                doubleClickEvent.initEvent('dblclick', true, true);
                item.dispatchEvent(doubleClickEvent);
            }
        )
        document.getElementById('EditAll').style.cssText = 'display:none;';
    }
}

// NoteField 雙擊綁定
(function () {
    var htmlCollection = document.getElementsByClassName("NoteField");
    if (htmlCollection.length > 0) {
        var arr = Array.prototype.slice.call(htmlCollection);
        arr.forEach(
            function (item) {
                item.addEventListener("dblclick", changeElement);
            }
        )
    }
})();

// 標籤（FLAGS）編輯器：Select2 tagging 風格的動態標籤建立（Vanilla JS）
(function () {
    var SEP = ';';
    var editor = document.getElementById('flags-editor');
    if (!editor) return;

    // Excel 工作表（Sheet）名稱限制：最多 N 字元，且不可包含特定特殊字元
    var MAX_LEN = parseInt(editor.getAttribute('data-max-len'), 10);
    var INVALID_CHARS = editor.getAttribute('data-invalid-chars') || '';
    // 資料庫中所有物件出現過的標籤，供下拉建議清單使用
    var ALL_FLAGS = JSON.parse(editor.getAttribute('data-all-flags'));

    var container = document.getElementById('flags-tags');
    var hidden = document.getElementById('flags-hidden');
    var input = document.getElementById('flags-new');
    var errorBox = document.getElementById('flags-error');
    var suggestBox = document.getElementById('flags-suggest');
    var activeIndex = -1;
    if (!container || !hidden || !input) return;

    function showError(msg) {
        if (errorBox) {
            errorBox.textContent = msg;
            errorBox.style.display = msg ? 'block' : 'none';
        }
    }
    function validateTag(val) {
        if (val.indexOf(SEP) > -1) {
            return '標籤不可包含分號（' + SEP + '）。';
        }
        for (var i = 0; i < INVALID_CHARS.length; i++) {
            if (val.indexOf(INVALID_CHARS[i]) > -1) {
                return '標籤（Excel 工作表名稱）不可包含下列字元：' + INVALID_CHARS;
            }
        }
        if (val.length > MAX_LEN) {
            return '標籤（Excel 工作表名稱）長度不可超過 ' + MAX_LEN + ' 個字元。';
        }
        return '';
    }

    function showSubmit() {
        var submit = document.getElementById('submit');
        if (submit) submit.style.cssText = 'display:initial;';
    }
    function currentValues() {
        var values = [];
        var chips = container.querySelectorAll('.tag-chip');
        for (var i = 0; i < chips.length; i++) {
            values.push(chips[i].getAttribute('data-value'));
        }
        return values;
    }
    function syncHidden() {
        hidden.value = currentValues().join(SEP);
        showSubmit();
    }
    function createChip(val) {
        var chip = document.createElement('span');
        chip.className = 'tag-chip';
        chip.setAttribute('data-value', val);
        var text = document.createElement('span');
        text.className = 'tag-chip-text';
        text.textContent = val;
        var remove = document.createElement('button');
        remove.type = 'button';
        remove.className = 'tag-chip-remove';
        remove.setAttribute('aria-label', 'remove');
        remove.innerHTML = '&times;';
        chip.appendChild(text);
        chip.appendChild(remove);
        return chip;
    }
    function addTag(rawVal) {
        // 保留標籤前後空白，僅忽略完全空字串（允許沒有標籤，空值靜默略過不報錯）
        var val = rawVal;
        if (!val) { showError(''); return false; }
        var err = validateTag(val);
        if (err) { showError(err); return false; }
        if (currentValues().indexOf(val) > -1) {
            showError('標籤已存在。');
            return false;
        }
        showError('');
        container.insertBefore(createChip(val), input);
        syncHidden();
        return true;
    }
    function removeLastTag() {
        var chips = container.querySelectorAll('.tag-chip');
        if (chips.length > 0) {
            container.removeChild(chips[chips.length - 1]);
            syncHidden();
        }
    }

    // ===== 下拉建議清單（Select2 風格）=====
    function hideSuggest() {
        if (suggestBox) { suggestBox.style.display = 'none'; suggestBox.innerHTML = ''; }
        activeIndex = -1;
    }
    function filteredFlags() {
        var selected = currentValues();
        var q = input.value.toUpperCase();
        return ALL_FLAGS.filter(function (f) {
            // 排除已選取的；依輸入文字過濾（大小寫不敏感）
            if (selected.indexOf(f) > -1) return false;
            return q === '' || f.toUpperCase().indexOf(q) > -1;
        });
    }
    function renderSuggest() {
        if (!suggestBox) return;
        var items = filteredFlags();
        if (items.length === 0) { hideSuggest(); return; }
        suggestBox.innerHTML = '';
        items.forEach(function (f, idx) {
            var item = document.createElement('div');
            item.className = 'tag-suggest-item';
            item.setAttribute('data-value', f);
            item.setAttribute('data-index', idx);
            item.textContent = f;
            // 用 mousedown 避免 input blur 先觸發導致點選失效
            item.addEventListener('mousedown', function (e) {
                e.preventDefault();
                if (addTag(f)) input.value = '';
                renderSuggest();
                input.focus();
            });
            suggestBox.appendChild(item);
        });
        activeIndex = -1;
        suggestBox.style.display = 'block';
    }
    function moveActive(delta) {
        var items = suggestBox ? suggestBox.querySelectorAll('.tag-suggest-item') : [];
        if (items.length === 0) return;
        if (activeIndex > -1 && items[activeIndex]) items[activeIndex].classList.remove('active');
        activeIndex += delta;
        if (activeIndex < 0) activeIndex = items.length - 1;
        if (activeIndex >= items.length) activeIndex = 0;
        items[activeIndex].classList.add('active');
        items[activeIndex].scrollIntoView({ block: 'nearest' });
    }

    // 點擊 ✕ 移除該標籤
    container.addEventListener('click', function (e) {
        if (e.target.classList.contains('tag-chip-remove')) {
            var chip = e.target.closest('.tag-chip');
            if (chip) { container.removeChild(chip); syncHidden(); renderSuggest(); }
        } else {
            input.focus();
        }
    });

    // 聚焦即顯示下拉建議
    input.addEventListener('focus', renderSuggest);
    // 輸入時即時過濾建議
    input.addEventListener('input', function () { showError(''); renderSuggest(); });

    // Enter/逗號建立標籤；上下鍵導航建議；Backspace 刪除最後一個標籤；Esc 關閉建議
    input.addEventListener('keydown', function (e) {
        var items = suggestBox ? suggestBox.querySelectorAll('.tag-suggest-item') : [];
        if (e.key === 'ArrowDown') {
            e.preventDefault(); moveActive(1);
        } else if (e.key === 'ArrowUp') {
            e.preventDefault(); moveActive(-1);
        } else if (e.key === 'Enter' || e.key === ',') {
            e.preventDefault();
            if (activeIndex > -1 && items[activeIndex]) {
                // 選取建議清單中被標記的項目
                if (addTag(items[activeIndex].getAttribute('data-value'))) input.value = '';
            } else if (addTag(input.value)) {
                input.value = '';
            }
            renderSuggest();
        } else if (e.key === 'Escape') {
            hideSuggest();
        } else if (e.key === 'Backspace' && input.value === '') {
            removeLastTag();
            renderSuggest();
        }
    });
    // 失焦時將殘留文字轉為標籤，並關閉建議
    input.addEventListener('blur', function () {
        if (addTag(input.value)) input.value = '';
        hideSuggest();
    });
})();
