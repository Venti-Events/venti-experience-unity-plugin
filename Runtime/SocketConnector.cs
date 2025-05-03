using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIO.Core;
using PimDeWitte.UnityMainThreadDispatcher;


public class SocketConnector : MonoBehaviour
{
    public string serverIp = "https://venti-server-nestjs-128798841108.us-central1.run.app";

    public string appKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcmoiOiI1ZjQwNjBiZi04NjdkLTQ0MDctYmQ5NC0yZTBjMWU4YmFkY" +
        "mIiLCJhcHAiOiJkYWQxYzBmMC0zY2MzLTRiMDQtYjE4YS01ZDg4ZTM2YWRlNjkiLCJqdGkiOiJlM2FkNDIzNy03ZDA4LTQ4MzItOGU3Yi1jYmE3OT" +
        "BjNmIwMWMiLCJydGMiOjAsImlzcyI6InNlcnZlciIsImlhdCI6MTc0NjE4ODY1OSwiZXhwIjoxNzQ2ODM1MjAwLCJsdmEiOjE3NDU5NzEyMDAsIm1v" +
        "ZCI6ImRlbW8ifQ.NhemXduwgdi6kb2z5R0bIM6WDYoP8XN-mJun-svhNqA";

    private SocketIOClient.SocketIO client;

    private void Start()
    {
        // Initialize the socket connection here
        ConnectToServer();
    }

    private void ConnectToServer()
    {
        //client = new SocketIOClient.SocketIO(serverIp, new SocketIOOptions());
        //client = new SocketIOClient.SocketIO(serverIp, new SocketIOOptions
        //{
        //    Query = new List<KeyValuePair<string, string>>
        //    {
        //        new KeyValuePair<string, string>("token", appKey),
        //    }
        //});

        client = new SocketIOClient.SocketIO(serverIp, new SocketIOClient.SocketIOOptions
        {
            Auth = new Dictionary<string, string>
            {
                { "token", appKey }
            }
        });

        client.On("configHash", response =>
        {
            Debug.Log("configHash Response: " + response);

            string text = response.GetValue<string>();

            Debug.Log("configHash Text: " + text);
        });

        client.On("appConfig", response =>
        {
            Debug.Log("appConfig Response: " + response);

            string text = response.GetValue<string>();

            Debug.Log("appConfig Text: " + text);
        });

        client.On("themeConfig", response =>
        {
            Debug.Log("themeConfig Response: " + response);

            string text = response.GetValue<string>();

            Debug.Log("themeConfig Text: " + text);
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

    void OnApplicationQuit()
    {
        client.DisconnectAsync();
    }
}
