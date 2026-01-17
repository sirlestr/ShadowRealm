#nullable enable
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShadowRealm.Auth;

namespace ShadowRealm.UI
{
    /// <summary>
    /// Příklad UI pro login/register obrazovku
    /// </summary>
    public class LoginUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField usernameInput = null!;
        [SerializeField] private TMP_InputField passwordInput = null!;
        [SerializeField] private Button loginButton = null!;
        [SerializeField] private Button registerButton = null!;
        [SerializeField] private TMP_Text errorText = null!;
        [SerializeField] private GameObject loadingPanel = null!;

        [Header("Scenes")]
        [SerializeField] private string gameSceneName = "GameScene";

        private void Start()
        {
            // Připoj tlačítka
            loginButton.onClick.AddListener(OnLoginClicked);
            registerButton.onClick.AddListener(OnRegisterClicked);

            // Skryj error text na začátku
            if (errorText != null)
                errorText.gameObject.SetActive(false);

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            // Připoj eventy z AuthManageru
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess += OnLoginSuccess;
                AuthManager.Instance.OnLoginFailed += OnLoginFailed;
            }
        }

        private void OnDestroy()
        {
            // Odpoj eventy
            if (AuthManager.Instance != null)
            {
                AuthManager.Instance.OnLoginSuccess -= OnLoginSuccess;
                AuthManager.Instance.OnLoginFailed -= OnLoginFailed;
            }
        }

        private void OnLoginClicked()
        {
            string username = usernameInput.text.Trim();
            string password = passwordInput.text;

            // Validace
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Username and password are required");
                return;
            }

            // Skryj error, zobraz loading
            HideError();
            ShowLoading(true);

            // Zavolej login
            AuthManager.Instance.Login(username, password, (success, message) =>
            {
                ShowLoading(false);
                
                if (!success)
                {
                    ShowError(message);
                }
                // OnLoginSuccess event se zavolá automaticky
            });
        }

        private void OnRegisterClicked()
        {
            string username = usernameInput.text.Trim();
            string password = passwordInput.text;

            // Validace
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ShowError("Username and password are required");
                return;
            }

            if (password.Length < 8)
            {
                ShowError("Password must be at least 8 characters long");
                return;
            }

            HideError();
            ShowLoading(true);

            AuthManager.Instance.Register(username, password, (success, message) =>
            {
                ShowLoading(false);

                if (success)
                {
                    ShowError("Registration successful! Please login.", false);
                }
                else
                {
                    ShowError(message);
                }
            });
        }

        private void OnLoginSuccess(Auth.PlayerData playerData)
        {
            Debug.Log($"Welcome {playerData.username}!");
            
            // Přejdi do herní scény
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
        }

        private void OnLoginFailed(string error)
        {
            ShowError(error);
        }

        private void ShowError(string message, bool isError = true)
        {
            if (errorText != null)
            {
                errorText.text = message;
                errorText.color = isError ? Color.red : Color.green;
                errorText.gameObject.SetActive(true);
            }
        }

        private void HideError()
        {
            if (errorText != null)
                errorText.gameObject.SetActive(false);
        }

        private void ShowLoading(bool show)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(show);

            // Zakázání tlačítek během loading
            loginButton.interactable = !show;
            registerButton.interactable = !show;
        }
    }
}