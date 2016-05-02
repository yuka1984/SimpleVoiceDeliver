using System;
using System.Reactive.Subjects;
using NAudio.Wave;
using NSpeex;

namespace VoiceRecorder
{
    public class ObservableSpeexEncoder : IObserver<byte[]>, IObservable<byte[]>
    {
        private readonly SpeexEncoder _encoder;

        private readonly WaveBuffer _encoderInputBuffer;
        private readonly Subject<byte[]> _encodeSubject = new Subject<byte[]>();

        public ObservableSpeexEncoder(int bufferSize) : this(bufferSize, BandMode.Narrow)
        {
        }

        public ObservableSpeexEncoder(int bufferSize, BandMode mode)
        {
            _encoder = new SpeexEncoder(mode);
            _encoderInputBuffer = new WaveBuffer(bufferSize);
        }

        IDisposable IObservable<byte[]>.Subscribe(IObserver<byte[]> observer) => _encodeSubject.Subscribe(observer);

        void IObserver<byte[]>.OnNext(byte[] value)
        {
            _encodeSubject.OnNext(Encode(value, 0, value.Length));
        }

        void IObserver<byte[]>.OnError(Exception error)
        {
            _encodeSubject.OnError(error);
        }

        void IObserver<byte[]>.OnCompleted()
        {
            _encodeSubject.OnCompleted();
        }

        private byte[] Encode(byte[] data, int offset, int length)
        {
            FeedSamplesIntoEncoderInputBuffer(data, offset, length);
            var samplesToEncode = _encoderInputBuffer.ShortBufferCount;
            if (samplesToEncode%_encoder.FrameSize != 0)
            {
                samplesToEncode -= samplesToEncode%_encoder.FrameSize;
            }
            var outputBufferTemp = new byte[length]; // contains more than enough space
            var bytesWritten = _encoder.Encode(_encoderInputBuffer.ShortBuffer, 0, samplesToEncode, outputBufferTemp, 0,
                length);
            var encoded = new byte[bytesWritten];
            Array.Copy(outputBufferTemp, 0, encoded, 0, bytesWritten);
            ShiftLeftoverSamplesDown(samplesToEncode);
            //Debug.WriteLine(
            //$"NSpeex: In {length} bytes, encoded {bytesWritten} bytes [enc frame size = {_encoder.FrameSize}]");
            return encoded;
        }

        private void ShiftLeftoverSamplesDown(int samplesEncoded)
        {
            var leftoverSamples = _encoderInputBuffer.ShortBufferCount - samplesEncoded;
            Array.Copy(_encoderInputBuffer.ByteBuffer, samplesEncoded*2, _encoderInputBuffer.ByteBuffer, 0,
                leftoverSamples*2);
            _encoderInputBuffer.ShortBufferCount = leftoverSamples;
        }

        private void FeedSamplesIntoEncoderInputBuffer(byte[] data, int offset, int length)
        {
            Array.Copy(data, offset, _encoderInputBuffer.ByteBuffer, _encoderInputBuffer.ByteBufferCount, length);
            _encoderInputBuffer.ByteBufferCount += length;
        }
    }
}