using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliverServer;
using VoiceRecorder;

namespace SimpleVoiceDeliverService
{
    public class ReceiverGatawayService : IService
    {
        private readonly ReceiverGateway _gateway;
        private readonly ObservableListenerServer _server;

        public ReceiverGatawayService()
        {
            ObservableSoundCapture capture = new ObservableSoundCapture();
            ObservableSpeexEncoder encoder = new ObservableSpeexEncoder(6400);
            capture.Subscribe(encoder);
            var encoderSender = encoder.Delay(TimeSpan.FromMilliseconds(3000)).Select(x => new SenderModel("TestChannel", true, x));

            capture.Start();

            _server = new ObservableListenerServer("http://*:81/");
            var timer = Observable.Interval(TimeSpan.FromMilliseconds(100))
                .Select(x => new SenderModel("TestChannel", false, Encoding.ASCII.GetBytes(x.ToString())))
                .Publish()
                ;
            timer.Connect();                           
            _gateway = new ReceiverGateway(Auth);
            encoderSender.Subscribe(_gateway);            
        }

        private AuthResult Auth(HttpListenerContext context)
        {
            return new AuthResult(true, "TestChannel");
        }

        private IDisposable dispose;
        public void Start()
        {
            _server.Start();
            dispose = _server.Subscribe(_gateway);
        }

        public void Stop()
        {
            dispose.Dispose();
            _server.Stop();
        }
    }
}
