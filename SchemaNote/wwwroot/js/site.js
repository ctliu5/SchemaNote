function fuzzy(txt, compareStr) { return txt.indexOf(compareStr) > -1; }
function exact(txt, compareStr) { return txt === compareStr; }
var Overview = {}, CurrentIndex, CompareMethod = fuzzy, Iterator, SearchTextBox = document.getElementById('SearchTextBox');
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
    Iterator = Iterator_jQuery;
    $('[data-toggle="tooltip"]').tooltip();
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
};

function Iterator_jQuery() {
    var compareStr = typeof SearchTextBox.value === 'string' ? SearchTextBox.value.trim() : '';
    var className = '.' + CurrentIndex;
    compareStr = compareStr.toUpperCase();
    var htmlCollection = $('.accordion');
    if (compareStr) {
        var flag;
        for (var i = 0; i < htmlCollection.length; i++) {
            flag = true;
            $(htmlCollection[i]).find(className).each(function () {
                let text = $(this).text();
                if (text) {
                    if (CompareMethod(text.trim().toUpperCase(), compareStr)) {
                        $(htmlCollection[i]).css('display', 'initial');
                        flag = false;
                        return false;
                    }
                } else {
                    console.log($(this));
                }
            });
            if (flag) {
                $(htmlCollection[i]).css('display', 'none');
            }
        }
    } else {
        for (var i = 0; i < htmlCollection.length; i++) {
            $(htmlCollection[i]).css('display', 'initial');
        }
    }
}

function Iterator_js_querySelector() {
    var compareStr = typeof SearchTextBox.value === 'string' ? SearchTextBox.value.trim() : '';
    var className = '.' + CurrentIndex;
    compareStr = compareStr.toUpperCase();
    var htmlCollection = document.querySelectorAll('.accordion');
    if (compareStr) {
        var flag;
        for (var i = 0; i < htmlCollection.length; i++) {
            flag = true;
            var eles = htmlCollection[i].querySelectorAll(className);
            for (let ele of eles) {
                if (ele.textContent) {
                    if (CompareMethod(ele.textContent.trim().toUpperCase(), compareStr)) {
                        htmlCollection[i].style.cssText = 'display:initial;';
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
        for (var i = 0; i < htmlCollection.length; i++) {
            htmlCollection[i].style.cssText = 'display:initial;';
        }
    }
}

function Iterator_js_ClassName() {
    var compareStr = typeof SearchTextBox.value === 'string' ? SearchTextBox.value.trim() : '';
    compareStr = compareStr.toUpperCase();
    var htmlCollection = document.getElementsByClassName('accordion');
    if (compareStr) {
        var flag;
        for (var i = 0; i < htmlCollection.length; i++) {
            flag = true;
            var eles = htmlCollection[i].getElementsByClassName(CurrentIndex);
            for (let ele of eles) {
                if (ele.textContent) {
                    if (CompareMethod(ele.textContent.trim().toUpperCase(), compareStr)) {
                        htmlCollection[i].style.cssText = 'display:initial;';
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
        for (var i = 0; i < htmlCollection.length; i++) {
            htmlCollection[i].style.cssText = 'display:initial;';
        }
    }
}

function Iterator_js_JsonObj() {
    var compareStr = typeof SearchTextBox.value === 'string' ? SearchTextBox.value.trim() : '';
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