using System.Net;
using System.Text;
using Newtonsoft.Json;
using VaderSharp2;

namespace ReactiveRedditAnalysis.ReactiveLayers;

public class CommentAnalyzer : IObserver<(List<String>,HttpListenerContext)>
{
    private IDisposable _unsubscriber;

    public void OnCompleted()
    {
        throw new NotImplementedException();
    }

    public void OnError(Exception error)
    {
        throw new NotImplementedException();
    }

    public void OnNext((List<string>, HttpListenerContext) value)
    {
        SentimentIntensityAnalyzer analyzer = new();
        double totalPositive = 0;
        double totalNegative = 0;
        double totalNeutral = 0;
        double totalCompound = 0;
        foreach (var comment in value.Item1)
        {
            var resultsIndividual = analyzer.PolarityScores(comment);
            totalPositive += resultsIndividual.Positive;
            totalNegative += resultsIndividual.Negative;
            totalNeutral += resultsIndividual.Neutral;
            totalCompound += resultsIndividual.Compound;
        }
        var results = new Dictionary<string, double>
        {
            {"Positive", totalPositive},
            {"Negative", totalNegative},
            {"Neutral", totalNeutral},
            {"Compound", totalCompound}
        };
        var response = value.Item2.Response;
        response.StatusCode = 200;
        response.ContentType = "application/json";
        var json = JsonConvert.SerializeObject(results);
        var buffer = Encoding.UTF8.GetBytes(json);
        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.Close();
    }

    public void Subscribe(SubredditReader subredditReader)
    {
        _unsubscriber = subredditReader.Subscribe(this);
    }

    public void Unsubscribe()
    {
        _unsubscriber.Dispose();
    }
}