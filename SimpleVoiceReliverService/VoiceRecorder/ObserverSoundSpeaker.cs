using System;
using NAudio.Wave;

namespace VoiceRecorder
{
    /// <summary>
    ///     音声再生
    /// </summary>
    public class ObserverSoundSpeaker : IObserver<byte[]>
    {
        private readonly IWavePlayer _player;
        private readonly BufferedWaveProvider _provider;

        public ObserverSoundSpeaker(IWavePlayer player, WaveFormat format = null)
        {
            if (format == null)
            {
                format = new WaveFormat(8000, 16, 1);
            }
            _player = player;
            _provider = new BufferedWaveProvider(format);
            player.Init(_provider);
        }

        public bool IsPlay { get; private set; }

        public void OnNext(byte[] value)
        {
            _provider.AddSamples(value, 0, value.Length);
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     再生開始
        /// </summary>
        public void Start()
        {
            if (IsPlay) return;
            IsPlay = true;
            _player.Play();
        }

        /// <summary>
        ///     再生停止
        /// </summary>
        public void Stop()
        {
            if (!IsPlay) return;
            IsPlay = false;
            _player.Stop();
        }
    }
}