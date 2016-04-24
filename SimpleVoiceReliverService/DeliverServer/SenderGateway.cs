using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace DeliverServer
{
    /// <summary>
    ///     データ配信ゲートウェイ
    /// </summary>
    public class SenderGateway
    {
        private readonly Func<HttpListenerContext, AuthResult> _authFunc;
        private readonly IObservable<HttpListenerContext> _connectObservable;
        private readonly HttpListener _listener;
        private CompositeDisposable _disposable;

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="authFunc">認証ファンクション</param>
        /// <param name="prefixes">HttpListnerプレフィックス</param>
        public SenderGateway(Func<HttpListenerContext, AuthResult> authFunc, params string[] prefixes)
        {
            _listener = new HttpListener();
            _authFunc = authFunc;

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

        private Subject<SenderModel> SenderSubject { get; } = new Subject<SenderModel>();

        /// <summary>
        ///     ゲートウェイ開始
        /// </summary>
        public void Start()
        {
            if (_listener.IsListening) return;
            _disposable = new CompositeDisposable();
            _connectObservable.Subscribe(async x => await Accept(x)).AddTo(_disposable);
            _listener.Start();
        }

        /// <summary>
        ///     ゲートウェイ終了
        /// </summary>
        public void Stop()
        {
            if (!_listener.IsListening) return;
            _disposable.Dispose();
            _disposable = null;
            SenderSubject.OnCompleted();
            _listener.Stop();
        }

        private async Task Accept(HttpListenerContext context)
        {
            if (context.Request.IsWebSocketRequest)
            {
                var authResult = _authFunc(context);
                if (authResult.Result)
                {
                    var websocketContext = await context.AcceptWebSocketAsync(string.Empty);
                    var client = new SenderClient(websocketContext, authResult.Channel);
                    client.Subscribe(SenderSubject.OnNext);
                    return;
                }
            }
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            context.Response.Close();
        }
    }
}