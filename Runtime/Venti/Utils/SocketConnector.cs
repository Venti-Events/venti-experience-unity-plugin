using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PimDeWitte.UnityMainThreadDispatcher;
using Venti.Experience;
using Venti.Theme;

namespace Venti
{
    public class SocketConnector : IDisposable
    {
        private SocketIOClient.SocketIO client;

        public SocketConnector(string serverIp, string appKey)
        {
            ConnectToServer(serverIp, appKey);
        }

        ~SocketConnector()
        {
            Disconnect();
        }

        private void ConnectToServer(string serverIp, string appKey)
        {
            Debug.Log("Connecting to socket server at " + serverIp + " with appKey: " + appKey);

            client = new SocketIOClient.SocketIO(serverIp, new SocketIOClient.SocketIOOptions
            {
                Auth = new Dictionary<string, string>
            {
                { "token", appKey }
            }
            });

            // On app connection, receive hash for appConfig and themeConfig
            client.On("configHash", response =>
            {
                string configHashJsonStr = response.GetValue<System.Text.Json.JsonElement>().ToString();
                Debug.Log("configHash: " + configHashJsonStr);

                UnityMainThreadDispatcher.Instance().Enqueue(ParseHashesJson(configHashJsonStr));
            });

            // On app config change, receive hash
            client.On("appHash", response =>
            {
                //string hash = response.GetValue<System.Text.Json.JsonElement>().ToString();
                string hash = response.GetValue<string>();
                Debug.Log("appHash: " + hash);

                UnityMainThreadDispatcher.Instance().Enqueue(FetchAppConfig(hash));
            });

            client.On("themeHash", response =>
            {
                //string hash = response.GetValue<System.Text.Json.JsonElement>().ToString();
                string hash = response.GetValue<string>();
                Debug.Log("themeHash: " + hash);

                UnityMainThreadDispatcher.Instance().Enqueue(FetchThemeConfig(hash));
            });

            client.On("sessionStarted", response =>
            {
                string sessionStartJsonStr = response.GetValue<System.Text.Json.JsonElement>().ToString();
                Debug.Log("Session started: " + sessionStartJsonStr);

                UnityMainThreadDispatcher.Instance().Enqueue(FetchSessionAttendee(sessionStartJsonStr));
            });

            client.OnConnected += (sender, e) =>
            {
                Debug.Log("Connected to server!");
            };

            client.OnDisconnected += (sender, e) =>
            {
                Debug.Log("Disconnected from server!");
            };

            client.OnError += (sender, e) =>
            {
                Debug.Log("Socket error: " + e.ToString());
            };

            client.ConnectAsync();
        }

        IEnumerator ParseHashesJson(string jsonResponse)
        {
            VentiManager.Instance?.ParseHashesJson(jsonResponse);
            yield return null;
        }

        IEnumerator FetchAppConfig(string hash)
        {
            ExperienceManager.Instance?.FetchAppConfig(hash);
            yield return null;
        }

        IEnumerator FetchThemeConfig(string hash)
        {
            ThemeManager.Instance?.FetchThemeConfig(hash);
            yield return null;
        }

        IEnumerator FetchSessionAttendee(string jsonResponse)
        {
            SessionManager.Instance?.FetchSessionAttendee(jsonResponse);
            yield return null;
        }

        void Disconnect()
        {
            if (client != null && client.Connected)
            {
                client.DisconnectAsync();
                client = null;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}