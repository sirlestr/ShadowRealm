#nullable enable
using System.Collections;
using Helpers;
using Models;
using ShadowRealm.Networking;
using UnityEngine;
namespace UI
{
    public class QuestListUI : MonoBehaviour
    {
        [SerializeField] private Transform listContainer = null!;
        [SerializeField] private GameObject questButtonPrefab = null!;

        private void OnEnable()
        {
            StartCoroutine(LoadQuests());
        }

        private IEnumerator LoadQuests()
        {
            if (questButtonPrefab == null)
            {
                Debug.LogError("QuestButton prefab is not assigned!");
            }
            if (listContainer == null)
            {
                Debug.LogError("List container is not assigned!");
            }

            yield return  ApiClient.Instance.Get("/api/quest",
                onSuccess: response =>
                {
                    Debug.Log("Raw quest response: " + response);
                    var wrapper = JsonArrayWrapper<QuestData>.FromJson(response);
                    foreach (var quest in wrapper.items)
                    {
                        var go = Instantiate(questButtonPrefab, listContainer);
                        var qb = go.GetComponent<QuestButton>();
                        qb.Setup(quest);
                    }
                },
                onError: error =>
                {
                    Debug.LogError("Failed to load quests: " + error);
                });
        }
    }
}