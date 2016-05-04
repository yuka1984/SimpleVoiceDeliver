/// <reference path="NSpeex.Decoder.ts"/>
/// <reference path="Scripts/collections.ts" />

class Greeter {
    element: HTMLElement;
    span: HTMLElement;
    timerToken: number;
    decoder: SpeexDecoder;
    socket: WebSocket;
    reader: FileReader;
    audioCotext: AudioContext;
    waiting: boolean;
    bufferQueue: collections.Queue<Float32Array>;

    constructor(element: HTMLElement) {
        this.element = element;
        this.element.innerHTML += "The time is: ";
        this.span = document.createElement('span');
        this.element.appendChild(this.span);
        this.span.innerText = new Date().toUTCString();
        this.reader = new FileReader();
        this.reader.onload = () => this.fileonLoad();
        this.decoder = new SpeexDecoder(BandMode.Narrow);
        this.socket = new WebSocket("ws://localhost:82/");
        this.socket.onmessage = (message) => this.onmessage(message);
        this.audioCotext = new AudioContext();
        this.waiting = true;
        this.bufferQueue = new collections.Queue<Float32Array>();

    }

    start() {
        this.timerToken = setInterval(() => this.span.innerHTML = new Date().toUTCString(), 500);
    }

    stop() {
        clearTimeout(this.timerToken);
    }

    onmessage(message: MessageEvent) {
        if (message.data.constructor === Blob) {
            this.reader.readAsArrayBuffer(message.data);
        }
    }

    fileonLoad() {
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
    }

    SoundPlay() {
        if (this.bufferQueue.size() > 0) {
            var buffer = this.bufferQueue.dequeue();
            var source = this.audioCotext.createBufferSource();
            var audioBuffer = this.audioCotext.createBuffer(1, buffer.length, 8000);
            audioBuffer.getChannelData(0).set(buffer);
            source.buffer = audioBuffer;
            source.connect(this.audioCotext.destination);
            source.onended = (ev) => this.SoundPlay();
            source.start();
        } else {
            this.waiting = true;
        }
        
    }

}

window.onload = () => {
    var el = document.getElementById('content');
    var greeter = new Greeter(el);
    greeter.start();
};