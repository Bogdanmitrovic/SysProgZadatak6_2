using System.Net;
using System.Text;
using Newtonsoft.Json;
using ReactiveRedditAnalysis.Models;

namespace ReactiveRedditAnalysis;

public static class Utilities
{
    public static async Task<string> GetAccessTokenAsync(string appId, string appSecret)
    {
        var client = new HttpClient();

        var tokenRequestBody = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "scope", "read" }
        };

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
            "Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{appId}:{appSecret}")));
        client.DefaultRequestHeaders.Add("User-Agent", "C# console app");

        var tokenResponse = await client.PostAsync("https://www.reddit.com/api/v1/access_token", new FormUrlEncodedContent(tokenRequestBody));
        var responseContent = await tokenResponse.Content.ReadAsStringAsync();

        client.Dispose();

        var token = JsonConvert.DeserializeObject<Token>(responseContent);
        if (token != null) return token.AccessToken!;
        throw new WebException("Could not get access token");
    }

    public static void GetCredentials(out string appId, out string appSecret)
    {
        if (!File.Exists("properties.json")) throw new Exception("Could not find properties.json");
        var keys = JsonConvert.DeserializeObject<Keys>(File.ReadAllText("properties.json"));
        if (keys == null) throw new Exception("Could not read properties.json");
        appId = keys.AppId!;
        appSecret = keys.AppSecret!;
    }
    public static void ReturnBadRequest(HttpListenerContext context, string message)
    {
        //print message to response
        var buffer = Encoding.UTF8.GetBytes(message);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.StatusCode = 400;
        context.Response.Close();
    }
}