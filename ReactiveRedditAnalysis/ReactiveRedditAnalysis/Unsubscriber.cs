namespace ReactiveRedditAnalysis;

public class Unsubscriber<T> : IDisposable
{
    private List<IObserver<T>> _observers;
    private IObserver<T> _observer;

    public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
    {
        _observers = observers;
        _observer = observer;
    }

    public void Dispose()
    {
        if (_observers.Contains(_observer))
        {
            _observers.Remove(_observer);
        }
    }
}