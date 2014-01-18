using ExitGames.Client.Photon;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, IPhotonPeerListener
{
    private PhotonPeer _photonPeer;
    private List<string> _messages;
    private string _message;

    public void Start()
    {
        _messages = new List<string>();
        _photonPeer = new PhotonPeer(this, ConnectionProtocol.Udp);
        if(!_photonPeer.Connect("127.0.0.1:5055", "Runeslinger"))
        {
            Debug.LogError("Could not connect to photon");
        }
        _message = "";
    }



    public void Update()
    {
        _photonPeer.Service();
    }

    public void OnGUI()
    {
        _message = GUI.TextField(new Rect(0, 0, 200, 40), _message);
        if(GUI.Button(new Rect(0,45,100,40), "Send Message"))
        {
            SendServerMessage(_message);
            _message = "";
        }
        GUI.Label(new Rect(0, 90, 300, 400), string.Join("\n", _messages.ToArray()));
    }

    private void SendServerMessage(string _message)
    {
        _photonPeer.OpCustom(
            1,
            new Dictionary<byte, object>()
            {
                {0, _message}
            },
            true);
    }

    public void OnApplicationQuit()
    {
        _photonPeer.Disconnect();
    }

    public void DebugReturn(DebugLevel level, string message)
    {
    }

    public void OnEvent(EventData eventData)
    {
        _messages.Add(eventData.Parameters[0].ToString());
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
    }

    public void OnStatusChanged(StatusCode statusCode)
    {
    }

    
}