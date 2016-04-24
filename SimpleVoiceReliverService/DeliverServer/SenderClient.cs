using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace DeliverServer
{
    /// <summary>
    ///     データ送信クライアント
    /// </summary>
    public class SenderClient : IObservable<SenderModel>
    {
        private const int BufferSize = 64000;
        private readonly byte[] _buffer = new byte[BufferSize];

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        /// <param name="context"></param>
        /// <param name="channel"></param>
        public SenderClient(WebSocketContext context, string channel)
        {
            Context = context;
            Channel = channel;
            var receiveObservable = Observable
                .FromAsync(() => ReceiveAsync(context.WebSocket, _buffer))
                .Repeat()
                .Publish()
                .RefCount();

            receiveObservable.Subscribe(
                x =>
                {
                    if (x.Item1.MessageType == WebSocketMessageType.Close)
                    {
                        SenderSubject
                            .OnNext(new SenderModel(channel, x.Item1.MessageType == WebSocketMessageType.Binary, x.Item2, true));
                        SenderSubject.OnCompleted();
                    }
                    else
                    {
                        SenderSubject
                            .OnNext(new SenderModel(channel, x.Item1.MessageType == WebSocketMessageType.Binary, x.Item2));
                    }
                }
                , e => SenderSubject.OnError(e)
                , () => SenderSubject.OnCompleted());
        }

        private Subject<SenderModel> SenderSubject { get; } = new Subject<SenderModel>();

        /// <summary>
        ///     コンテキスト
        /// </summary>
        public WebSocketContext Context { get; }

        /// <summary>
        ///     チャンネル識別
        /// </summary>
        public string Channel { get; }

        /// <summary>オブザーバーが通知を受け取ることをプロバイダーに通知します。</summary>
        /// <returns>プロバイダーが通知の送信を完了する前に、オブザーバーが通知の受信を停止できるインターフェイスへの参照。</returns>
        /// <param name="observer">通知を受け取るオブジェクト。</param>
        IDisposable IObservable<SenderModel>.Subscribe(IObserver<SenderModel> observer)
            => SenderSubject.Subscribe(observer);

        public static async Task<Tuple<WebSocketReceiveResult, byte[]>> ReceiveAsync(WebSocket webSocket, byte[] buffer)
        {
            var resultCount = 0;
            while (true)
            {
                var segmentbuffer = new ArraySegment<byte>(buffer, resultCount, buffer.Length - resultCount);
                var result = await webSocket.ReceiveAsync(segmentbuffer, CancellationToken.None);
                resultCount += result.Count;
                if (resultCount >= buffer.Length)
                {
                    throw new Exception("long message");
                }
                if (result.EndOfMessage)
                {
                    if (resultCount == 0) return Tuple.Create(result, new byte[0]);
                    var resultbuffer = new byte[resultCount];
                    Buffer.BlockCopy(buffer, 0, resultbuffer, 0, resultCount);
                    return Tuple.Create(result, resultbuffer);
                }
            }
        }
    }
}