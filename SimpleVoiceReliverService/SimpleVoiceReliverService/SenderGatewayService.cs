using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DeliverServer;

namespace SimpleVoiceDeliverService
{
    public class SenderGatewayService : IService
    {
        private readonly SenderGateway _senderGateway;

        public SenderGatewayService()
        {
            _senderGateway = new SenderGateway(Auth, "http://*:81/");
            _senderGateway.Subscribe(x =>
            {
                Console.Out.WriteLineAsync(
                    $"{x.Channel} IsBinary:{x.IsBinary} IsClose:{x.IsClose} {string.Join("-", x.ReceiveData.Select(y => y.ToString("x2")))}");
            });
        }

        private AuthResult Auth(HttpListenerContext context)
        {
            return new AuthResult(true, "TestChannel");
        }

        public void Start()
        {
            _senderGateway.Start();
        }

        public void Stop()
        {
            _senderGateway.Stop();
        }
    }
}
