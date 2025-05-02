using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SocketIOClient;
using SocketIO.Core;


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
        client = new SocketIOClient.SocketIO(serverIp, new SocketIOOptions());

        client = new SocketIOClient.SocketIO(serverIp, new SocketIOOptions
        {
            Auth = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("token", appKey),
    }
        });

        client.On("configHash", response =>
        {
            // You can print the returned data first to decide what to do next.
            // output: ["hi client"]

            Debug.Log(response);

            string text = response.GetValue<string>();

            Debug.Log("configHash: " + text);

            // The socket.io server code looks like this:
            // socket.emit('hi', 'hi client');
        });

        client.OnConnected += async (sender, e) =>
        {

            Debug.Log("Connected to server!");
            // Emit a string
            //await client.EmitAsync("hi", "socket.io");

            // Emit a string and an object
            //var dto = new TestDTO { Id = 123, Name = "bob" };
            //await client.EmitAsync("register", "source", dto);
        };
        client.ConnectAsync();
    }


}
