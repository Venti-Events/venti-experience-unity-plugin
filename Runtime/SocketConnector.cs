using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Venti;
using PimDeWitte.UnityMainThreadDispatcher;

public class SocketConnector: IDisposable
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
        Debug.Log("Connecting to server at " + serverIp + " with appKey: " + appKey);

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
            //UnityMainThreadDispatcher.Instance().Enqueue(ThisWillBeExecutedOnTheMainThread(dto));
        });

        // On app config change, receive hash
        client.On("appHash", response =>
        {
            string hash = response.ToString();
            Debug.Log("appHash: " + hash);

            SettingsManager.Instance.FetchAppConfig(hash);
        });

        client.On("themeHash", response =>
        {
            string hash = response.ToString();
            Debug.Log("themeHash: " + hash);

            SettingsManager.Instance.FetchThemeConfig(hash);
        });

        //client.On("transform", response =>
        //{
        //    try
        //    {
        //        Debug.Log("response: " + response);
        //        string responseStr = response.ToString();
        //        Debug.Log("responseStr: " + responseStr);

        //        JSONNode parsedJson = SimpleJSON.JSON.Parse(responseStr)[0];

        //        TestDTO dto = new TestDTO
        //        {
        //            name = parsedJson["name"],
        //            rotation = parsedJson["rotation"].AsInt,
        //            scale = parsedJson["scale"].AsFloat
        //        };

        //        // TestDTO dto = response.GetValue<TestDTO>(1);
        //        Debug.Log(dto.name);
        //        Debug.Log(dto.rotation);
        //        Debug.Log(dto.scale);

        //        UnityMainThreadDispatcher.Instance().Enqueue(ThisWillBeExecutedOnTheMainThread(dto));
        //    }
        //    catch (System.Exception e)
        //    {
        //        Debug.LogError("Transform error: " + e);
        //    }
        //});

        client.OnConnected += async (sender, e) =>
        {
            Debug.Log("Connected to server!");
        };
        client.ConnectAsync();
    }

    //public IEnumerator ThisWillBeExecutedOnTheMainThread(TestDTO dto)
    //{
    //    Debug.Log("Dto received in the main thread: " + dto.name);
    //    testObj.transform.rotation = Quaternion.Euler(0, dto.rotation, 0);
    //    testObj.transform.localScale = Vector3.one * dto.scale;
    //    yield return null;
    //}

    IEnumerator ParseHashesJson(string jsonResponse)
    {
        Debug.Log("Received hashes JSON: " + jsonResponse);
        SettingsManager.Instance.ParseHashesJson(jsonResponse);
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

    void OnApplicationQuit()
    {
        Disconnect();
    }

    public void Dispose()
    {
        Disconnect();
    }
}
