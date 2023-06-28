using System.Net;

namespace ReactiveRedditAnalysis.ReactiveLayers;

public class HttpServer : IObservable<HttpListenerContext>, IDisposable
{
    private readonly HttpListener _listener;
    private readonly Thread _listenerThread;
    private bool _disposed;
    private readonly List<IObserver<HttpListenerContext>> _observers;

    public HttpServer(string address = "localhost", int port = 5080)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{address}:{port}/");
        _listenerThread = new Thread(Listen);
        _disposed = false;
        _observers = new List<IObserver<HttpListenerContext>>();
    }

    public void Start()
    {
        _listener.Start();
        _listenerThread.Start();
    }
    private void Listen()
    {
        while (_listener.IsListening)
        {
            try
            {
                var context = _listener.GetContext();
                if(_disposed) return;
                PassContextToObservers(context);
            }
            catch (HttpListenerException)
            {
                break;
            }
        }
    }

    private void PassContextToObservers(HttpListenerContext context)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(context);
        }
    }

    public IDisposable Subscribe(IObserver<HttpListenerContext> observer)
    {
        _observers.Add(observer);
        return new Unsubscriber<HttpListenerContext>(_observers, observer);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    private void Dispose(bool disposing)
    {
        if (_disposed) return;
        if (disposing)
        {
            _listener.Stop();
            _listenerThread.Join();
            _listener.Close();
        }
        _disposed = true;
    }
}