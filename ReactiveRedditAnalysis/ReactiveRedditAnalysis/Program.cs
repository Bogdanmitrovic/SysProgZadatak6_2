using System.Net;
using Newtonsoft.Json;
using ReactiveRedditAnalysis;
using ReactiveRedditAnalysis.Models;
using ReactiveRedditAnalysis.ReactiveLayers;
using Reddit;

HttpServer server = new();
server.Start();

SubredditReader subredditReader = new();
subredditReader.Subscribe(server);

CommentAnalyzer commentAnalyzer = new();
commentAnalyzer.Subscribe(subredditReader);


Console.ReadLine();

subredditReader.Unsubscribe();
server.Dispose();