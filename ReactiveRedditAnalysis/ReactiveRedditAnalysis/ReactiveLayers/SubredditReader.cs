using System.Net;
using Reddit;

namespace ReactiveRedditAnalysis.ReactiveLayers;

public class SubredditReader : IObserver<HttpListenerContext>, IObservable<(List<string>, HttpListenerContext)>
{
    private IDisposable _unsubscriber;
    private readonly RedditClient _redditClient;
    private readonly List<IObserver<(List<string>, HttpListenerContext)>> _observers;

    public SubredditReader()
    {
        _observers = new List<IObserver<(List<string>, HttpListenerContext)>>();
        Utilities.GetCredentials(out string appId, out string appSecret);
        var accessToken = Utilities.GetAccessTokenAsync(appId!, appSecret!).Result;
        _redditClient = new RedditClient(appId: appId, appSecret: appSecret, accessToken: accessToken);
    }

    public void Subscribe(HttpServer httpServer)
    {
        _unsubscriber = httpServer.Subscribe(this);
    }

    public void Unsubscribe()
    {
        _unsubscriber.Dispose();
    }

    public void OnCompleted()
    {
        // Handle completion
    }

    public void OnError(Exception error)
    {
        // Handle error
    }

    public void OnNext(HttpListenerContext context)
    {
        var request = context.Request;
        if (request.HttpMethod != "GET")
        {
            Utilities.ReturnBadRequest(context, "Only GET requests are supported");
            return;
        }

        //get subreddit name from request
        var parts = request.Url?.AbsolutePath.Split('/');
        if (parts == null || parts.Length < 2 || parts[1] == "")
        {
            Utilities.ReturnBadRequest(context, "Subreddit name not specified");
            return;
        }

        string subredditName = parts[1];
        try
        {
            var subreddit = _redditClient.Subreddit(parts[1]);
            var posts = subreddit.Posts.GetHot(limit: 10);
            var comments = posts.SelectMany(post => post.Comments.GetComments(limit: 10));
            var commentBodies = comments.Select(comment => comment.Body).ToList();
            PassListToObservers(commentBodies, context);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private void PassListToObservers(List<string> commentBodiesList, HttpListenerContext context)
    {
        foreach (var observer in _observers)
        {
            observer.OnNext((commentBodiesList, context));
        }
    }

    public IDisposable Subscribe(IObserver<(List<string>, HttpListenerContext)> observer)
    {
        _observers.Add(observer);
        return new Unsubscriber<(List<string>, HttpListenerContext)>(_observers, observer);
    }
}