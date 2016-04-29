using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using VoiceRecorder;

namespace TryCodeProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var re = new ManualResetEventSlim();

            ObservableSoundCapture recorder = new ObservableSoundCapture();
            var encoder = new ObservableSpeexEncoder(640);

            recorder.Subscribe(x => Console.Out.WriteLineAsync(x.Length.ToString()));
            recorder.Subscribe(encoder);
            encoder.Subscribe(x => Console.Out.WriteLineAsync(x.Length.ToString()));

            recorder.Start();
            re.Wait();
        }

        private static void record()
        {
            
            
        }
    }
}
