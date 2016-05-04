using System;
using System.Reactive.Subjects;

namespace NSpeex.Reactive
{
    /// <summary>
    /// デコードサブジェクト
    /// </summary>
    public class DecodeSubject : ISubject<byte[]>, IDisposable
    {
        private readonly SpeexDecoder _decoder;
        private readonly Subject<byte[]> _subject = new Subject<byte[]>();

        public DecodeSubject(BandMode mode)
        {
            _decoder = new SpeexDecoder(mode);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }

        void IObserver<byte[]>.OnCompleted()
        {
            _subject.OnCompleted();
        }

        void IObserver<byte[]>.OnError(Exception error)
        {
            _subject.OnError(error);
        }

        void IObserver<byte[]>.OnNext(byte[] value)
        {
            _subject.OnNext(Decode(value));
        }

        IDisposable IObservable<byte[]>.Subscribe(IObserver<byte[]> observer) => _subject.Subscribe(observer);

        private unsafe byte[] Decode(byte[] data)
        {
            var outputBufferTemp = new byte[data.Length*320];
            fixed (byte* bBuffer = outputBufferTemp)
            {
                var sBuffer = (short*) bBuffer;
                var samplesDecoded = _decoder.Decode(data, 0, data.Length, sBuffer, outputBufferTemp.Length/2, 0, false);
                var decoded = new byte[samplesDecoded*2];
                Buffer.BlockCopy(outputBufferTemp, 0, decoded, 0, decoded.Length);
                return decoded;
            }
        }
    }
}