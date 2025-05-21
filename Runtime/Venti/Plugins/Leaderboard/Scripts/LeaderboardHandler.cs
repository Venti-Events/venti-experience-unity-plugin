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

        public UnityEvent onLeaderboardLoaded;

        [SerializeField] private List<LeaderboardItem> leaderboardItems = new List<LeaderboardItem>();
        private int currentAttendeeRank = -1;
        private int currentAttendeeScore = 0;

        private const string leaderboardUrl = "/project-attendee-leaderboard/get-by-appkey";
        private const int itemsLimit = 10;

        void Start()
        {
            LoadLeaderboard(0);
        }

        public void ShowLeaderboard()
        {
            leaderboardPanel.gameObject.SetActive(true);
        }

        public void HideLeaderboard()
        {
            leaderboardPanel.gameObject.SetActive(false);
        }

        public void LoadLeaderboard(int currentScore)
        {
            currentAttendeeScore = currentScore;
            StartCoroutine(LoadLeaderboardCoroutine());
        }

        private IEnumerator LoadLeaderboardCoroutine()
        {
            // Fetch leaderboard
            yield return FetchLeaderboardCoroutine();

            // Render leaderboard
            RenderLeadboard();
        }

        private IEnumerator FetchLeaderboardCoroutine()
        {
            string attendeeId = SessionManager.Instance?.session?.attendee?.id;

            VentiApiRequest www;
            if (attendeeId == null)
            {
                currentAttendeeRank = -1;
                currentAttendeeScore = 0;
                www = VentiApiRequest.GetApi(leaderboardUrl + "?limit=" + itemsLimit);
                Debug.Log("Leaderboard URL: " + leaderboardUrl + "?limit=" + itemsLimit);
            }
            else
            {
                www = VentiApiRequest.GetApi(leaderboardUrl + "?limit=" + itemsLimit + "&attendeeId=" + attendeeId);
                Debug.Log("Leaderboard URL: " + leaderboardUrl + "?limit=" + itemsLimit + "&attendeeId=" + attendeeId);
            }

            yield return www.SendAuthenticatedApiRequest();

            if (www.result != VentiApiRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch leaderboard");
                yield break;
            }

            Debug.Log("Leaderboard fetched: " + www.downloadHandler.text);

            // parse response
            JSONObject responseJson = JSON.Parse(www.downloadHandler.text).AsObject;
            if (attendeeId != null)
            {
                JSONObject attendeeData = responseJson["data"]["currentUserData"].AsObject;
                currentAttendeeRank = attendeeData["rank"].AsInt;
                Debug.Log("Attendee Rank: " + currentAttendeeRank);
            }

            JSONArray leaderboardData = responseJson["data"]["leaderboardData"].AsArray;
            leaderboardItems.Clear();

            for (int i = 0; i < leaderboardData.Count; i++)
            {
                JSONObject item = leaderboardData[i].AsObject;
                LeaderboardItem leaderboardItem = new LeaderboardItem();
                leaderboardItem.rank = i + 1;
                leaderboardItem.name = item["first_name"].Value;
                leaderboardItem.score = item["data"]["score"].AsInt;
                leaderboardItem.sessionId = item["id"].Value;
                if (currentAttendeeRank == i + 1)
                    leaderboardItem.isCurrent = true;

                leaderboardItems.Add(leaderboardItem);
            }

            if (attendeeId != null && currentAttendeeRank > itemsLimit)
            {
                LeaderboardItem leaderboardItem = new LeaderboardItem();
                leaderboardItem.rank = currentAttendeeRank;
                leaderboardItem.name = SessionManager.Instance.session.attendee.firstName;
                leaderboardItem.score = currentAttendeeScore;
                leaderboardItem.sessionId = SessionManager.Instance.session.id;
                leaderboardItem.isCurrent = true;

                leaderboardItems.Add(leaderboardItem);
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