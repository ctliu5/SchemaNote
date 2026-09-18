/* =========================================================
   site.js — 全站共用工具函式
   ========================================================= */

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
