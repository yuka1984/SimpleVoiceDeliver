using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings.Extensions;

namespace SimpleVoiceDeliverService
{
    public class VoiceDeliverServer
    {
        private Func<IReadOnlyList<string>> GetChannels { get; }
        private HttpListener Listner { get; }
        private IObservable<HttpListenerContext> ConnectObservable { get; }
        private IDisposable ConnectDisposable { get; set; }
        private IObservable<Tuple<VoiceSpeakClient, byte[]>> VoiceObservable { get; }
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public VoiceDeliverServer(IObservable<Tuple<VoiceSpeakClient, byte[]>> voiceObservable, Func<IReadOnlyList<string>> channelsFunc, params string[] prefixes)
        {
            this.Listner = new HttpListener();
            this.VoiceObservable = voiceObservable;
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
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.Close();
        }


        private void Accept(WebSocketContext context, string channel)
        {
            var client = new VoiceReceiveClient(context, channel);
            VoiceObservable.Subscribe(client).AddTo(_disposable);
        }
    }
}
