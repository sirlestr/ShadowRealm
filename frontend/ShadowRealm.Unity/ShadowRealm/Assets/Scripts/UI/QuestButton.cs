#nullable enable
using Models;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace UI
{
    public class QuestButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText = null!;
        [SerializeField] private Button completeButton = null!;

        private int questId;

        public void Setup(QuestData quest)
        {
            questId = quest.id;
            titleText.text = $"{quest.title} ({quest.rewardXP} XP)";
            completeButton.onClick.AddListener(CompleteQuest);
        }

        private void CompleteQuest()
        {
            Debug.Log($"Completing quest ID: {questId}");
            ApiClient.Instance.Post($"/api/quest/complete/{questId}", "",
                onSuccess: response =>
                {
                    Debug.Log("Quest completed: " + response);
                },
                onError: error =>
                {
                    Debug.LogError("Failed to complete quest: " + error);
                });
        }
    }
}