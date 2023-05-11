using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using WebSocketSharp;

public class NetworkManager : MonoBehaviour
{
    private readonly static string MAHITM_UNITY_WS_SERVER_ADDRESS = "wss://unity.mahitm.com/unity";
    private readonly static string MAHITM_UNITY_HTTP_SERVER_ADDRESS = "http://unity.mahitm.com/unity";

    [System.Serializable]
    public class WSRoomBody
    {
        public string name;
    }

    [System.Serializable]
    public class WSJoinedRoomBody
    {
        public string name;
        public string userId;
    }

    [System.Serializable]
    public class WSLeftRoomBody
    {
        public string userId;
    }

    [System.Serializable]
    class WSMessage
    {
        public string type;
    }

    [System.Serializable]
    class WSMessage<T> : WSMessage 
    {
        public T body;
    }

    [System.Serializable]
    class RoomsResponse
    {
        public List<string> rooms; 
    }

    [System.Serializable]
    class ClientsResponse
    {
        public string room;
        public List<string> clients; 
    }

    [System.Serializable]
    public class BatchTransform
    {
        public string type; // position | rotation
        public string go; // gameobject name
        public string userId;
        public List<float> vector; 
    }

    [System.Serializable]
    public class WSBatchTransformationBody
    {
        public List<BatchTransform> transformations;  
    }

    protected WebSocket ws;

    void OnApplicationQuit()
    {
        Debug.Log("closing");
        ws.Close();
    }

    protected void ConnectToWebSocket(string userId)
    {
        if (ws != null) return;

        ws = new WebSocket(MAHITM_UNITY_WS_SERVER_ADDRESS + "?userId=" + userId);
        ws.Connect();

        ws.OnMessage += (sender, e) =>
        {
            WSMessage message = JsonUtility.FromJson<WSMessage>(e.Data);

            if (message.type.Equals("created_room"))
            {
                WSMessage<WSRoomBody> createdRoomMSG = JsonUtility.FromJson<WSMessage<WSRoomBody>>(e.Data);
                OnCreatedRoom(createdRoomMSG.body.name);
            } else if (message.type.Equals("joined_room"))
            {
                WSMessage<WSJoinedRoomBody> joinedRoomMSG = JsonUtility.FromJson<WSMessage<WSJoinedRoomBody>>(e.Data);
                OnJoinedRoom(joinedRoomMSG.body.name, joinedRoomMSG.body.userId);
            }
            else if (message.type.Equals("left_room"))
            {
                WSMessage<WSLeftRoomBody> leftRoomMSG = JsonUtility.FromJson<WSMessage<WSLeftRoomBody>>(e.Data);
                OnLeftRoom(leftRoomMSG.body.userId);
            }
            else if (message.type.Equals("batch_transform"))
            {
                WSMessage<WSBatchTransformationBody> batchTransformMSG = JsonUtility.FromJson<WSMessage<WSBatchTransformationBody>>(e.Data);
                OnBatchTransform(batchTransformMSG.body.transformations);
            }
        };
    }

    public void BroadcastBatchTransform(List<BatchTransform> bt)
    {
        WSBatchTransformationBody body = new() { transformations = bt };
        WSMessage<WSBatchTransformationBody> msg = new()
        {
            type = "batch_transform",
            body = body
        };

        ws.Send(JsonUtility.ToJson(msg));
    }

    protected IEnumerator GetAllRoomClients(string room)
    {
        UnityWebRequest www = UnityWebRequest.Get(MAHITM_UNITY_HTTP_SERVER_ADDRESS + "/room/clients?room=" + room);
        www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            ClientsResponse data = JsonUtility.FromJson<ClientsResponse>(www.downloadHandler.text);
            OnRetrievedAllClientsForRoom(data.room, data.clients);
        }
    }

    protected IEnumerator GetAllRooms()
    {
        UnityWebRequest www = UnityWebRequest.Get(MAHITM_UNITY_HTTP_SERVER_ADDRESS + "/room/all");
        www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            RoomsResponse data = JsonUtility.FromJson<RoomsResponse>(www.downloadHandler.text);
            OnRetrievedAllRooms(data.rooms);
        }
    }

    protected void JoinRoom(string roomName)
    {
        WSRoomBody rb = new() { name = roomName };
        WSMessage<WSRoomBody> msg = new()
        {
            type = "join_room",
            body = rb
        };

        ws.Send(JsonUtility.ToJson(msg));
    }

    protected void CreateRoom(string roomName)
    {
        WSRoomBody rb = new() { name = roomName };
        WSMessage<WSRoomBody> msg = new()
        {
            type = "create_room",
            body = rb
        };

        ws.Send(JsonUtility.ToJson(msg));
    }

    protected virtual void OnRetrievedAllClientsForRoom(string room, List<string> clients) {}

    protected virtual void OnRetrievedAllRooms(List<string> rooms) {}

    protected virtual void OnCreatedRoom(string roomName) {}

    protected virtual void OnJoinedRoom(string roomName, string userId) {}

    protected virtual void OnLeftRoom(string userId) {}

    protected virtual void OnBatchTransform(List<BatchTransform> transformations) { }
}
