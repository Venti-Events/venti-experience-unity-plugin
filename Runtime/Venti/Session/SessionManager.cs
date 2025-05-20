using System;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace Venti
{
    public class SessionManager : Singleton<SessionManager>
    {
        public UnityEvent<Session> onSessionStart;
        public UnityEvent onSessionEnd;

        private Session session;

        private const string checkInAppUrl = @"https://venti-checkin.web.app";
        private const string getSessionUrl = @"/app-sessions";
        private const string getAttendeeUrl = @"/get-attendee-with-appKey";
        private const string endSessionUrl = @"/end-app-session";
        private const string endAllActiveSessionsUrl = @"/end-all-active-sessions";

        public void FetchSession(string sessionId)
        {
            Debug.Log("Fetching session: " + sessionId);
            StartCoroutine(FetchSessionCoroutine(sessionId));
        }

        private IEnumerator FetchSessionCoroutine(string sessionId)
        {
            using (VentiApiRequest www = VentiApiRequest.GetApi($"{getSessionUrl}/{sessionId}"))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                    Debug.LogError("Error fetching session: " + www.error);
                else
                {
                    Debug.Log("Session fetched: " + www.downloadHandler.text);
                    JSONObject sessionJson = JSON.Parse(www.downloadHandler.text).AsObject;
                    session.SetFromJson(sessionJson["data"].AsObject);

                    yield return FetchAttendeeCoroutine(sessionJson["attendee_id"].Value);
                }
            }
        }

        private IEnumerator FetchAttendeeCoroutine(string attendeeId)
        {
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

        public void EndSession(int score)
        {
            StartCoroutine(EndSessionCoroutine(score));
        }

        public void ResetSessions()
        {
            StartCoroutine(EndAllActiveSessionsCoroutine());
        }

        public string GetCheckInAppUrl(string returnUrl, string roomId)
        {
            Uri abc = new Uri(checkInAppUrl);
            // abc.Query = "appKey=" + appKey;
            return abc.ToString();
        }

        public IEnumerator EndSessionCoroutine(int score)
        {
            JSONObject dataJson = new JSONObject();
            dataJson["score"] = score;

            using (VentiApiRequest www = VentiApiRequest.PutApi($"{endSessionUrl}/{session.id}", dataJson.ToString()))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                    Debug.LogError("Error ending session: " + www.error);
                else
                {
                    Debug.Log("Session ended: " + www.downloadHandler.text);
                    onSessionEnd.Invoke();
                }
            }
        }

        private IEnumerator EndAllActiveSessionsCoroutine()
        {
            using (VentiApiRequest www = VentiApiRequest.GetApi(endAllActiveSessionsUrl))
            {
                yield return www.SendAuthenticatedApiRequest();

                if (www.result != VentiApiRequest.Result.Success)
                    Debug.LogError("Error ending all active sessions: " + www.error);
                else
                {
                    Debug.Log("All active sessions ended: " + www.downloadHandler.text);
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
        // public string appId;
        public Attendee attendee;
        public string sessionStartTimestamp;
        public string sessionEndTimestamp;
        public string data;
        public string createdAt;
        public string updatedAt;

        public void SetFromJson(JSONObject sessionJson)
        {
            id = sessionJson["id"].Value;
            // appId = sessionJson["app_id"].Value;
            // attendee = JsonUtility.FromJson<Attendee>(attendeeJson.ToString());
            attendee = new Attendee();
            sessionStartTimestamp = sessionJson["session_start_timestamp"].Value;
            sessionEndTimestamp = sessionJson["session_end_timestamp"].Value;
            data = sessionJson["data"].Value;
            createdAt = sessionJson["created_at"].Value;
            updatedAt = sessionJson["updated_at"].Value;
        }
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
