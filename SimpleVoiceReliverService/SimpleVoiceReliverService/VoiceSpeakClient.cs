using System;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace SimpleVoiceReliverService
{
    /// <summary>
    /// �������M�N���C�A���g
    /// </summary>
    public class VoiceSpeakClient : IDisposable
    {
        private readonly byte[] _buffer = new byte[102400];
        private readonly CompositeDisposable _disposable;

        private IObservable<Tuple<WebSocketReceiveResult, byte[]>> ReceiveObservable { get; }
        private Subject<Tuple<string, byte[]>> SoundSubject { get; } = new Subject<Tuple<string, byte[]>>();
        private Subject<Tuple<string, string>> TextSubject { get; } = new Subject<Tuple<string, string>>();
        private Subject<VoiceSpeakClient> CloseSubject { get; } = new Subject<VoiceSpeakClient>();
        /// <summary>
        /// �E�F�u�\�P�b�g�R���e�L�X�g
        /// </summary>
        public WebSocketContext Context { get; }
        /// <summary>
        /// �`�����l��
        /// </summary>
        public string Channel { get; }
        /// <summary>
        /// ����Observable
        /// </summary>
        public IObservable<Tuple<string, byte[]>> SoundObservable => SoundSubject;
        /// <summary>
        /// �e�L�X�gObservable
        /// </summary>
        public IObservable<Tuple<string, string>> TextObservable => TextSubject;
        /// <summary>
        /// �N���[�YObservable
        /// </summary>
        public IObservable<VoiceSpeakClient> CloseObservable => CloseSubject;

        /// <summary>
        /// �R���X�g���N�^
        /// </summary>
        /// <param name="context">WebSocket�R���e�L�X�g</param>
        /// <param name="channel">�`�����l��</param>
        public VoiceSpeakClient(WebSocketContext context, string channel)
        {
            _disposable = new CompositeDisposable(SoundSubject, TextSubject, CloseSubject, context.WebSocket);

            Context = context;
            Channel = channel;
            ReceiveObservable = Observable
                .FromAsync(() => ReceiveAsync(context.WebSocket, _buffer))
                .Repeat()
                .Publish()
                .RefCount()
                ;

            ReceiveObservable.Subscribe(x =>
            {
                switch (x.Item1.MessageType)
                {
                    case WebSocketMessageType.Binary:
                        SoundSubject.OnNext(Tuple.Create(channel, x.Item2));
                        break;
                    case WebSocketMessageType.Text:
                        TextSubject.OnNext(Tuple.Create(channel, Encoding.ASCII.GetString(x.Item2)));
                        break;
                    case WebSocketMessageType.Close:
                        SoundSubject.OnCompleted();
                        TextSubject.OnCompleted();
                        CloseSubject.OnNext(this);
                        CloseSubject.OnCompleted();
                        break;
                }
            }).AddTo(_disposable);
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }

        private static async Task<Tuple<WebSocketReceiveResult, byte[]>> ReceiveAsync(WebSocket webSocket, byte[] buffer)
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