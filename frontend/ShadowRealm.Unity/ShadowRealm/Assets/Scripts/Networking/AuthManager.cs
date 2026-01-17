#nullable enable
using System;
using System.Collections;
using UnityEngine;
using ShadowRealm.Networking;

namespace ShadowRealm.Auth
{
    [Serializable]
    public class LoginRequest
    {
        public string username = "";
        public string password = "";
    }

    [Serializable]
    public class LoginResponse
    {
        public string token = "";
    }

    [Serializable]
    public class RegisterRequest
    {
        public string username = "";
        public string password = "";
    }

    [Serializable]
    public class PlayerData
    {
        public int id;
        public string username = "";
        public int level;
        public int experience;
    }

    /// <summary>
    /// Spravuje autentizaci hráče (login, register, logout)
    /// </summary>
    public class AuthManager : MonoBehaviour
    {
        public static AuthManager Instance { get; private set; } = null!;

        // Events pro UI feedback
        public event Action<PlayerData>? OnLoginSuccess;
        public event Action<string>? OnLoginFailed;
        public event Action? OnLogout;

        [Header("Auto-Login (pouze pro development!)")]
        [SerializeField] private bool autoLoginOnStart = false;
        [SerializeField] private string devUsername = "testuser";
        [SerializeField] private string devPassword = "Password123";

        private PlayerData? _currentPlayer;
        public PlayerData? CurrentPlayer => _currentPlayer;
        public bool IsLoggedIn => _currentPlayer != null && ApiClient.Instance.HasToken();

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

        private void Start()
        {
            // POUZE PRO DEVELOPMENT - v produkci vypnout!
            if (autoLoginOnStart && Application.isEditor)
            {
                Debug.LogWarning("AUTO-LOGIN ENABLED - This should be disabled in production!");
                StartCoroutine(LoginCoroutine(devUsername, devPassword, null));
            }
        }

        /// <summary>
        /// Registruje nového hráče
        /// </summary>
        public void Register(string username, string password, Action<bool, string>? callback = null)
        {
            StartCoroutine(RegisterCoroutine(username, password, callback));
        }

        private IEnumerator RegisterCoroutine(string username, string password, Action<bool, string>? callback)
        {
            var requestData = new RegisterRequest
            {
                username = username,
                password = password
            };

            string json = JsonUtility.ToJson(requestData);
            bool success = false;
            string message = "";

            yield return ApiClient.Instance.Post(
                "/auth/register", // OPRAVENO: bez /api
                json,
                onSuccess: response =>
                {
                    success = true;
                    message = "Registration successful";
                    Debug.Log($"Registration successful for user: {username}");
                },
                onError: error =>
                {
                    success = false;
                    message = error;
                    Debug.LogError($"Registration failed: {error}");
                });

            callback?.Invoke(success, message);
        }

        /// <summary>
        /// Přihlásí hráče a automaticky načte jeho data
        /// </summary>
        public void Login(string username, string password, Action<bool, string>? callback = null)
        {
            StartCoroutine(LoginCoroutine(username, password, callback));
        }

        private IEnumerator LoginCoroutine(string username, string password, Action<bool, string>? callback)
        {
            var requestData = new LoginRequest
            {
                username = username,
                password = password
            };

            string json = JsonUtility.ToJson(requestData);
            bool loginSuccess = false;
            string errorMessage = "";

            yield return ApiClient.Instance.Post(
                "/auth/login", // OPRAVENO: bez /api
                json,
                onSuccess: response =>
                {
                    try
                    {
                        var loginResponse = JsonUtility.FromJson<LoginResponse>(response);
                        
                        if (string.IsNullOrEmpty(loginResponse.token))
                        {
                            errorMessage = "Invalid token received";
                            return;
                        }

                        ApiClient.Instance.SetToken(loginResponse.token);
                        loginSuccess = true;
                        Debug.Log($"Login successful for user: {username}");
                    }
                    catch (Exception ex)
                    {
                        errorMessage = $"Failed to parse login response: {ex.Message}";
                        Debug.LogError(errorMessage);
                    }
                },
                onError: error =>
                {
                    errorMessage = error;
                    Debug.LogError($"Login failed: {error}");
                });

            if (loginSuccess)
            {
                // Automaticky načti data hráče po úspěšném přihlášení
                yield return LoadPlayerData();
                callback?.Invoke(true, "Login successful");
            }
            else
            {
                callback?.Invoke(false, errorMessage);
                OnLoginFailed?.Invoke(errorMessage);
            }
        }

        /// <summary>
        /// Načte data aktuálně přihlášeného hráče
        /// </summary>
        public IEnumerator LoadPlayerData()
        {
            if (!ApiClient.Instance.HasToken())
            {
                Debug.LogError("Cannot load player data - no token available");
                yield break;
            }

            yield return ApiClient.Instance.Get(
                "/player/me", // OPRAVENO: bez /api
                onSuccess: response =>
                {
                    try
                    {
                        _currentPlayer = JsonUtility.FromJson<PlayerData>(response);
                        Debug.Log($"Player data loaded: {_currentPlayer.username}, Level: {_currentPlayer.level}, XP: {_currentPlayer.experience}");
                        OnLoginSuccess?.Invoke(_currentPlayer);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to parse player data: {ex.Message}");
                    }
                },
                onError: error =>
                {
                    Debug.LogError($"Failed to load player data: {error}");
                    // Pokud server vrátí 401, token pravděpodobně expiroval
                    if (error.Contains("401"))
                    {
                        Debug.LogWarning("Token expired - logging out");
                        Logout();
                    }
                });
        }

        /// <summary>
        /// Odhlásí hráče a vyčistí data
        /// </summary>
        public void Logout()
        {
            _currentPlayer = null;
            ApiClient.Instance.ClearToken();
            OnLogout?.Invoke();
            Debug.Log("User logged out");
        }

        /// <summary>
        /// Kontrola, zda je hráč přihlášen
        /// </summary>
        public bool CheckAuthentication()
        {
            if (!IsLoggedIn)
            {
                Debug.LogWarning("User is not authenticated");
                return false;
            }
            return true;
        }
    }
}