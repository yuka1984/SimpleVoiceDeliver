using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Reactive.Bindings.Extensions;
using SimpleVoiceReliverService;

namespace SimpleVoiceDeliverService
{
    /// <summary>
    ///     音声データ受信ゲートウェイ
    /// </summary>
    public class VoiceReceiveGateway
    {
        private readonly ObservableCollection<Tuple<VoiceSpeakClient, IDisposable>> _clients = new ObservableCollection<Tuple<VoiceSpeakClient, IDisposable>>();
        private readonly object _clientsLock = new object();

        private Func<IReadOnlyList<string>> GetChannels { get; }
        private HttpListener Listner { get; }
        private IObservable<HttpListenerContext> ConnectObservable { get; }
        private IDisposable ConnectDisposable { get; set; }

        private Subject<Tuple<VoiceSpeakClient, byte[]>> VoiceSubject { get; } = new Subject<Tuple<VoiceSpeakClient, byte[]>>();
        private Subject<Tuple<VoiceSpeakClient, string>> TextSubject { get; } = new Subject<Tuple<VoiceSpeakClient, string>>();

        /// <summary>
        /// 音声Observable
        /// </summary>
        public IObservable<Tuple<VoiceSpeakClient, byte[]>> VoiceObservable => VoiceSubject;

        /// <summary>
        ///     コンストラクタ
        /// </summary>
        public VoiceReceiveGateway(Func<IReadOnlyList<string>> channelsFunc, params string[] prefixes)
        {
            if (channelsFunc == null) throw new ArgumentNullException(nameof(channelsFunc));
            Listner = new HttpListener();
            foreach (var prefix in prefixes)
            {
                Listner.Prefixes.Add(prefix);
            }
            ConnectObservable = Observable
                .FromAsync(Listner.GetContextAsync)
                .Repeat()
                .Retry()
                .Publish()
                .RefCount()
                ;

            GetChannels = channelsFunc;
        }             

        /// <summary>
        ///     ゲートウェイ開始
        /// </summary>
        public void Start()
        {
            ConnectDisposable = ConnectObservable.Subscribe(Connect);
            Listner.Start();
        }

        /// <summary>
        ///     ゲートウェイ停止
        /// </summary>
        public void Stop()
        {
            ConnectDisposable?.Dispose();
            Listner.Stop();
            lock (_clientsLock)
            {
                _clients.ToList().ForEach(x => x.Item2.Dispose());
                _clients.Clear();
            }
            VoiceSubject.OnCompleted();
        }

        private void Connect(HttpListenerContext context)
        {
            if (context.Request.IsWebSocketRequest)
            {
                if (context.Request.QueryString.AllKeys.Contains("Channel"))
                {
                    var channel = context.Request.QueryString["Channel"];
                    if (GetChannels().Any(x => x.Equals(channel)))
                    {
                        Observable
                            .FromAsync(() => context.AcceptWebSocketAsync(string.Empty))
                            .Take(1)
                            .Subscribe(x => Accept(x, channel));
                        return;
                    }
                }
            }
            context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            context.Response.Close();
        }

        private void Accept(WebSocketContext context, string channel)
        {
            var client = new VoiceSpeakClient(context, channel);
            lock (_clientsLock)
            {
                var disposable = new CompositeDisposable();
                client.SoundObservable.Subscribe(VoiceSubject).AddTo(disposable);
                client.TextObservable.Subscribe(TextSubject).AddTo(disposable);
                client.CloseObservable.Subscribe(x =>
                {
                    lock (_clientsLock)
                    {
                        _clients
                        .Where(y => y.Item1.Equals(x))
                        .ToList()
                        .ForEach(y => _clients.Remove(y));
                    }
                });
                disposable.Add(client);
                _clients.Add(new Tuple<VoiceSpeakClient, IDisposable>(client, new CompositeDisposable()));                
            }
        }
    }
}