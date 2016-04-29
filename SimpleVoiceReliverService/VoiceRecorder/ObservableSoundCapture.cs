using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NAudio.Wave;
using Reactive.Bindings.Extensions;

namespace VoiceRecorder
{
    public class ObservableSoundCapture : IObservable<byte[]>, IDisposable
    {
        private readonly Subject<byte[]> _avaibleSubject = new Subject<byte[]>();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        public IWaveIn WaveIn { get; }

        public ObservableSoundCapture() : this(new WaveInEvent {BufferMilliseconds = 20, NumberOfBuffers = 1})
        {
        }

        public ObservableSoundCapture(IWaveIn wavein)
        {
            WaveIn = wavein;

            var dataAbalableObservable = Observable.FromEvent<EventHandler<WaveInEventArgs>, WaveInEventArgs>(
                h => ((sender, eventArgs) => h(eventArgs))
                , h => wavein.DataAvailable += h
                , h => wavein.DataAvailable -= h);

            var recordingStopObservable = Observable.FromEvent<EventHandler<StoppedEventArgs>, StoppedEventArgs>(
                h => ((sender, eventArgs) => h(eventArgs))
                , h => wavein.RecordingStopped += h, h => wavein.RecordingStopped -= h
                );

            dataAbalableObservable.Select(x =>
            {
                var buffer = new byte[x.BytesRecorded];
                Buffer.BlockCopy(x.Buffer, 0, buffer, 0, buffer.Length);
                return buffer;
            }).Subscribe(_avaibleSubject)
                .AddTo(_compositeDisposable);

            recordingStopObservable
                .Subscribe(x => _avaibleSubject.OnCompleted())
                .AddTo(_compositeDisposable);

            _compositeDisposable.Add(WaveIn);
            _compositeDisposable.Add(_avaibleSubject);
        }

        public bool IsRunning { get; private set; }

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }


        public IDisposable Subscribe(IObserver<byte[]> observer) => _avaibleSubject.Subscribe(observer);

        public void Start()
        {
            if (IsRunning) return;
            IsRunning = true;
            WaveIn.StartRecording();
        }

        public void Stop()
        {
            if (!IsRunning) return;
            IsRunning = false;
            WaveIn.StopRecording();
        }
    }
}