using System;
using System.Collections;
using Helpers;
using Models;
using UnityEngine;

namespace Networking
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
    public class PlayerData
    {
        public int id;
        public string username = "";
        public int level;
        public int experience;
    }

    public class AuthManager : MonoBehaviour
    {
        [SerializeField] private string testUsername = "testuser";
        [SerializeField] private string testPassword = "Password123";
        
        private void Start()
        {
            StartCoroutine(Login(testUsername, testPassword));
        }
        
        private IEnumerator Login (string username, string password)
        {
            var requestData  = new LoginRequest
            {
                username = username,
                password = password
            };
            
            string json = JsonUtility.ToJson(requestData);
            
            yield return ApiClient.Instance.Post("/api/auth/login", json,
                onSuccess: response =>
                {
                    var loginResponse = JsonUtility.FromJson<LoginResponse>(response);
                    Debug.Log($"Token: {loginResponse.token}");
                    ApiClient.Instance.SetToken(loginResponse.token);
                    StartCoroutine(GetPlayerData());
                    StartCoroutine(GetAvailableQuests());
                },
                onError: error =>
                {
                    Debug.LogError("Login failed: " + error);
                });

          
        }
        
        private IEnumerator GetPlayerData()
        {
            yield return ApiClient.Instance.Get("/api/player/me",
                onSuccess: response =>
                {
                    var player = JsonUtility.FromJson<PlayerData>(response);
                    Debug.Log($"Logged in as {player.username}, Level: {player.level}, XP: {player.experience}");
                },
                onError: error =>
                {
                    Debug.LogError("Failed to load player data: " + error);
                });
        }
        
        private IEnumerator GetAvailableQuests()
        {
            yield return ApiClient.Instance.Get("/api/quest",
                onSuccess: response =>
                {
                    var wrapper = JsonArrayWrapper<QuestData>.FromJson(response);
                    foreach (var quest in wrapper.items)
                    {
                        //Debug.Log($"Quest {quest.id}: {quest.title} ({quest.rewardXP} XP)");
                    }
                },
                onError: error =>
                {
                    Debug.LogError("Failed to load quests: " + error);
                });
        }
    }
    
   
}