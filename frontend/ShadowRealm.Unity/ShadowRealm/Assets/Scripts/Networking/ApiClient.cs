#nullable enable
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ShadowRealm.Networking
{
    /// <summary>
    /// Singleton API klient pro komunikaci s backendem
    /// </summary>
    public class ApiClient : MonoBehaviour
    {
        public static ApiClient Instance { get; private set; } = null!;

        [Header("Configuration")]
        [SerializeField] private string baseUrl = "http://localhost:5000/api";
        [SerializeField] private int requestTimeout = 30;
        [SerializeField] private int maxRetries = 3;

        private string? _jwtToken;

        // Event pro sledování stavu připojení
        public event Action<bool>? OnConnectionStateChanged;
        public bool IsConnected { get; private set; }

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
            Debug.Log("JWT Token set successfully");
        }

        public void ClearToken()
        {
            _jwtToken = null;
            Debug.Log("JWT Token cleared");
        }

        public bool HasToken() => !string.IsNullOrEmpty(_jwtToken);

        /// <summary>
        /// Vytvoří UnityWebRequest s automatickým přidáním headers
        /// </summary>
        private UnityWebRequest CreateRequest(string endpoint, string method, string? jsonBody = null)
        {
            // OPRAVENO: endpoint už NEMÁ /api prefix
            string fullUrl = $"{baseUrl}{endpoint}";
            
            var request = new UnityWebRequest(fullUrl, method)
            {
                uploadHandler = jsonBody != null
                    ? new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody))
                    : null,
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = requestTimeout
            };

            request.SetRequestHeader("Content-Type", "application/json");

            if (!string.IsNullOrEmpty(_jwtToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_jwtToken}");
            }

            return request;
        }

        /// <summary>
        /// POST request s retry logikou a lepším error handlingem
        /// </summary>
        public IEnumerator Post(
            string endpoint, 
            string jsonBody, 
            Action<string> onSuccess, 
            Action<string> onError,
            int retryCount = 0)
        {
            using var request = CreateRequest(endpoint, UnityWebRequest.kHttpVerbPOST, jsonBody);
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                IsConnected = true;
                OnConnectionStateChanged?.Invoke(true);
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                // Retry logika pro network errors
                if (ShouldRetry(request.result) && retryCount < maxRetries)
                {
                    Debug.LogWarning($"Request failed, retrying... ({retryCount + 1}/{maxRetries})");
                    yield return new WaitForSeconds(Mathf.Pow(2, retryCount)); // Exponential backoff
                    yield return Post(endpoint, jsonBody, onSuccess, onError, retryCount + 1);
                }
                else
                {
                    IsConnected = false;
                    OnConnectionStateChanged?.Invoke(false);
                    string errorMessage = GetErrorMessage(request);
                    Debug.LogError($"POST {endpoint} failed: {errorMessage}");
                    onError?.Invoke(errorMessage);
                }
            }
        }

        /// <summary>
        /// GET request s retry logikou
        /// </summary>
        public IEnumerator Get(
            string endpoint, 
            Action<string> onSuccess, 
            Action<string> onError,
            int retryCount = 0)
        {
            using var request = CreateRequest(endpoint, UnityWebRequest.kHttpVerbGET);
            
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                IsConnected = true;
                OnConnectionStateChanged?.Invoke(true);
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                if (ShouldRetry(request.result) && retryCount < maxRetries)
                {
                    Debug.LogWarning($"Request failed, retrying... ({retryCount + 1}/{maxRetries})");
                    yield return new WaitForSeconds(Mathf.Pow(2, retryCount));
                    yield return Get(endpoint, onSuccess, onError, retryCount + 1);
                }
                else
                {
                    IsConnected = false;
                    OnConnectionStateChanged?.Invoke(false);
                    string errorMessage = GetErrorMessage(request);
                    Debug.LogError($"GET {endpoint} failed: {errorMessage}");
                    onError?.Invoke(errorMessage);
                }
            }
        }

        /// <summary>
        /// PUT request (pro budoucí použití)
        /// </summary>
        public IEnumerator Put(
            string endpoint, 
            string jsonBody, 
            Action<string> onSuccess, 
            Action<string> onError)
        {
            using var request = CreateRequest(endpoint, UnityWebRequest.kHttpVerbPUT, jsonBody);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                string errorMessage = GetErrorMessage(request);
                Debug.LogError($"PUT {endpoint} failed: {errorMessage}");
                onError?.Invoke(errorMessage);
            }
        }

        /// <summary>
        /// Určí, zda má smysl request opakovat
        /// </summary>
        private bool ShouldRetry(UnityWebRequest.Result result)
        {
            return result == UnityWebRequest.Result.ConnectionError || 
                   result == UnityWebRequest.Result.DataProcessingError;
        }

        /// <summary>
        /// Získá čitelnou chybovou zprávu z requestu
        /// </summary>
        private string GetErrorMessage(UnityWebRequest request)
        {
            // Pokus se parsovat error message z response body
            if (!string.IsNullOrEmpty(request.downloadHandler?.text))
            {
                try
                {
                    var errorResponse = JsonUtility.FromJson<ErrorResponse>(request.downloadHandler.text);
                    if (!string.IsNullOrEmpty(errorResponse.message))
                        return errorResponse.message;
                }
                catch
                {
                    // Pokud se nepodaří parsovat, vrátíme raw text
                    return request.downloadHandler.text;
                }
            }

            return $"{request.error} (Code: {request.responseCode})";
        }

        [Serializable]
        private class ErrorResponse
        {
            public string message = "";
        }
    }
}