﻿using System;
using System.Runtime;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Reactive.Bindings.Extensions;

namespace SoundCapture.Reactive
{
    public class SoundCapture : ISoundCapture, IDisposable
    {
        private readonly Subject<byte[]> _avaibleSubject = new Subject<byte[]>();
        private readonly CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private readonly IWaveIn _waveIn;
        private bool _isrecording;

        public SoundCapture(int sampleSize, int buffermilliseconds)
        {
            _waveIn = new WaveInEvent
            {
                BufferMilliseconds = buffermilliseconds,
                WaveFormat = new WaveFormat(sampleSize, 16, 1),
            };

            var dataAbalableObservable = Observable.FromEvent<EventHandler<WaveInEventArgs>, WaveInEventArgs>(
                h => ((sender, eventArgs) => h(eventArgs))
                , h => _waveIn.DataAvailable += h
                , h => _waveIn.DataAvailable -= h);

            var recordingStopObservable = Observable.FromEvent<EventHandler<StoppedEventArgs>, StoppedEventArgs>(
                h => ((sender, eventArgs) => h(eventArgs))
                , h => _waveIn.RecordingStopped += h, h => _waveIn.RecordingStopped -= h
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

            _compositeDisposable.Add(_waveIn);
            _compositeDisposable.Add(_avaibleSubject);
        }

        public IDisposable Subscribe(IObserver<byte[]> observer) => _avaibleSubject.Subscribe(observer);

        public void Dispose()
        {
            _compositeDisposable.Dispose();
        }

        public void Start()
        {
            if (_isrecording) return;
            _isrecording = true;
            _waveIn.StartRecording();
        }

        public void Stop()
        {
            if (!_isrecording) return;
            _isrecording = false;
            _waveIn.StopRecording();
        }
    }
}
