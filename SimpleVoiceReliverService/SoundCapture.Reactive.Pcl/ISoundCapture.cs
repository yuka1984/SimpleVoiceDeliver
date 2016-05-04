using System;

namespace SoundCapture.Reactive
{
    public interface ISoundCapture : IObservable<byte[]>
    {
        void Start();

        void Stop();
    }
}
