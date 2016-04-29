using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using NAudio.Wave;
using NSpeex;

namespace VoiceRecorder
{
    public class ObservableSpeexEncoder : IObserver<byte[]>, IObservable<byte[]>
    {
        private readonly SpeexEncoder _encoder;

        private readonly WaveBuffer encoderInputBuffer;
        private readonly Subject<byte[]> _encodeSubject = new Subject<byte[]>();

        public ObservableSpeexEncoder(int bufferSize)
        {
            _encoder = new SpeexEncoder(BandMode.Narrow);
            encoderInputBuffer = new WaveBuffer(bufferSize);
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
            var samplesToEncode = encoderInputBuffer.ShortBufferCount;
            if (samplesToEncode%_encoder.FrameSize != 0)
            {
                samplesToEncode -= samplesToEncode%_encoder.FrameSize;
            }
            var outputBufferTemp = new byte[length]; // contains more than enough space
            var bytesWritten = _encoder.Encode(encoderInputBuffer.ShortBuffer, 0, samplesToEncode, outputBufferTemp, 0,
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
            var leftoverSamples = encoderInputBuffer.ShortBufferCount - samplesEncoded;
            Array.Copy(encoderInputBuffer.ByteBuffer, samplesEncoded*2, encoderInputBuffer.ByteBuffer, 0,
                leftoverSamples*2);
            encoderInputBuffer.ShortBufferCount = leftoverSamples;
        }

        private void FeedSamplesIntoEncoderInputBuffer(byte[] data, int offset, int length)
        {
            Array.Copy(data, offset, encoderInputBuffer.ByteBuffer, encoderInputBuffer.ByteBufferCount, length);
            encoderInputBuffer.ByteBufferCount += length;
        }
    }
}