using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeliverServer;
using NAudio.Wave;
using VoiceRecorder;

namespace ClientTest
{
    class Program
    {
        static void Main(string[] args)
        {
            ManualResetEventSlim re = new ManualResetEventSlim();

            var client = new WebSocketClient();
            var decoder = new ObserverSpeexDecoder();
            var speaker = new ObserverSoundSpeaker(new WaveOutEvent(), new WaveFormat(8000, 16, 1));
            decoder.Subscribe(speaker);
            ((IObservable<byte[]>) client).Subscribe(decoder);

            client.Start();
            re.Wait();
        }
    }
}
