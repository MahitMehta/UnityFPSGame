using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;
using WSMessage;
using Newtonsoft.Json;

public class NetworkManager : MonoBehaviour
{
    public bool useProduction = false; 

    private readonly static string MAHITM_UNITY_WSS_SERVER_ADDRESS = "wss://unity.mahitm.com/unity";
    private readonly static string MAHITM_UNITY_HTTPS_SERVER_ADDRESS = "https://unity.mahitm.com/unity";
    private readonly static string MAHITM_UNITY_WS_SERVER_ADDRESS = "ws://localhost:3000/unity";
    private readonly static string MAHITM_UNITY_HTTP_SERVER_ADDRESS = "http://localhost:3000/unity";

    private string WEBSOCKET_ADDRESS;
    private string HTTP_ADDRESS;

 
    protected WebSocket ws;
    public Dictionary<string, User> users = new ();

    protected virtual void Start()
    {
        WEBSOCKET_ADDRESS = useProduction ? MAHITM_UNITY_WSS_SERVER_ADDRESS : MAHITM_UNITY_WS_SERVER_ADDRESS;
        HTTP_ADDRESS = useProduction ? MAHITM_UNITY_HTTPS_SERVER_ADDRESS : MAHITM_UNITY_HTTP_SERVER_ADDRESS;
    }

    void OnApplicationQuit()
    {
        ws.Close();
    }

    public User GetUser(string userId)
    {
        return users.ContainsKey(userId) ? users[userId] : null; 
    }

    async protected void ConnectToWebSocket(string userId)
    {
        if (ws != null) return;

        ws = new WebSocket(WEBSOCKET_ADDRESS + "?userId=" + userId);
      
        ws.OnMessage += (bytes) =>
        {
            var data = System.Text.Encoding.UTF8.GetString(bytes);
   
            MessagesContainer<IGeneralBody> mc = JsonConvert.DeserializeObject<MessagesContainer<IGeneralBody>>(data);

            foreach (var message in mc.messages)
            {
                if (message.type.Equals("created_room"))
                {
                    OnCreatedRoom(message.body.name);
                }
                else if (message.type.Equals("joined_room"))
                {
                    if (!users.ContainsKey(message.body.userId)) users.Add(message.body.userId, new User());
                    users[message.body.userId].username = message.body.username;
                    users[message.body.userId].isConnected = true; 
                    OnJoinedRoom(message.body.name, message.body.userId, message.body.username);
                }
                else if (message.type.Equals("left_room"))
                {
                    users[message.body.userId].isConnected = false;
                    OnLeftRoom(message.body.userId);

                }
                else if (message.type.Equals("batch_transform"))
                {
                    OnBatchTransform(message.body.transformations);

                }
                else if (message.type.Equals("set_user_property"))
                {
                    var userId = message.body.userId;

                    if (!users.ContainsKey(userId))
                    {
                        Debug.Log("Creating new user: " + userId);
                        users.Add(userId, new User()
                        {
                            isConnected = true,
                            userId = userId,
                        });
                    }

                    Debug.Log("username" + message.body.value);
         
                    if (message.body.property == "username") users[userId].username = message.body.value;

                    OnSetUserProperty(userId, message.body.property, message.body.value);
                }
            }
        };

        await ws.Connect();
        Debug.Log(ws.State);
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            ws.DispatchMessageQueue();
        #endif
    }


    protected IEnumerator GetAllRooms()
    {
        UnityWebRequest www = UnityWebRequest.Get(HTTP_ADDRESS + "/room/all");
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

    protected IEnumerator GetAllRoomClients(string room)
    {
        UnityWebRequest www = UnityWebRequest.Get(HTTP_ADDRESS + "/room/clients?room=" + room);
        www.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            RoomClientsResponse data = JsonUtility.FromJson<RoomClientsResponse>(www.downloadHandler.text);

            // TODO: Most likely only need to remove users that are not the main user
            // users.Clear();  
            foreach (var user in data.clients)
            {
                if (!users.ContainsKey(user.userId)) users.Add(user.userId, new User() {
                    userId = user.userId,
                    username = user.username,
                    isConnected = true
                });
            }

            OnRetrievedAllClientsForRoom(data.room, data.clients);
        }
    }

    public void SendMessages(List<Message> messages)
    {
        MessagesContainer mc = new() {
            messages = messages
        };

        ws.SendText(JsonConvert.SerializeObject(mc));
    }

    public Message<BatchTransformationBody> ContructBatchTransformMessage(List<BatchTransform> bt)
    {
        BatchTransformationBody body = new() { transformations = bt };
        Message<BatchTransformationBody> msg = new()
        {
            type = "batch_transform",
            body = body
        };

        return msg; 
    }

    public Message<UserPropertyBody> ContructUserPropertyMessage(string property, string userId, string value)
    {
        UserPropertyBody body = new()
        {
            property = property,
            userId = userId,
            value = value
        };
        Message<UserPropertyBody> msg = new()
        {
            type = "set_user_property",
            body = body
        };
        return msg; 
    }

    protected Message<RoomBody> ContructJoinRoomMessage(string roomName)
    {
        RoomBody rb = new() { name = roomName };
        Message<RoomBody> msg = new()
        {
            type = "join_room",
            body = rb
        };

        return msg; 
    }

    protected Message<RoomBody> ContructCreateRoomMessage(string roomName)
    {
        RoomBody rb = new() { name = roomName };
        Message<RoomBody> msg = new()
        {
            type = "create_room",
            body = rb
        };

        return msg; 
    }

    protected virtual void OnRetrievedAllClientsForRoom(string room, List<User> clients) {}

    protected virtual void OnRetrievedAllRooms(List<string> rooms) {}

    protected virtual void OnCreatedRoom(string roomName) {}

    protected virtual void OnJoinedRoom(string roomName, string userId, string joinedUsername) {}

    protected virtual void OnLeftRoom(string userId) {}

    protected virtual void OnSetUserProperty(string userId, string property, string value) { }

    protected virtual void OnBatchTransform(List<BatchTransform> transformations) { }
}
