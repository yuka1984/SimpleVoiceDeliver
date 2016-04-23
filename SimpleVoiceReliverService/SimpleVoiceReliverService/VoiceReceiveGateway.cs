using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Linq;

namespace SimpleVoiceReliverService
{
    /// <summary>
    ///     音声データ受信ゲートウェイ
    /// </summary>
    public class VoiceReceiveGateway
    {
        private readonly ObservableCollection<VoiceSpeakClient> _clients = new ObservableCollection<VoiceSpeakClient>();
        private readonly object _clientsLock = new object();

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
        
        private Func<IReadOnlyList<string>> GetChannels { get; }
        private HttpListener Listner { get; }

        private IObservable<HttpListenerContext> ConnectObservable { get; }
        private IDisposable ConnectDisposable { get; set; }

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
                _clients.Add(client);                
            }
        }
    }
}