#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ShadowRealm.Networking;

namespace ShadowRealm.Quests
{
    [Serializable]
    public class QuestResponse
    {
        public int id;
        public string title = "";
        public string description = "";
        public int rewardXP;
    }

    [Serializable]
    public class QuestListResponse
    {
        public QuestResponse[] quests = Array.Empty<QuestResponse>();
    }

    [Serializable]
    public class QuestCompletionResponse
    {
        public string message = "";
        public int experienceGained;
        public int totalXP;
    }

    /// <summary>
    /// Spravuje questy - načítání dostupných questů a jejich dokončování
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; } = null!;

        private List<QuestResponse> _availableQuests = new();
        public IReadOnlyList<QuestResponse> AvailableQuests => _availableQuests;

        public event Action<List<QuestResponse>>? OnQuestsLoaded;
        public event Action<QuestCompletionResponse>? OnQuestCompleted;
        public event Action<string>? OnQuestError;

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

        /// <summary>
        /// Načte dostupné questy pro aktuálního hráče
        /// </summary>
        public void LoadAvailableQuests(Action<bool>? callback = null)
        {
            StartCoroutine(LoadAvailableQuestsCoroutine(callback));
        }

        private IEnumerator LoadAvailableQuestsCoroutine(Action<bool>? callback)
        {
            bool success = false;

            yield return ApiClient.Instance.Get(
                "/quest",
                onSuccess: response =>
                {
                    try
                    {
                        // Unity JsonUtility nepodporuje přímo pole, takže musíme workaround
                        string wrappedJson = $"{{\"quests\":{response}}}";
                        var questList = JsonUtility.FromJson<QuestListResponse>(wrappedJson);
                        
                        _availableQuests.Clear();
                        _availableQuests.AddRange(questList.quests);
                        
                        success = true;
                        Debug.Log($"Loaded {_availableQuests.Count} available quests");
                        OnQuestsLoaded?.Invoke(_availableQuests);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to parse quests: {ex.Message}");
                        OnQuestError?.Invoke($"Failed to parse quests: {ex.Message}");
                    }
                },
                onError: error =>
                {
                    Debug.LogError($"Failed to load quests: {error}");
                    OnQuestError?.Invoke(error);
                });

            callback?.Invoke(success);
        }

        /// <summary>
        /// Dokončí quest a přidá hráči odměnu
        /// </summary>
        public void CompleteQuest(int questId, Action<bool, QuestCompletionResponse?>? callback = null)
        {
            StartCoroutine(CompleteQuestCoroutine(questId, callback));
        }

        private IEnumerator CompleteQuestCoroutine(int questId, Action<bool, QuestCompletionResponse?>? callback)
        {
            QuestCompletionResponse? completionData = null;
            bool success = false;

            yield return ApiClient.Instance.Post(
                $"/quest/complete/{questId}",
                "", // Empty body
                onSuccess: response =>
                {
                    try
                    {
                        completionData = JsonUtility.FromJson<QuestCompletionResponse>(response);
                        success = true;
                        
                        Debug.Log($"Quest completed! Gained {completionData.experienceGained} XP. Total XP: {completionData.totalXP}");
                        
                        // Odstraň quest ze seznamu dostupných
                        _availableQuests.RemoveAll(q => q.id == questId);
                        
                        OnQuestCompleted?.Invoke(completionData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Failed to parse quest completion response: {ex.Message}");
                        OnQuestError?.Invoke($"Failed to parse response: {ex.Message}");
                    }
                },
                onError: error =>
                {
                    Debug.LogError($"Failed to complete quest: {error}");
                    OnQuestError?.Invoke(error);
                });

            callback?.Invoke(success, completionData);
        }

        /// <summary>
        /// Získá quest podle ID
        /// </summary>
        public QuestResponse? GetQuestById(int questId)
        {
            return _availableQuests.Find(q => q.id == questId);
        }

        /// <summary>
        /// Zkontroluje, zda je quest dostupný
        /// </summary>
        public bool IsQuestAvailable(int questId)
        {
            return _availableQuests.Exists(q => q.id == questId);
        }
    }
}