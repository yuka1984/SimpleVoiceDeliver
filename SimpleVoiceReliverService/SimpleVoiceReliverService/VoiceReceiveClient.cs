using System; == 
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleVoiceDeliverService
{
    public class VoiceReceiveClient : IObserver<Tuple<VoiceSpeakClient, byte[]>>
    {
        private WebSocketContext Context { get; }
        public string Channel { get; }

        public VoiceReceiveClient(WebSocketContext context, string channel)
        {
            this.Context = context;
            this.Channel = channel;
        }

        public async void OnNext(Tuple<VoiceSpeakClient, byte[]> value)
        {
            if(value.Item1.Channel != this.Channel) return;
            if (Context.WebSocket.State != WebSocketState.Open) return;
            
            await Context.WebSocket.SendAsync(new ArraySegment<byte>(value.Item2), WebSocketMessageType.Binary, true,
                CancellationToken.None);
        }

        public async void OnError(Exception error)
        {
            await Context.WebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "", CancellationToken.None);
            Context.WebSocket.Dispose();
        }

        public async void OnCompleted()
        {
            await Context.WebSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "", CancellationToken.None);
            Context.WebSocket.Dispose();
        }
    }
}
