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
    public class SenderGateway : IObserver<HttpListenerContext>, IObservable<SenderModel>
    {
        private readonly Func<HttpListenerContext, AuthResult> _authFunc;
        private readonly Subject<SenderModel> _senderSubject = new Subject<SenderModel>();

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="authFunc">認証ファンクション</param>
        /// <param name="prefixes">HttpListnerプレフィックス</param>
        public SenderGateway(Func<HttpListenerContext, AuthResult> authFunc)
        {
            _authFunc = authFunc;
        }                

        /// <summary>オブザーバーが通知を受け取ることをプロバイダーに通知します。</summary>
        /// <returns>プロバイダーが通知の送信を完了する前に、オブザーバーが通知の受信を停止できるインターフェイスへの参照。</returns>
        /// <param name="observer">通知を受け取るオブジェクト。</param>
        public IDisposable Subscribe(IObserver<SenderModel> observer) => _senderSubject.Subscribe(observer);

        /// <summary>オブザーバーに新しいデータを提供します。</summary>
        /// <param name="value">現在の通知情報。</param>
        public　async void OnNext(HttpListenerContext context)
        {
            var authResult = _authFunc(context);
            if (authResult.Result)
            {
                var websocketContext = await context.AcceptWebSocketAsync(null);
                var client = new SenderClient(websocketContext, authResult.Channel);
                client.Subscribe(_senderSubject.OnNext);
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.Close();
            }
        }

        /// <summary>プロバイダーでエラー状態が発生したことをオブザーバーに通知します。</summary>
        /// <param name="error">エラーに関する追加情報を提供するオブジェクト。</param>
        public void OnError(Exception error)
        {
            _senderSubject.OnError(error);
        }

        /// <summary>プロバイダーがプッシュ ベースの通知の送信を完了したことをオブザーバーに通知します。</summary>
        public void OnCompleted()
        {
            _senderSubject.OnCompleted();
        }
    }
}