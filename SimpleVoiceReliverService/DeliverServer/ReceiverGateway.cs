using System;
using System.Net;
using System.Reactive.Subjects;

namespace DeliverServer
{
    public class ReceiverGateway : IObserver<HttpListenerContext>, IObserver<SenderModel>
    {
        private readonly Func<HttpListenerContext, AuthResult> _authFunc;

        private readonly Subject<SenderModel> _sendSubject = new Subject<SenderModel>();

        public ReceiverGateway(Func<HttpListenerContext, AuthResult> authFunc)
        {
            _authFunc = authFunc;
        }

        async void IObserver<HttpListenerContext>.OnNext(HttpListenerContext context)
        {
            var authResult = _authFunc(context);
            if (authResult.Result)
            {
                var websocketContext = await context.AcceptWebSocketAsync(null);
                var client = new ReceiverClient(websocketContext, authResult.Channel);
                _sendSubject.Subscribe(client);
            }
            else
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                context.Response.Close();
            }
        }

        void IObserver<HttpListenerContext>.OnError(Exception error)
        {
        }

        void IObserver<HttpListenerContext>.OnCompleted()
        {
            _sendSubject.OnCompleted();
        }

        public void OnNext(SenderModel value)
        {
            _sendSubject.OnNext(value);
        }

        public void OnError(Exception error)
        {
            _sendSubject.OnError(error);
        }

        public void OnCompleted()
        {
            _sendSubject.OnCompleted();
        }
    }
}