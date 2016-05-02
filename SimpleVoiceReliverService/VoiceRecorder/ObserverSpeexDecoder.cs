using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NSpeex;

namespace VoiceRecorder
{
    public class ObserverSpeexDecoder : IObserver<byte[]>, IObservable<byte[]>
    {
        private SpeexDecoder decoder;
        private Subject<byte[]> _pcmSubject = new Subject<byte[]>();

        public ObserverSpeexDecoder()
        {
            decoder = new SpeexDecoder(BandMode.Narrow);
        }

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

        public byte[] Decode(byte[] data, int offset, int length)
        {
            var outputBufferTemp = new byte[length * 320];
            var wb = new WaveBuffer(outputBufferTemp);
            int samplesDecoded = decoder.Decode(data, offset, length, wb.ShortBuffer, 0, false);
            int bytesDecoded = samplesDecoded * 2;
            var decoded = new byte[bytesDecoded];
            Array.Copy(outputBufferTemp, 0, decoded, 0, bytesDecoded);
            Debug.WriteLine(
                $"NSpeex: In {length} bytes, decoded {bytesDecoded} bytes [dec frame size = {decoder.FrameSize}]");
            return decoded;
        }

        public IDisposable Subscribe(IObserver<byte[]> observer)
        {
            return _pcmSubject.Subscribe(observer);
        }
    }
}
