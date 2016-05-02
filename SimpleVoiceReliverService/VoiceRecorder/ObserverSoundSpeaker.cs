using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace VoiceRecorder
{
    public class ObserverSoundSpeaker : IObserver<byte[]>
    {
        private IWavePlayer player;
        private BufferedWaveProvider provider;

        public ObserverSoundSpeaker(IWavePlayer player, WaveFormat format)
        {
            this.player = player;
            provider = new BufferedWaveProvider(format);
            player.Init(provider);
            player.Play();
        }

        public void OnNext(byte[] value)
        {
            provider.AddSamples(value, 0, value.Length);
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
