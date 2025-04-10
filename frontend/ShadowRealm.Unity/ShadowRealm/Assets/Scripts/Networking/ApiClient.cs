#nullable enable
using System;
using System.Collections;
using System.Text;
using UnityEngine.Networking;
using UnityEngine;

public class ApiClient : MonoBehaviour
{
    public static ApiClient Instance { get; private set; } = null!;
    public string BaseUrl = "http://localhost:5000/api";

    private string? _jwtToken;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
           Destroy(gameObject);
           return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetToken(string token)
    {
        _jwtToken = token;
    }


    private UnityWebRequest CreateRequest(string endpoint, string method, string? jsonBody = null)
    {
        var request = new UnityWebRequest($"{BaseUrl}{endpoint}", method)
        {
            uploadHandler = jsonBody != null
                ? new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody))
                : null,
            downloadHandler = new DownloadHandlerBuffer()
        };

        request.SetRequestHeader("Content-Type", "application/json");

        if (!string.IsNullOrEmpty(_jwtToken))
        {
            request.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
        }

        return request;
    }
    public IEnumerator Post(string endpoint, string jsonBody, Action<string> onSuccess, Action<string> onError)
    {
        var request = CreateRequest(endpoint, UnityWebRequest.kHttpVerbPOST, jsonBody);
        yield return request.SendWebRequest();
        
        if(request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }
    
    public IEnumerator Get(string endpoint, Action<string> onSuccess, Action<string> onError)
    {
        var request = CreateRequest(endpoint, UnityWebRequest.kHttpVerbGET);
        yield return request.SendWebRequest();
        
        if(request.result == UnityWebRequest.Result.Success)
            onSuccess?.Invoke(request.downloadHandler.text);
        else
            onError?.Invoke(request.error);
    }
}
