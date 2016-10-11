using System;
using UnityEngine;
using System.Collections;
using Assets.Scripts;
using SignalR.Client._20.Hubs;

public class CoreConnect : MonoBehaviour {
    private SignalRClient _client;

    // Use this for initialization
	void Start ()
	{
        //_client = new SignalRClient();
        //   _client.Open();
        //   _client.SendMessage("asd","111");

        HubConnection connection = new HubConnection("http://localhost:42282/");
        IHubProxy proxy = connection.CreateProxy("MyHub1");
        // subscribe to event
        // subscribe to event
        proxy.Subscribe("Send").Data += data =>
        {
            Debug.Log(String.Format("Received: [{0}] ", data));
        };

        
        // start connection
        connection.Start();

        proxy.Invoke("Send", "a1", "a2").Finished += (sender, e) =>
        {
            Debug.Log(String.Format("Received: [{0}] ", e.Result));
        };
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
