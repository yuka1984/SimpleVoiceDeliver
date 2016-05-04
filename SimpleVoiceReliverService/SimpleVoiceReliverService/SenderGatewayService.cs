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
        private readonly ObservableListenerServer _server;

        public SenderGatewayService()
        {
            _server = new ObservableListenerServer("http://*:81/");
            _senderGateway = new SenderGateway(Auth);            

            _senderGateway.Subscribe(x =>
            {
                Console.Out.WriteLineAsync(
                    $"{x.Channel} IsBinary:{x.IsBinary} IsClose:{x.IsClose} {string.Join("-", x.ReceiveData.Select(y => y.ToString("x2")))}");
            });
        }

        private AuthResult Auth(HttpListenerContext context)
        {
            return new AuthResult(true, "TestChannel", ClientType.Sender);
        }

        private IDisposable dispose;
        public void Start()
        {
            _server.Start();
            dispose = _server.Subscribe(_senderGateway);
        }

        public void Stop()
        {
            dispose.Dispose();
            _server.Stop();
        }
    }
}
