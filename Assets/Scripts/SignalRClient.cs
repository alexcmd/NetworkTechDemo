using System;

using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using UnityEngine;
using WebSocketSharp;

namespace Assets.Scripts
{
    class SignalRClient
    {
        private WebSocket _ws;
        private string _connectionToken="";
        private Dictionary<string, UnTypedActionContainer> _actionMap;

        private readonly string _socketUrl = "http://localhost:42282/";

        private readonly string _socket = "ws://localhost:42282/";

        public SignalRClient()
        {
            _actionMap = new Dictionary<string, UnTypedActionContainer>();
            var webRequest = (HttpWebRequest)WebRequest.Create(_socketUrl + "/signalr/negotiate?connectionData=%5B%7B%22name%22%3A%22myHub1%22%7D%5D&clientProtocol=1.4");
            var response = (HttpWebResponse)webRequest.GetResponse();

            using (var sr = new StreamReader(response.GetResponseStream()))
            {
                var payload = sr.ReadToEnd();

                Debug.Log(payload);
                var obj = JsonConvert.DeserializeObject<NegotiateResponse>(payload);
                _connectionToken = Uri.EscapeDataString(obj.ConnectionToken);

                UnityEngine.Debug.Log(_connectionToken);

            }
        }

        public void Open()
        {
            _ws = _ws == null
                ? new WebSocket(_socket + "signalr/connect?transport=webSockets&connectionToken=" + _connectionToken)
                : new WebSocket(_socket + "signalr/reconnect?transport=webSockets&connectionToken=" + _connectionToken);

            AttachAndConnect();
        }

        public void Close()
        {
            _ws.Close();
        }

        public void SendMessage(string name, string message)
        {
            //{"H":"chathub","M":"Send","A":["tester","hello"],"I":0}

            var payload = new RollerBallWrapper()
            {
                H = "myhub1",
                M = "Send",
                A = new[] { name, message },
                I = 12
            };

            var wsPacket = JsonConvert.SerializeObject(payload);

            _ws.Send(wsPacket);
        }

        private void AttachAndConnect()
        {
            _ws.OnClose += _ws_OnClose;

            _ws.OnError += _ws_OnError;

            _ws.OnMessage += _ws_OnMessage;

            _ws.OnOpen += _ws_OnOpen;

            _ws.Connect();
        }

        void _ws_OnOpen(object sender, EventArgs e)
        {
            Debug.Log("Opened Connection");
        }

        //
        // This seems to be retriving the last frame containing the Identifier
        void _ws_OnMessage(object sender, MessageEventArgs e)
        {
            Debug.Log(e.Data); // Returns {"I":"0"} ????
        }

        void _ws_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            Debug.Log(e.Message);
        }

        void _ws_OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log(e.Reason + " Code: " + e.Code + " WasClean: " + e.WasClean);
        }

        public void On<T>(string method, Action<T> callback) where T : class
        {
            _actionMap.Add(method, new UnTypedActionContainer
            {
                Action = new Action<object>(x =>
                {
                    callback(x as T);
                }),
                ActionType = typeof(T)
            });
        }
    }

   

    internal class UnTypedActionContainer
    {
        public Action<object> Action { get; set; }
        public Type ActionType { get; set; }
    }

    class MessageWrapper
    {
        public string C { get; set; }

        public RollerBallWrapper[] M { get; set; }
    }

    class RollerBallWrapper
    {
        public string H { get; set; }

        public string M { get; set; }

        public string[] A { get; set; }

        public int I { get; set; }
    }

    /*
    * {
   "Url": "/signalr",
   "ConnectionToken": "TI4s3p0TrF0WeZeuMwCwZ+V8lgqiFf96/nVQbRzswwlkVOAW1/MGfgQYGuvwtBjOp4oiYpE0z8jrAFPYrjUahjmRzTtCmIOFD+HvFkGz5WyR57rqufJwbHsPYtVQRe0h",
   "ConnectionId": "7fb23953-6e3f-48e3-8542-cd4b611590bc",
   "KeepAliveTimeout": 20.0,
   "DisconnectTimeout": 30.0,
   "ConnectionTimeout": 110.0,
   "TryWebSockets": true,
   "ProtocolVersion": "1.3",
   "TransportConnectTimeout": 5.0,
   "LongPollDelay": 0.0
}
    */
    internal class NegotiateResponse
    {
        public string Url { get; set; }
        public string ConnectionToken { get; set; }

        public string ConnectionId { get; set; }

        public float KeepAliveTimeout { get; set; }
        public float DisconnectTimeout { get; set; }

    }

   
}