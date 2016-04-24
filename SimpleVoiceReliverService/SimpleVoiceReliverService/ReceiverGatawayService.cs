using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DeliverServer;

namespace SimpleVoiceDeliverService
{
    public class ReceiverGatawayService : IService
    {
        private readonly ReceiverGateway _gateway;

        public ReceiverGatawayService()
        {
            var timer = Observable.Interval(TimeSpan.FromMilliseconds(100))
                .Select(x => new SenderModel("TestChannel", false, Encoding.ASCII.GetBytes(x.ToString())))
                .Publish()
                ;
            timer.Connect();
                           
            _gateway = new ReceiverGateway(Auth, timer, "http://*:81/");
        }

        private AuthResult Auth(HttpListenerContext context)
        {
            return new AuthResult(true, "TestChannel");
        }

        public void Start()
        {
            _gateway.Start();
        }

        public void Stop()
        {
            _gateway.Stop();
        }
    }
}
