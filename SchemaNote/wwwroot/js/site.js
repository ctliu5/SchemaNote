var Overview = {};
//todo 英文大小寫皆可
function Iterator_jQuery(compareStr) {
    var htmlCollection = $('.accordion');
    if (compareStr) {
        for (var i = 0; i < htmlCollection.length; i++) {
            var ele = $(htmlCollection[i]).find('.TableName');
            if (ele.text().trim().indexOf(compareStr) > -1) {
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

function Iterator_js_ClassName(compareStr) {
    var htmlCollection = document.getElementsByClassName('accordion');
    if (compareStr) {
        for (var i = 0; i < htmlCollection.length; i++) {
            var ele = htmlCollection[i].getElementsByClassName('TableName')[0];
            if (ele.textContent.trim().indexOf(compareStr) > -1) {
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
    if (compareStr) {
        ForeachObj(Overview,
            function (obj, key) {
                if (key.indexOf(compareStr) > -1) {
                    document.getElementById(obj[key]).style.cssText = 'display:initial;';
                } else {
                    document.getElementById(obj[key]).style.cssText = 'display:none;';
                }
            }
        );
    } else {
        ForeachObj(Overview,
            function (obj, key) {
                document.getElementById(obj[key]).style.cssText = 'display:initial;';
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

function changeElement() {
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