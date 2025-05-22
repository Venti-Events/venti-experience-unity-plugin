using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Venti.Plugins.Leaderboard
{
    public class LeaderboardHandler : MonoBehaviour
    {
        public Transform leaderboardPanel;
        public GameObject itemPrefab;
        public Transform itemsContainer;

        [SerializeField] private LeaderboardItem currentAttendee;
        [SerializeField] private List<LeaderboardItem> leaderboardItems = new List<LeaderboardItem>();
        public UnityEvent onLeaderboardLoaded;

        private const string leaderboardUrl = "/project-attendee-leaderboard/get-by-appkey";
        private const int itemsLimit = 10;

        // void OnEnable()
        // {
        //     LoadLeaderboard();
        // }

        public void ShowLeaderboard()
        {
            leaderboardPanel.gameObject.SetActive(true);
            leaderboardPanel.GetComponent<CanvasGroup>().alpha = 1f;
        }

        public void HideLeaderboard()
        {
            leaderboardPanel.GetComponent<CanvasGroup>().alpha = 0;
            leaderboardPanel.gameObject.SetActive(false);
        }

        public void LoadLeaderboard(string currentAttendeeId = null)
        {
            StartCoroutine(LoadLeaderboardCoroutine(currentAttendeeId));
        }

        private IEnumerator LoadLeaderboardCoroutine(string currentAttendeeId = null)
        {
            // Fetch leaderboard
            yield return FetchLeaderboardCoroutine(currentAttendeeId);

            // Render leaderboard
            RenderLeadboard();
        }

        private IEnumerator FetchLeaderboardCoroutine(string currentAttendeeId = null)
        {
            string attendeeId = currentAttendeeId ?? SessionManager.Instance?.session?.attendee?.id;

            VentiApiRequest www;
            if (attendeeId == null)
            {
                currentAttendee = null;
                www = VentiApiRequest.GetApi(leaderboardUrl + "?limit=" + itemsLimit);
                // Debug.Log("Leaderboard URL: " + leaderboardUrl + "?limit=" + itemsLimit);
            }
            else
            {
                www = VentiApiRequest.GetApi(leaderboardUrl + "?limit=" + itemsLimit + "&attendeeId=" + attendeeId);
                // Debug.Log("Leaderboard URL: " + leaderboardUrl + "?limit=" + itemsLimit + "&attendeeId=" + attendeeId);
            }

            yield return www.SendAuthenticatedApiRequest();

            if (www.result != VentiApiRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch leaderboard");
                yield break;
            }

            // Debug.Log("Leaderboard fetched: " + www.downloadHandler.text);

            // parse response
            JSONObject responseJson = JSON.Parse(www.downloadHandler.text)["data"].AsObject;
            if (attendeeId != null)
            {
                JSONObject attendeeData = responseJson["currentUserData"].AsObject;
                currentAttendee = new LeaderboardItem();
                currentAttendee.rank = attendeeData["rank"].AsInt;
                currentAttendee.name = attendeeData["user"]["first_name"].Value;
                currentAttendee.score = attendeeData["user"]["data"]["score"].AsInt;
                currentAttendee.sessionId = attendeeData["user"]["id"].Value;
                currentAttendee.isCurrent = true;
            }

            JSONArray leaderboardData = responseJson["leaderboardData"].AsArray;
            leaderboardItems.Clear();

            for (int i = 0; i < leaderboardData.Count; i++)
            {
                JSONObject item = leaderboardData[i].AsObject;
                LeaderboardItem leaderboardItem = new LeaderboardItem();
                leaderboardItem.rank = i + 1;
                leaderboardItem.name = item["first_name"].Value;
                leaderboardItem.score = item["data"]["score"].AsInt;
                leaderboardItem.sessionId = item["id"].Value;
                if (currentAttendee != null && currentAttendee.rank == i + 1)
                    leaderboardItem.isCurrent = true;

                leaderboardItems.Add(leaderboardItem);
            }

            if (currentAttendee != null && currentAttendee.rank > itemsLimit)
            {
                leaderboardItems.Add(currentAttendee);
            }
        }

        void RenderLeadboard()
        {
            // delete all older entries
            foreach (Transform item in itemsContainer)
                Destroy(item.gameObject);

            foreach (LeaderboardItem leaderboardItem in leaderboardItems)
            {
                GameObject rowObj = Instantiate(itemPrefab, itemsContainer);
                LeaderboardItemHandler itemHandler = rowObj.GetComponent<LeaderboardItemHandler>();
                itemHandler.UpdateInfo(leaderboardItem);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(leaderboardPanel.GetComponent<RectTransform>());

            onLeaderboardLoaded.Invoke();
        }
    }
}