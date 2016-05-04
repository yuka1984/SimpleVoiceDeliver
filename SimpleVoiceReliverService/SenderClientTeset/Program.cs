using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DeliverServer;
using VoiceRecorder;

namespace SenderClientTeset
{
    class Program
    {
        static void Main(string[] args)
        {
            ManualResetEventSlim mr = new ManualResetEventSlim();

            var client = new WebSocketClient(new Uri("ws://localhost:81/"));
            ObservableSoundCapture capture = new ObservableSoundCapture();
            ObservableSpeexEncoder encoder = new ObservableSpeexEncoder(6400);
            capture.Subscribe(encoder);
            var encoderSender = encoder.Select(x => new SenderModel("TestChannel", true, x));

            capture.Start();
            encoderSender.Subscribe(client);
            client.Start();

            mr.Wait();
        }
    }
}
