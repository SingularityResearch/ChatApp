window.chatWysiwyg = {
    init: function (elementId, dotNetHelper) {
        var el = document.getElementById(elementId);
        if (!el) return;

        el.addEventListener('input', function () {
            dotNetHelper.invokeMethodAsync('UpdateContent', el.innerHTML);
        });

        el.addEventListener('blur', function () {
            dotNetHelper.invokeMethodAsync('UpdateContent', el.innerHTML);
        });
    },
    executeCommand: function (command, value) {
        document.execCommand(command, false, value);
    },
    setContent: function (elementId, content) {
        var el = document.getElementById(elementId);
        if (el && el.innerHTML !== content) {
            el.innerHTML = content;
        }
    },
    focus: function(elementId) {
        var el = document.getElementById(elementId);
        if (el) {
            el.focus();
            
            // Move cursor to end
            if (typeof window.getSelection !== "undefined"
                    && typeof document.createRange !== "undefined") {
                var range = document.createRange();
                range.selectNodeContents(el);
                range.collapse(false);
                var sel = window.getSelection();
                sel.removeAllRanges();
                sel.addRange(range);
            }
        }
    },
    clearContent: function (elementId) {
        var el = document.getElementById(elementId);
        if (el) {
            el.innerHTML = '';
        }
    },
    savedSelection: null,
    saveSelection: function() {
        if (window.getSelection) {
            var sel = window.getSelection();
            if (sel.getRangeAt && sel.rangeCount) {
                this.savedSelection = sel.getRangeAt(0);
            }
        } else if (document.selection && document.selection.createRange) {
            this.savedSelection = document.selection.createRange();
        }
    },
    restoreSelection: function() {
        if (this.savedSelection) {
            if (window.getSelection) {
                var sel = window.getSelection();
                sel.removeAllRanges();
                sel.addRange(this.savedSelection);
            } else if (document.selection && this.savedSelection.select) {
                this.savedSelection.select();
            }
        }
    },
    executeColorCommand: function(command, value) {
        this.restoreSelection();
        document.execCommand(command, false, value);
    }
};
