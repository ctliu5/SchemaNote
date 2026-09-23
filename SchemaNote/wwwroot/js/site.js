/* =========================================================
   site.js — 全站共用工具函式
   ========================================================= */

// 取得防偽權杖（由 _Layout.cshtml 的 <meta name="request-verification-token"> 提供）。
function getAntiforgeryToken() {
    var meta = document.querySelector('meta[name="request-verification-token"]');
    return meta ? meta.getAttribute('content') : '';
}

// 取得防偽權杖的 form 欄位名稱（由 AntiforgeryOptions.FormFieldName 設定，透過 meta 提供）。
function getAntiforgeryFieldName() {
    var meta = document.querySelector('meta[name="csrf-form-field-name"]');
    return (meta && meta.getAttribute('content')) || '__RequestVerificationToken';
}

// 取得防偽權杖的 request header 名稱（由 AntiforgeryOptions.HeaderName 設定，供 ajax/fetch 使用）。
function getAntiforgeryHeaderName() {
    var meta = document.querySelector('meta[name="csrf-header-name"]');
    return (meta && meta.getAttribute('content')) || 'RequestVerificationToken';
}

// 在動態建立的 form 中補上防偽權杖的 hidden 欄位，供 AutoValidateAntiforgeryToken 全域驗證。
function appendAntiforgeryToken(form) {
    var input = document.createElement('input');
    input.type = 'hidden';
    input.name = getAntiforgeryFieldName();
    input.value = getAntiforgeryToken();
    form.appendChild(input);
}

// 走訪物件/陣列的每個自有屬性（for...in 支援 break 與物件，forEach 不支援）。
function ForeachObj(obj, func) {
    for (var key in obj) {
        if (obj.hasOwnProperty(key)) {
            func(obj, key);
        }
    }
}

// 以純前端方式下載文字檔（加上 BOM 以利 Windows 正確辨識 UTF-8）。
// see https://stackoverflow.com/questions/17879198/adding-utf-8-bom-to-string-blob
function download(filename, text) {
    text = '\ufeff' + text;

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

// 關閉連線：確認後以 POST 方式送出，後端清除 Session 並導回首頁。
// Overview 與 Details 兩頁的「關閉連線」按鈕皆以 onclick="CloseConnection()" 呼叫。
window.CloseConnection = function () {
    if (!window.confirm('確定要關閉目前的連線嗎？')) {
        return;
    }
    var form = document.createElement('form');
    form.method = 'POST';
    form.action = '/Home/CloseConnection';
    form.style.display = 'none';
    appendAntiforgeryToken(form);
    document.body.appendChild(form);
    form.submit();
    document.body.removeChild(form);
};

// 連線方式切換：在「填寫欄位」與「連線字串」兩種表單間切換顯示。
document.addEventListener('DOMContentLoaded', function () {
    var fieldsForm = document.getElementById('connFieldsForm');
    var stringForm = document.getElementById('connStringForm');
    var radios = document.querySelectorAll('input[name="connMode"]');
    if (!fieldsForm || !stringForm || radios.length === 0) {
        return;
    }

    function applyMode(mode) {
        if (mode === 'string') {
            stringForm.classList.remove('d-none');
            fieldsForm.classList.add('d-none');
        } else {
            fieldsForm.classList.remove('d-none');
            stringForm.classList.add('d-none');
        }
    }

    radios.forEach(function (radio) {
        radio.addEventListener('change', function () {
            if (radio.checked) {
                applyMode(radio.value);
            }
        });
    });

    var checked = document.querySelector('input[name="connMode"]:checked');
    applyMode(checked ? checked.value : 'fields');
});

