using System;
using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace DeliverServer
{
    public class ObservableListenerServer : IObservable<HttpListenerContext>
    {
        private readonly IObservable<HttpListenerContext> _connectObservable;
        private readonly HttpListener _listener = new HttpListener();
        private readonly Subject<HttpListenerContext> _subject = new Subject<HttpListenerContext>();
        private CompositeDisposable _disposable;

        public ObservableListenerServer(params string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                _listener.Prefixes.Add(prefix);
            }
            _connectObservable = Observable
                .FromAsync(_listener.GetContextAsync)
                .Where(x =>
                {
                    if (x.Request.IsWebSocketRequest) return true;
                    x.Response.StatusCode = 400;
                    x.Response.Close();
                    return false;
                })
                .Repeat()
                .Retry()
                .Publish()
                .RefCount()
                ;
        }

        public IDisposable Subscribe(IObserver<HttpListenerContext> observer)
        {
            if (_listener.IsListening)
            {
                return _connectObservable.Subscribe(observer);
            }
            observer.OnCompleted();
            return Disposable.Empty;
        }

        public void Start()
        {
            _disposable = new CompositeDisposable();
            _listener.Start();
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}