using System;
using System.Net.WebSockets;
using System.Threading;

namespace DeliverServer
{
    /// <summary>
    ///     データ受信クライアント
    /// </summary>
    public class ReceiverClient : IObserver<SenderModel>
    {
        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        public ReceiverClient(WebSocketContext context, string channel)
        {
            Context = context;
            Channel = channel;
        }

        private WebSocketContext Context { get; }
        private string Channel { get; }

        /// <summary>オブザーバーに新しいデータを提供します。</summary>
        /// <param name="value">現在の通知情報。</param>
        public async void OnNext(SenderModel value)
        {
            if (value.Channel != Channel) return;
            if (Context.WebSocket.State != WebSocketState.Open) return;

            if (value.IsClose)
            {
                OnCompleted();
            }
            else
            {
                await Context.WebSocket.SendAsync(
                    new ArraySegment<byte>(value.ReceiveData),
                    value.IsBinary ? WebSocketMessageType.Binary : WebSocketMessageType.Text
                    , true,
                    CancellationToken.None);
            }
        }

        /// <summary>プロバイダーでエラー状態が発生したことをオブザーバーに通知します。</summary>
        /// <param name="error">エラーに関する追加情報を提供するオブジェクト。</param>
        public void OnError(Exception error)
        {
            OnCompleted();
        }

        /// <summary>プロバイダーがプッシュ ベースの通知の送信を完了したことをオブザーバーに通知します。</summary>
        public async void OnCompleted()
        {
            await Context.WebSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "SenderClose",
                CancellationToken.None);
            Context.WebSocket.Abort();
        }
    }
}