window.scrollToBottom = (elementId) => {
    var element = document.getElementById(elementId);
    if (element) {
        element.scrollTop = element.scrollHeight;
    }
};

window.positionEmojiPicker = (buttonId, pickerId) => {
    var btn = document.getElementById(buttonId);
    var picker = document.getElementById(pickerId);
    if (!btn || !picker) return;

    var rect = btn.getBoundingClientRect();
    var vw = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
    var vh = Math.max(document.documentElement.clientHeight || 0, window.innerHeight || 0);

    // Initial setup for measurement
    picker.style.position = 'fixed';
    picker.style.margin = '0';
    picker.style.bottom = 'auto';
    picker.style.right = 'auto';
    picker.style.visibility = 'hidden';
    picker.style.display = 'block';

    let top = rect.bottom + 5;
    let left = rect.left;

    picker.style.top = top + 'px';
    picker.style.left = left + 'px';

    var pRect = picker.getBoundingClientRect();

    // Horizontal check
    if (pRect.right > vw) {
        left = vw - pRect.width - 10;
        if (left < 10) {
            left = 10;
            picker.style.width = (vw - 20) + 'px';
        }
    }

    // Vertical check
    if (pRect.bottom > vh) {
        top = rect.top - pRect.height - 5;
        if (top < 10) top = 10;
    }

    picker.style.top = top + 'px';
    picker.style.left = left + 'px';
    picker.style.visibility = 'visible';
};
