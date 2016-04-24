using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace DeliverServer
{
    public class ReceiverGateway
    {
        private readonly Func<HttpListenerContext, AuthResult> _authFunc;
        private readonly IObservable<HttpListenerContext> _connectObservable;
        private IObservable<SenderModel> _voiceObservable;
        private readonly HttpListener _listener;
        private CompositeDisposable _disposable;

        public ReceiverGateway(Func<HttpListenerContext, AuthResult> authFunc, IObservable<SenderModel> voiceObservable, params string[] prefixes)
        {
            _authFunc = authFunc;
            _listener = new HttpListener();
            _voiceObservable = voiceObservable;

            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _connectObservable = Observable
                .FromAsync(_listener.GetContextAsync)
                .Repeat()
                .Retry()
                .Publish()
                .RefCount()
                ;
        }

        /// <summary>
        /// ゲートウェイ開始
        /// </summary>
        public void Start()
        {
            if (_listener.IsListening) return;
            _disposable = new CompositeDisposable();
            _listener.Start();
            _connectObservable.Subscribe(async x => await Accept(x)).AddTo(_disposable);            
        }

        /// <summary>
        /// ゲートウェイ停止
        /// </summary>
        public void Stop()
        {
            if (!_listener.IsListening) return;
            _disposable.Dispose();
            _disposable = null;
            _listener.Stop();
        }

        private async Task Accept(HttpListenerContext context)
        {
            if (context.Request.IsWebSocketRequest)
            {
                var authResult = _authFunc(context);
                if (authResult.Result)
                {
                    var websocketContext = await context.AcceptWebSocketAsync(null);
                    var client = new ReceiverClient(websocketContext, authResult.Channel);
                    _voiceObservable.Subscribe(client);
                    return;
                }
            }
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
        }


    }
}
