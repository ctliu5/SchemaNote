function fuzzy(txt, compareStr) { return txt.indexOf(compareStr) > -1; }
function exact(txt, compareStr) { return txt === compareStr; }
var Overview = {}, CurrentIndex, CompareMethod = fuzzy
function SetIndex() { CurrentIndex = this.value; }
function SetMethod() {
    switch (this.value) {
        case "fuzzy": CompareMethod = fuzzy; break;
        case "exact": CompareMethod = exact; break;
        default: CompareMethod = fuzzy; break;
    }
}

function initialOption() {
    $('[data-toggle="tooltip"]').tooltip();
    var chooseUl = document.getElementById('choose');
    chooseUl.addEventListener("change", SetIndex, false);
    var i = 0;
    for (var cName in Overview) {
        if (++i == 1)
            CurrentIndex = cName;
        if (Overview.hasOwnProperty(cName)) {
            var option = document.createElement('option');
            var textnode = document.createTextNode(Overview[cName].name_cht);
            option.appendChild(textnode);
            option.setAttribute("value", cName);
            chooseUl.appendChild(option);
        }
    };
    var chooseUl = document.getElementById('switch');
    chooseUl.addEventListener("change", SetMethod, false);
};

function Iterator_jQuery(compareStr, className) {
    className = '.' + className;
    compareStr = compareStr.toUpperCase();
    var htmlCollection = $('.accordion');
    if (compareStr) {
        for (var i = 0; i < htmlCollection.length; i++) {
            var ele = $(htmlCollection[i]).find(className);
            if (CompareMethod(ele.text().trim().toUpperCase(), compareStr)) {
                $(htmlCollection[i]).css('display', 'initial');
            } else {
                $(htmlCollection[i]).css('display', 'none');
            }
        }
    } else {
        for (var i = 0; i < htmlCollection.length; i++) {
            $(htmlCollection[i]).css('display', 'initial');
        }
    }
}

function Iterator_js_querySelector(compareStr, className) {
    className = '.' + className;
    compareStr = compareStr.toUpperCase();
    var htmlCollection = document.querySelectorAll('.accordion');
    if (compareStr) {
        for (var i = 0; i < htmlCollection.length; i++) {
            var ele = htmlCollection[i].querySelector(className);
            if (CompareMethod(ele.textContent.trim().toUpperCase(), compareStr)) {
                htmlCollection[i].style.cssText = 'display:initial;';
            } else {
                htmlCollection[i].style.cssText = 'display:none;';
            }
        }
    } else {
        for (var i = 0; i < htmlCollection.length; i++) {
            htmlCollection[i].style.cssText = 'display:initial;';
        }
    }
}

function Iterator_js_ClassName(compareStr, className) {
    compareStr = compareStr.toUpperCase();
    var htmlCollection = document.getElementsByClassName('accordion');
    if (compareStr) {
        for (var i = 0; i < htmlCollection.length; i++) {
            var ele = htmlCollection[i].getElementsByClassName(className)[0];
            if (CompareMethod(ele.textContent.trim().toUpperCase(), compareStr)) {
                htmlCollection[i].style.cssText = 'display:initial;';
            } else {
                htmlCollection[i].style.cssText = 'display:none;';
            }
        }
    } else {
        for (var i = 0; i < htmlCollection.length; i++) {
            htmlCollection[i].style.cssText = 'display:initial;';
        }
    }
}

function Iterator_js_JsonObj(compareStr) {
    compareStr = compareStr.toUpperCase();
    if (compareStr) {
        ForeachObj(Overview[CurrentIndex].json,
            function (obj, key) {
                if (CompareMethod(obj[key], compareStr)) {
                    document.getElementById(key).style.cssText = 'display:initial;';
                } else {
                    document.getElementById(key).style.cssText = 'display:none;';
                }
            }
        );
    } else {
        ForeachObj(Overview[CurrentIndex].json,
            function (obj, key) {
                document.getElementById(key).style.cssText = 'display:initial;';
            }
        );
    }
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

function changeElement(e) {
    this.removeEventListener("dblclick", changeElement, false);
    var columnID = this.dataset.column_id;
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
    content.name = (columnID > 0 ? '[' + columnID + '].' : '[0].') + field;
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

function ExportExtendedPropScript() {
    $.ajax({
        url: "/Home/ExportExtendedPropScript",
        type: "POST",
        dataType: "text",
        contentType: "text/plain;charset=UTF-8",
        //contentType: "text/plain;charset=UTF-16LE",
        success: function (data, textStatus, jqXHR) {
            download('ExtendedPropScript_urlencoded_' + Date.now() + '.sql', data);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            //todo
        }
    })
}

function download(filename, text) {

    text = '\ufeff' + text; //for windows OS, convert『UTF-8』 to 『UTF-8 with bom』,see https://stackoverflow.com/questions/17879198/adding-utf-8-bom-to-string-blob

    var blob = new Blob([text], { type: 'text/plain;charset=UTF-8' });
    //var blob = new Blob([text], { type: 'text/plain;charset=UTF-16LE' });
    var url = window.URL.createObjectURL(blob);

    var element = document.createElement('a');

    element.setAttribute('href', url);
    element.setAttribute('download', filename);

    element.style.display = 'none';
    document.body.appendChild(element);

    element.click();

    document.body.removeChild(element);
}