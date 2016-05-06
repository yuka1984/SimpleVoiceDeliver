using System;
using System.Reactive.Subjects;
    
namespace NSpeex.Reactive
{
    /// <summary>
    ///     エンコードサブジェクト
    /// </summary>
    public class EncodeSubject : ISubject<byte[]>, IDisposable
    {
        private readonly SpeexEncoder _encoder;
        private readonly Subject<byte[]> _subject = new Subject<byte[]>();

        public EncodeSubject() : this(BandMode.Narrow) { }

        public EncodeSubject(BandMode mode)
        {
            _encoder = new SpeexEncoder(mode);
        }

        public void Dispose()
        {
            _subject.Dispose();
        }

        public void OnNext(byte[] value)
        {
            _subject.OnNext(Encode(value));
        }

        public void OnError(Exception error)
        {
            _subject.OnError(error);
        }

        public void OnCompleted()
        {
            _subject.OnCompleted();
        }

        public IDisposable Subscribe(IObserver<byte[]> observer) => _subject.Subscribe(observer);

        private unsafe byte[] Encode(byte[] data)
        {
            fixed (byte* bBuffer = data)
            {
                var sBuffer = (short*) bBuffer;
                var samplesToEncode = data.Length/2;
                if (samplesToEncode%_encoder.FrameSize != 0)
                {
                    samplesToEncode -= samplesToEncode%_encoder.FrameSize;
                }
                var outputBufferTemp = new byte[data.Length];
                var bytesWritten = _encoder.Encode(sBuffer, 0, samplesToEncode, outputBufferTemp, 0, data.Length);
                var encoded = new byte[bytesWritten];
                Buffer.BlockCopy(outputBufferTemp, 0, encoded, 0, bytesWritten);
                return encoded;
            }
        }
    }
}