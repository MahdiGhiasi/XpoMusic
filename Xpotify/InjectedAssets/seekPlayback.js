
var percentage = Number.parseFloat('{{percentage}}');

var rect = progressBar.getBoundingClientRect();
var x = rect.left + rect.width * percentage;
var y = rect.top + rect.height / 2;

var mouseDownEvent = document.createEvent('MouseEvents');
mouseDownEvent.initMouseEvent(
    'mousedown', true, true, window, 0,
    0, 0, x, y, false, false,
    false, false, 0, null
);
document.elementFromPoint(x, y).dispatchEvent(mouseDownEvent);

var mouseUpEvent = document.createEvent('MouseEvents');
mouseUpEvent.initMouseEvent(
    'mouseup', true, true, window, 0,
    0, 0, x, y, false, false,
    false, false, 0, null
);
document.elementFromPoint(x, y).dispatchEvent(mouseUpEvent);
