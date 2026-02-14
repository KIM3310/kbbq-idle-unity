using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public abstract class ApiClientBase
{
    protected ApiConfig config;
    protected string playerId;
    protected string token;

    public void Initialize(ApiConfig config)
    {
        this.config = config;
    }

    public void SetAuth(string playerId, string token)
    {
        this.playerId = playerId;
        this.token = token;
    }

    protected async Task<T> SendRequest<T>(string method, string path, object body, bool signHeaders)
    {
        if (config == null)
        {
            throw new InvalidOperationException("ApiConfig is not set.");
        }

        if (!config.enableNetwork || (Application.isEditor && !config.allowInEditor) || string.IsNullOrEmpty(config.baseUrl))
        {
            return default;
        }

        var url = config.baseUrl.TrimEnd('/') + path;
        var bodyJson = body != null ? JsonUtility.ToJson(body) : null;
        var request = new UnityWebRequest(url, method);

        if (!string.IsNullOrEmpty(bodyJson))
        {
            var payload = Encoding.UTF8.GetBytes(bodyJson);
            request.uploadHandler = new UploadHandlerRaw(payload);
        }

        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        if (signHeaders)
        {
            var headers = NetworkUtils.BuildSignedHeaders(playerId, bodyJson, config.hmacSecret);
            request.SetRequestHeader("X-Nonce", headers.nonce);
            request.SetRequestHeader("X-Timestamp", headers.timestamp.ToString());
            request.SetRequestHeader("X-Signature", headers.signature);
        }

        request.timeout = config.timeoutSeconds > 0 ? config.timeoutSeconds : 10;

        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            var responseBody = request.downloadHandler != null ? request.downloadHandler.text : string.Empty;
            var code = request.responseCode;
            var message = string.IsNullOrEmpty(request.error) ? "Request failed" : request.error;
            throw new Exception("HTTP " + code + ": " + message + (string.IsNullOrEmpty(responseBody) ? "" : (" | " + responseBody)));
        }

        var text = request.downloadHandler.text;
        if (string.IsNullOrEmpty(text))
        {
            return default;
        }

        return JsonUtility.FromJson<T>(text);
    }
}
