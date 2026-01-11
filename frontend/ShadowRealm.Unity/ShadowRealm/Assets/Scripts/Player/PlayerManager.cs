#nullable enable
using System;
using System.Collections;
using UnityEngine;
using ShadowRealm.Networking;

namespace ShadowRealm.Player
{
    [Serializable]
    public class PlayerSaveRequest
    {
        public float posX;
        public float posY;
        public float posZ;
    }

    [Serializable]
    public class PlayerStateResponse
    {
        public float posX;
        public float posY;
        public float posZ;
        public int level;
        public int experience;
    }

    /// <summary>
    /// Spravuje operace související s hráčem (save position, load state)
    /// </summary>
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager Instance { get; private set; } = null!;

        [Header("Auto-Save Settings")]
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float autoSaveInterval = 60f; // každých 60 sekund

        private float _lastSaveTime;
        private Vector3 _lastSavedPosition;

        public event Action<PlayerStateResponse>? OnStateLoaded;
        public event Action? OnPositionSaved;

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

        private void Update()
        {
            // Auto-save pozice, pokud je zapnutý
            if (enableAutoSave && Time.time - _lastSaveTime >= autoSaveInterval)
            {
                // Zde bys měl získat pozici hráče z tvého character controlleru
                // Tohle je jen příklad:
                // SavePosition(playerTransform.position);
            }
        }

        /// <summary>
        /// Načte stav hráče ze serveru (pozice, level, XP)
        /// </summary>
        public void LoadState(Action<bool, PlayerStateResponse?>? callback = null)
        {
            StartCoroutine(LoadStateCoroutine(callback));
        }

        private IEnumerator LoadStateCoroutine(Action<bool, PlayerStateResponse?>? callback)
        {
            PlayerStateResponse? state = null;
            bool success = false;

            yield return ApiClient.Instance.Get(
                "/player/state",
                onSuccess: response =>
                {
                    try
                    {
                        state = JsonUtility.FromJson<PlayerStateResponse>(response);
                        success = true;
                        Debug.Log($"Player state loaded - Pos: ({state.posX}, {state.posY}, {state.posZ}), Level: {state.level}");
                        OnStateLoaded?.Invoke(state);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to parse player state: {ex.Message}");
                    }
                },
                onError: error =>
                {
                    Debug.LogError($"Failed to load player state: {error}");
                });

            callback?.Invoke(success, state);
        }

        /// <summary>
        /// Uloží pozici hráče na server
        /// </summary>
        public void SavePosition(Vector3 position, Action<bool>? callback = null)
        {
            // Optimalizace - neukládej, pokud se pozice nezměnila
            if (Vector3.Distance(position, _lastSavedPosition) < 0.1f)
            {
                callback?.Invoke(true);
                return;
            }

            StartCoroutine(SavePositionCoroutine(position, callback));
        }

        private IEnumerator SavePositionCoroutine(Vector3 position, Action<bool>? callback)
        {
            var requestData = new PlayerSaveRequest
            {
                posX = position.x,
                posY = position.y,
                posZ = position.z
            };

            string json = JsonUtility.ToJson(requestData);
            bool success = false;

            yield return ApiClient.Instance.Post(
                "/player/save",
                json,
                onSuccess: response =>
                {
                    success = true;
                    _lastSavedPosition = position;
                    _lastSaveTime = Time.time;
                    Debug.Log($"Position saved: ({position.x}, {position.y}, {position.z})");
                    OnPositionSaved?.Invoke();
                },
                onError: error =>
                {
                    Debug.LogError($"Failed to save position: {error}");
                });

            callback?.Invoke(success);
        }

        /// <summary>
        /// Force save - okamžitě uloží pozici bez čekání
        /// </summary>
        public void ForceSave(Vector3 position)
        {
            _lastSaveTime = 0; // Reset timer
            SavePosition(position);
        }
    }
}