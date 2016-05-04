using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using NUnit.Framework;

namespace SoundCapture.Reactive.Droid.Test
{
    [TestFixture]
    public class SoundCaptureTest
    {
        private SoundCapture _capture;

        [SetUp]
        public void Setup()
        {
            _capture = new SoundCapture(8000, 250);
        }

        [TearDown]
        public void Tear()
        {
            _capture.Dispose();
        }

        [Test]
        public void StartTest()
        {
            _capture.Subscribe(new OutObserver());
            _capture.Start();
            ManualResetEventSlim ev = new ManualResetEventSlim();
            var timer = new Timer((o) =>
            {
                ev.Set();
            }, null, TimeSpan.FromMilliseconds(20000), TimeSpan.FromMilliseconds(0));
            ev.Wait();
        }

        [Test]
        public void SpeexTest()
        {
            var encoder = new NSpeex.Reactive.EncodeSubject();
            _capture.Subscribe(encoder);
            //_capture.Subscribe(new OutObserver());
            encoder.Subscribe(new OutObserver());
            _capture.Start();
            Thread.Sleep(20000);
        }

        [Test]
        public void WebSocketTest()
        {
            var observer = new WebSocketObserver(new Uri("ws://192.168.1.5:81"));
            var encoder = new NSpeex.Reactive.EncodeSubject();
            _capture.Subscribe(encoder);
            //_capture.Subscribe(new OutObserver());
            encoder.Subscribe(observer);
            _capture.Start();
            ManualResetEventSlim ev = new ManualResetEventSlim();
            ev.Wait();
        }
    }

    public class WebSocketObserver : IObserver<byte[]>
    {
        private ClientWebSocket client = new ClientWebSocket();

        public WebSocketObserver(Uri uri)
        {
            client.ConnectAsync(uri, CancellationToken.None).GetAwaiter().GetResult();
        }
        public void OnNext(byte[] value)
        {
            client.SendAsync(new ArraySegment<byte>(value), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        public void OnError(Exception error)
        {
            client.CloseAsync(WebSocketCloseStatus.InternalServerError, "", CancellationToken.None);
        }

        public void OnCompleted()
        {
            client.CloseAsync(WebSocketCloseStatus.InternalServerError, "", CancellationToken.None);
        }
    }

    public class OutObserver : IObserver<byte[]>
    {
        public void OnNext(byte[] value)
        {
            Console.WriteLine($"OnNext {value.Length}byte");
            Console.WriteLine(string.Join("-", value.Select(x => x.ToString("x2"))));
        }

        public void OnError(Exception error)
        {
            throw error;
        }

        public void OnCompleted()
        {
            Console.WriteLine("OnComplete");
        }
    }
}