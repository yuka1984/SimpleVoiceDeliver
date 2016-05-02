using System;
using System.Reactive.Subjects;
using NAudio.Wave;
using NSpeex;

namespace VoiceRecorder
{
    /// <summary>
    ///     Speexデコーダー
    /// </summary>
    public class ObserverSpeexDecoder : IObserver<byte[]>, IObservable<byte[]>
    {
        private readonly SpeexDecoder _decoder;
        private readonly Subject<byte[]> _pcmSubject = new Subject<byte[]>();

        public ObserverSpeexDecoder() : this(BandMode.Narrow)
        {
        }

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        public ObserverSpeexDecoder(BandMode mode)
        {
            _decoder = new SpeexDecoder(mode);
        }

        public IDisposable Subscribe(IObserver<byte[]> observer) => _pcmSubject.Subscribe(observer);

        public void OnNext(byte[] value)
        {
            _pcmSubject.OnNext(Decode(value, 0, value.Length));
        }

        public void OnError(Exception error)
        {
            _pcmSubject.OnError(error);
        }

        public void OnCompleted()
        {
            _pcmSubject.OnCompleted();
        }

        private byte[] Decode(byte[] data, int offset, int length)
        {
            var outputBufferTemp = new byte[length*320];
            var wb = new WaveBuffer(outputBufferTemp);
            var samplesDecoded = _decoder.Decode(data, offset, length, wb.ShortBuffer, 0, false);
            var bytesDecoded = samplesDecoded*2;
            var decoded = new byte[bytesDecoded];
            Array.Copy(outputBufferTemp, 0, decoded, 0, bytesDecoded);
            return decoded;
        }
    }
}