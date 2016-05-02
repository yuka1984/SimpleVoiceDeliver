/// <reference path="NSpeex.Decoder.ts"/>
/// <reference path="Scripts/collections.ts" />
var Greeter = (function () {
    function Greeter(element) {
        var _this = this;
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement('span');
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
        this.reader = new FileReader();
        this.reader.onload = function () { return _this.fileonLoad(); };
        this.decoder = new SpeexDecoder(BandMode.Narrow);
        this.socket = new WebSocket("ws://localhost:81/");
        this.socket.onmessage = function (message) { return _this.onmessage(message); };
        this.audioCotext = new AudioContext();
        this.waiting = true;
        this.bufferQueue = new collections.Queue();
    }
    Greeter.prototype.start = function () {
        var _this = this;
        this.timerToken = setInterval(function () { return _this.span.innerHTML = new Date().toUTCString(); }, 500);
    };
    Greeter.prototype.stop = function () {
        clearTimeout(this.timerToken);
    };
    Greeter.prototype.onmessage = function (message) {
        if (message.data.constructor === Blob) {
            this.reader.readAsArrayBuffer(message.data);
        }
    };
    Greeter.prototype.fileonLoad = function () {
        var r = new Uint8Array(this.reader.result);
        var res = new Int16Array(2100);
        var size = this.decoder.Decode(r, 0, r.length, res, 0, false);
        var farray = new Float32Array(size);
        for (var i = 0; i < size; i++) {
            farray[i] = res[i] / 32768;
        }
        this.bufferQueue.enqueue(farray);
        if (this.waiting) {
            if (this.bufferQueue.size() > 2) {
                this.waiting = false;
                this.SoundPlay();
            }
        }
    };
    Greeter.prototype.SoundPlay = function () {
        var _this = this;
        if (this.bufferQueue.size() > 0) {
            var buffer = this.bufferQueue.dequeue();
            var source = this.audioCotext.createBufferSource();
            var audioBuffer = this.audioCotext.createBuffer(1, buffer.length, 8000);
            audioBuffer.getChannelData(0).set(buffer);
            source.buffer = audioBuffer;
            source.connect(this.audioCotext.destination);
            source.onended = function (ev) { return _this.SoundPlay(); };
            source.start();
        }
        else {
            this.waiting = true;
        }
    };
    return Greeter;
}());
window.onload = function () {
    var el = document.getElementById('content');
    var greeter = new Greeter(el);
    greeter.start();
};
//# sourceMappingURL=app.js.map