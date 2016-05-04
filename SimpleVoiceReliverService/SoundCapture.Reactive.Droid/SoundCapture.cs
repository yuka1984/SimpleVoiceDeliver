using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Media;

[assembly: UsesPermission(Manifest.Permission.RecordAudio)]

namespace SoundCapture.Reactive.Droid
{
    public class SoundCapture : IObservable<byte[]>, IDisposable
    {
        private readonly short[] _buffer;
        private bool _isrecording;
        private readonly Subject<byte[]> _readSubject = new Subject<byte[]>();
        private readonly AudioRecord _record;
        private Thread _thread;

        public SoundCapture(int sampleSize, int buffermilliseconds)
        {
            if (buffermilliseconds > 1000) throw new ArgumentOutOfRangeException(nameof(buffermilliseconds));
            var pushsize = sampleSize/(1000/buffermilliseconds);
            var minbuffersize = AudioRecord.GetMinBufferSize(sampleSize, ChannelIn.Mono, Encoding.Pcm16bit);
            if (pushsize < minbuffersize)
                throw new ArgumentException($"MinBufferSize is {minbuffersize}byte");
            _record = new AudioRecord(AudioSource.Default, sampleSize, ChannelIn.Mono, Encoding.Pcm16bit, pushsize);
            _buffer = new short[pushsize / 2];
        }

        public void Dispose()
        {
            Stop();
            _record.Release();
            _readSubject.OnCompleted();
            _readSubject.Dispose();
            _record.Dispose();
        }

        public IDisposable Subscribe(IObserver<byte[]> observer) => _readSubject.Subscribe(observer);

        public void Start()
        {
            if (_isrecording) return;            
            _isrecording = true;
            _thread = new Thread(ReadThread);
            _thread.Start();
        }

        public void Stop()
        {
            if (!_isrecording) return;            
            _isrecording = false;
            Thread.Sleep(1000);
            _thread.Abort();
        }

        private void ReadThread()
        {
            _record.StartRecording();
            while (_isrecording)
            {
                var size = _record.Read(_buffer, 0, _buffer.Length);
                var result = new byte[size * 2];
                Buffer.BlockCopy(_buffer, 0, result, 0, result.Length);
                _readSubject.OnNext(result);
            }
            _record.Stop();
        }
    }
}