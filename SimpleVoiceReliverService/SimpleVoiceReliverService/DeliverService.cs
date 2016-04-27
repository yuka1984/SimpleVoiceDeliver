using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using DeliverServer;
using Reactive.Bindings.Extensions;

namespace SimpleVoiceDeliverService
{
    public class DeliverService : IService
    {
        private readonly SenderGateway _senderGateway;
        private readonly ReceiverGateway _receiverGataway;
        private readonly ObservableListenerServer _senderSerer;
        private readonly ObservableListenerServer _receiverServer;
        private CompositeDisposable _dispose;

        public DeliverService()
        {
            _senderSerer = new ObservableListenerServer("http://*:81/");
            _senderGateway = new SenderGateway(Auth);
            _senderGateway.Subscribe(x =>
            {
                Console.Out.WriteLineAsync(
                    $"{x.Channel} IsBinary:{x.IsBinary} IsClose:{x.IsClose} {string.Join("-", x.ReceiveData.Select(y => y.ToString("x2")))}");
            });
            

            _receiverServer = new ObservableListenerServer("http://*:82/");
            _receiverGataway = new ReceiverGateway(Auth);
            
        }

        private AuthResult Auth(HttpListenerContext context)
        {
            return new AuthResult(true, "TestChannel");
        }

        public void Start()
        {
            _dispose = new CompositeDisposable();
            _senderSerer.Start();
            _receiverServer.Start();

            _senderSerer.Subscribe(_senderGateway).AddTo(_dispose);
            _receiverServer.Subscribe(_receiverGataway).AddTo(_dispose);
            _senderGateway.Subscribe(_receiverGataway).AddTo(_dispose);

        }

        public void Stop()
        {
            _senderSerer.Stop();
            _receiverServer.Stop();

            _dispose.Dispose();
        }
    }
}
