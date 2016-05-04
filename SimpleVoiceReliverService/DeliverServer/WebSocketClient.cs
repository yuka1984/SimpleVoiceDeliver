using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeliverServer
{
    public class WebSocketClient : IObserver<SenderModel>,  IObservable<byte[]>, IObservable<string>, IDisposable
    {
        private const int BufferSize = 64000;
        private readonly Subject<byte[]> _binarySubjet = new Subject<byte[]>();
        private readonly byte[] _buffer = new byte[BufferSize];
        private readonly ClientWebSocket _client;
        private readonly Subject<string> _textSubjet = new Subject<string>();
        private readonly Uri _uri;
        private IDisposable _receiveDisposable;

        public WebSocketClient(Uri uri)
        {
            _client = new ClientWebSocket();
            _uri = uri;
        }

        public bool IsConnected { get; private set; }

        public void Dispose()
        {
            _client?.Dispose();
            if (IsConnected)
            {
                _receiveDisposable.Dispose();
            }
            _binarySubjet.Dispose();
            _textSubjet.Dispose();
        }

        IDisposable IObservable<byte[]>.Subscribe(IObserver<byte[]> observer)
        {
            return _binarySubjet.Subscribe(observer);
        }

        IDisposable IObservable<string>.Subscribe(IObserver<string> observer)
        {
            return _textSubjet.Subscribe(observer);
        }

        public async Task Start()
        {
            if (IsConnected) return;
            await _client.ConnectAsync(_uri, CancellationToken.None);
            _receiveDisposable = Observable.FromAsync(() => ReceiveAsync(_client, _buffer))
                .Repeat()
                .Publish()
                .RefCount()
                .Subscribe(x =>
                {
                    if (x.Item1.MessageType == WebSocketMessageType.Text)
                    {
                        _textSubjet.OnNext(Encoding.ASCII.GetString(x.Item2));
                    }
                    else if (x.Item1.MessageType == WebSocketMessageType.Binary)
                    {
                        _binarySubjet.OnNext(x.Item2);
                    }
                    else
                    {
                        _textSubjet.OnCompleted();
                        _binarySubjet.OnCompleted();
                    }
                });
            IsConnected = true;
        }

        public async Task Stop()
        {
            if (!IsConnected) return;
            _receiveDisposable.Dispose();
            await _client.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);
            IsConnected = false;
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

        void IObserver<SenderModel>.OnNext(SenderModel value)
        {
            if (!IsConnected) return;
            if (value.IsClose)
            {
                _client.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
            }
            else
            {
                try
                {
                    _client.SendAsync(new ArraySegment<byte>(value.ReceiveData),
                        value.IsBinary ? WebSocketMessageType.Binary : WebSocketMessageType.Text
                        , true,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        void IObserver<SenderModel>.OnError(Exception error)
        {
            _client.CloseAsync(WebSocketCloseStatus.InternalServerError, "", CancellationToken.None);
        }

        void IObserver<SenderModel>.OnCompleted()
        {
            _client.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }
    }
}