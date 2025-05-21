using System;
using System.Collections;
using System.Web;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace Venti
{
    public class SessionManager : Singleton<SessionManager>
    {
        public UnityEvent<Session> onSessionStart;
        public UnityEvent onSessionEnd;

        public bool isSessionActive { get; private set; }
        // public string sessionId { get; private set; }
        // public string attendeeId {get; private set; }
        // public Attendee attendee { get; private set; }
        private Session session = new Session();

        private const string checkInAppUrl = @"https://venti-checkin.web.app";
        // private const string getSessionUrl = @"/app-sessions";
        private const string getActiveSessionUrl = @"/project-app-session/app-active-sessions-appkey";
        private const string getAttendeeUrl = @"/attendee/get-attendee-with-appkey";
        private const string endSessionUrl = @"/project-app-session/end-app-session-appkey";
        private const string endAllSessionsUrl = @"/project-app-session/end-all-sessions-appkey";

        public void FetchActiveSession()
        {
            StartCoroutine(FetchActiveSessionCoroutine());
        }

        public void FetchSessionAttendee(string jsonString)
        {
            JSONObject sessionAttendeeJson = JSON.Parse(jsonString).AsObject;
            session.id = sessionAttendeeJson["sessionId"].Value;
            string attendeeId = sessionAttendeeJson["attendeeId"].Value;

            Debug.Log("Session ID: " + session.id);
            Debug.Log("Attendee ID: " + attendeeId);

            StartCoroutine(FetchAttendeeCoroutine(attendeeId));
        }

        public void EndSession()
        {
            StartCoroutine(EndSessionCoroutine("{}"));
        }

        public void EndSession(int score)
        {
            JSONObject dataJson = new JSONObject();
            dataJson["score"] = score;

            StartCoroutine(EndSessionCoroutine(dataJson.ToString()));
        }

        public void ResetSessions()
        {
            StartCoroutine(EndAllSessionsCoroutine());
        }

        public string GetCompanionTabUrl()
        {
            TokenPayload token = TokenManager.Instance.GetTokenPayload();
            if (token == null)
            {
                Debug.LogError("No token found");
                return null;
            }

            // Build the check-in app URL
            string checkInAppVerificationUrl = checkInAppUrl + "/verification/" + token.projectId + "/" + token.appId;
            UriBuilder checkInAppUrlBuilder = new UriBuilder(checkInAppVerificationUrl);

            return checkInAppUrlBuilder.Uri.AbsolutePath;
        }

        public string GetCheckInAppUrl(string redirectBaseUrl, string roomId)
        {
            TokenPayload token = TokenManager.Instance.GetTokenPayload();
            if (token == null)
            {
                Debug.LogError("No token found");
                return null;
            }

            // Build the check-in app URL
            string checkInAppVerificationUrl = checkInAppUrl + "/verification/" + token.projectId + "/" + token.appId;
            UriBuilder checkInAppUrlBuilder = new UriBuilder(checkInAppVerificationUrl);

            if (redirectBaseUrl != null)
            {
                // Add roomId as a query parameter to any existing query parameters
                UriBuilder redirectUriBuilder = new UriBuilder(redirectBaseUrl);
                if (roomId != null)
                {
                    if (redirectUriBuilder.Query != null)
                        redirectUriBuilder.Query += "&roomId=" + roomId; // (ss: skip session) do not create a new session before redirecting
                    else
                        redirectUriBuilder.Query = "roomId=" + roomId;
                }
                string redirectUrl = redirectUriBuilder.Uri.AbsoluteUri;

                checkInAppUrlBuilder.Query = "ru=" + HttpUtility.UrlEncode(redirectUrl);
                checkInAppUrlBuilder.Query += "&sv=true";           // (sv: save) denote we are opening the app from ateendee's phone (save in LocalStorage)
                if (roomId != null)
                    checkInAppUrlBuilder.Query += "&ss=true";
            }

            return checkInAppUrlBuilder.Uri.AbsoluteUri;
        }

        private IEnumerator FetchActiveSessionCoroutine()
        {
            using (VentiApiRequest www = VentiApiRequest.GetApi($"{getActiveSessionUrl}"))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                    Debug.LogError("Error fetching active session: " + www.error);
                else
                {
                    Debug.Log("Active session fetched: " + www.downloadHandler.text);
                    JSONObject sessionJson = JSON.Parse(www.downloadHandler.text).AsObject;
                    JSONArray data = sessionJson["data"].AsArray;
                    if (data.Count == 0)
                    {
                        Debug.Log("No active session found.");
                        yield break;
                    }

                    JSONObject sessionData = data[0].AsObject;
                    session.id = sessionData["session_id"];
                    string attendeeId = sessionData["attendee_id"];

                    yield return FetchAttendeeCoroutine(attendeeId);
                }
            }
        }

        // private IEnumerator FetchSessionCoroutine(string sessionId)
        // {
        //     using (VentiApiRequest www = VentiApiRequest.GetApi($"{getSessionUrl}/{sessionId}"))
        //     {
        //         yield return www.SendAuthenticatedApiRequest();

        //         if (www.result != VentiApiRequest.Result.Success)
        //             Debug.LogError("Error fetching session: " + www.error);
        //         else
        //         {
        //             Debug.Log("Session fetched: " + www.downloadHandler.text);
        //             JSONObject sessionJson = JSON.Parse(www.downloadHandler.text).AsObject;
        //             session.SetFromJson(sessionJson["data"].AsObject);

        //             yield return FetchAttendeeCoroutine(sessionJson["attendee_id"].Value);
        //         }
        //     }
        // }

        private IEnumerator FetchAttendeeCoroutine(string attendeeId)
        {
            if (attendeeId == null)
            {
                Debug.LogError("Attendee ID is null.");
                yield break;
            }

            using (VentiApiRequest www = VentiApiRequest.GetApi($"{getAttendeeUrl}/{attendeeId}"))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                    Debug.LogError("Error fetching attendee: " + www.error);
                else
                {
                    Debug.Log("Attendee fetched: " + www.downloadHandler.text);
                    JSONObject attendeeJson = JSON.Parse(www.downloadHandler.text).AsObject;
                    session.attendee.SetFromJson(attendeeJson["data"].AsObject);

                    onSessionStart.Invoke(session);
                }
            }
        }

        private IEnumerator EndSessionCoroutine(string dataJsonStr)
        {
            if (session.id == null)
            {
                Debug.LogError("No session ID found.");
                yield break;
            }

            Debug.Log("Ending session: " + session.id);

            using (VentiApiRequest www = VentiApiRequest.PutApi($"{endSessionUrl}/{session.id}", dataJsonStr))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                    Debug.LogError("Error ending session: " + www.error);
                else
                {
                    Debug.Log("Session ended: " + www.downloadHandler.text);
                    onSessionEnd.Invoke();

                    FetchActiveSession();
                }
            }
        }

        private IEnumerator EndAllSessionsCoroutine()
        {
            using (VentiApiRequest www = VentiApiRequest.PostApi(endAllSessionsUrl, new WWWForm()))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                    Debug.LogError("Error ending all active sessions: " + www.error);
                else
                {
                    Debug.Log("All active sessions ended: " + www.downloadHandler.text);
                    onSessionEnd.Invoke();

                    FetchActiveSession();
                }
            }
        }

        // public void EndSessionCoroutine(Texture2D photo)
        // {
        //     using (VentiApiRequest www = VentiApiRequest.Put($"{endSessionUrl}/{session.id}", ))
        //     {
        //         yield return www.SendAuthenticatedApiRequest();
        //     }
        // }

        // public void EndSessionCoroutine(int score, Texture2D photo)
        // {
        //     using (VentiApiRequest www = VentiApiRequest.Put($"{endSessionUrl}/{session.id}", ))
        //     {
        //         yield return www.SendAuthenticatedApiRequest();
        //     }
        // }

    }

    public class Session
    {
        public string id;
        public Attendee attendee = new Attendee();
        // public string sessionStartTimestamp;
        // public string sessionEndTimestamp;
        // public string data;
        // public string createdAt;
        // public string updatedAt;

        // public void SetFromJson(JSONObject sessionJson)
        // {
        //     id = sessionJson["id"].Value;
        //     // attendee = JsonUtility.FromJson<Attendee>(attendeeJson.ToString());
        //     attendee = new Attendee();
        //     sessionStartTimestamp = sessionJson["session_start_timestamp"].Value;
        //     sessionEndTimestamp = sessionJson["session_end_timestamp"].Value;
        //     data = sessionJson["data"].Value;
        //     createdAt = sessionJson["created_at"].Value;
        //     updatedAt = sessionJson["updated_at"].Value;
        // }
    }

    public class Attendee
    {
        public string id;
        public string firstName;
        // public string email;
        // public string form_fields_data;
        public string categoryId;
        public bool isActive;
        public string createdAt;
        public string updatedAt;

        public void SetFromJson(JSONObject attendeeJson)
        {
            id = attendeeJson["id"].Value;
            firstName = attendeeJson["first_name"].Value;
            categoryId = attendeeJson["category_id"].Value;
            isActive = attendeeJson["is_active"].AsBool;
            createdAt = attendeeJson["created_at"].Value;
            updatedAt = attendeeJson["updated_at"].Value;
        }
    }
}
