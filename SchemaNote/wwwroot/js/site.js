var Overview = {}, CurrentIndex;

function SetIndex() { CurrentIndex = this.value; }

function Iterator_jQuery(compareStr, className) {
    className = '.' + className;
    compareStr = compareStr.toUpperCase();
    var htmlCollection = $('.accordion');
    if (compareStr) {
        for (var i = 0; i < htmlCollection.length; i++) {
            var ele = $(htmlCollection[i]).find(className);
            if (ele.text().trim().toUpperCase().indexOf(compareStr) > -1) {
                $(htmlCollection[i]).css('display','initial');
            } else {
                $(htmlCollection[i]).css('display','none');
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
            if (ele.textContent.trim().toUpperCase().indexOf(compareStr) > -1) {
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
            if (ele.textContent.trim().toUpperCase().indexOf(compareStr) > -1) {
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

function Iterator_js_JsonObj(compareStr, index) {
    compareStr = compareStr.toUpperCase();
    if (compareStr) {
        ForeachObj(Overview[index].json,
            function (obj, key) {
                if (obj[key].indexOf(compareStr) > -1) {
                    document.getElementById(key).style.cssText = 'display:initial;';
                } else {
                    document.getElementById(key).style.cssText = 'display:none;';
                }
            }
        );
    } else {
        ForeachObj(Overview[index].json,
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

    this.removeChild(this.childNodes[0]);
    this.insertBefore(content, this.childNodes[0]);

    document.getElementById('submit').style.cssText = 'display:initial;';
}