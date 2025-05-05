using System.Collections.Generic;
using UnityEngine;
using Venti;

public class SocketConnector
{
    //private string serverIp = "https://venti-server-nestjs-128798841108.us-central1.run.app";

    //private string appKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJwcmoiOiI1ZjQwNjBiZi04NjdkLTQ0MDctYmQ5NC0yZTBjMWU4YmFkY" +
    //    "mIiLCJhcHAiOiJkYWQxYzBmMC0zY2MzLTRiMDQtYjE4YS01ZDg4ZTM2YWRlNjkiLCJqdGkiOiJlM2FkNDIzNy03ZDA4LTQ4MzItOGU3Yi1jYmE3OT" +
    //    "BjNmIwMWMiLCJydGMiOjAsImlzcyI6InNlcnZlciIsImlhdCI6MTc0NjE4ODY1OSwiZXhwIjoxNzQ2ODM1MjAwLCJsdmEiOjE3NDU5NzEyMDAsIm1v" +
    //    "ZCI6ImRlbW8ifQ.NhemXduwgdi6kb2z5R0bIM6WDYoP8XN-mJun-svhNqA";

    private SocketIOClient.SocketIO client;

    private void ConnectToServer(string serverIp, string appKey)
    {
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
            Debug.Log("configHash: " + response);
            SettingsManager.Instance.ParseHashesJson(response.ToString());
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

    void OnApplicationQuit()
    {
        client.DisconnectAsync();
    }
}
