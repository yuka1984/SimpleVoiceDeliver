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
    public class SenderGateway : IObservable<SenderModel>
    {
        private readonly Func<HttpListenerContext, AuthResult> _authFunc;
        private readonly IObservable<HttpListenerContext> _connectObservable;
        private readonly Subject<SenderModel> _senderSubject = new Subject<SenderModel>();
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

        /// <summary>
        ///     ゲートウェイ開始
        /// </summary>
        public void Start()
        {
            if (_listener.IsListening) return;
            _disposable = new CompositeDisposable();            
            _listener.Start();
            _connectObservable.Subscribe(async x => await Accept(x)).AddTo(_disposable);
        }

        /// <summary>
        ///     ゲートウェイ終了
        /// </summary>
        public void Stop()
        {
            if (!_listener.IsListening) return;
            _disposable.Dispose();
            _disposable = null;
            _senderSubject.OnCompleted();
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
                    var client = new SenderClient(websocketContext, authResult.Channel);
                    client.Subscribe(_senderSubject.OnNext);
                    return;
                }
            }
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            context.Response.Close();
        }


        /// <summary>オブザーバーが通知を受け取ることをプロバイダーに通知します。</summary>
        /// <returns>プロバイダーが通知の送信を完了する前に、オブザーバーが通知の受信を停止できるインターフェイスへの参照。</returns>
        /// <param name="observer">通知を受け取るオブジェクト。</param>
        public IDisposable Subscribe(IObserver<SenderModel> observer) => _senderSubject.Subscribe(observer);
    }
}