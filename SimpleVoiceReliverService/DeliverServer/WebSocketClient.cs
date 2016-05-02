using System;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeliverServer
{
    public class WebSocketClient : IObservable<byte[]>, IObservable<string>
    {
        private const int BufferSize = 64000;
        private readonly byte[] _buffer = new byte[BufferSize];
        private readonly Subject<byte[]> binarySubjet = new Subject<byte[]>();
        private readonly ClientWebSocket client;
        private readonly Subject<string> textSubjet = new Subject<string>();

        public WebSocketClient()
        {
            client = new ClientWebSocket();
        }

        IDisposable IObservable<byte[]>.Subscribe(IObserver<byte[]> observer)
        {
            return binarySubjet.Subscribe(observer);
        }

        IDisposable IObservable<string>.Subscribe(IObserver<string> observer)
        {
            return textSubjet.Subscribe(observer);
        }

        public async Task Start()
        {
            await client.ConnectAsync(new Uri("ws://localhost:81/"), CancellationToken.None);

            var receiveObservable = Observable.FromAsync(() => ReceiveAsync(client, _buffer))
                .Repeat()
                .Publish()
                .RefCount()
                ;

            receiveObservable.Subscribe(x =>
            {
                if (x.Item1.MessageType == WebSocketMessageType.Text)
                {
                    textSubjet.OnNext(Encoding.ASCII.GetString(x.Item2));
                }
                else if (x.Item1.MessageType == WebSocketMessageType.Binary)
                {
                    binarySubjet.OnNext(x.Item2);
                }
                else
                {
                    textSubjet.OnCompleted();
                    binarySubjet.OnCompleted();
                }
            });
        }

        public static async Task<Tuple<WebSocketReceiveResult, byte[]>> ReceiveAsync(ClientWebSocket webSocket,
            byte[] buffer)
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