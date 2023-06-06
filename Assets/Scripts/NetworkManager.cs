using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;
using WSMessage;
using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Linq;

public class NetworkManager : MonoBehaviour
{
    public bool useProduction = false; 

    private readonly static string MAHITM_UNITY_WSS_SERVER_ADDRESS = "wss://walrus-app-yazre.ondigitalocean.app/unity";
    private readonly static string MAHITM_UNITY_HTTPS_SERVER_ADDRESS = "https://walrus-app-yazre.ondigitalocean.app/unity";
    private readonly static string MAHITM_UNITY_WS_SERVER_ADDRESS = "ws://localhost:3000/unity";
    private readonly static string MAHITM_UNITY_HTTP_SERVER_ADDRESS = "http://localhost:3000/unity";

    private string WEBSOCKET_ADDRESS;
    private string HTTP_ADDRESS;

    public int ticks = 0;
    private readonly static int MAX_TICK_DIVERGENCE = 1; 
 
    protected WebSocket ws;
    public Dictionary<string, User> users = new ();

    public Dictionary<string, Update> updates = new ();
    public List<BatchTransform> batchTransforms; 

    protected virtual void Start()
    {
        WEBSOCKET_ADDRESS = useProduction ? MAHITM_UNITY_WSS_SERVER_ADDRESS : MAHITM_UNITY_WS_SERVER_ADDRESS;
        HTTP_ADDRESS = useProduction ? MAHITM_UNITY_HTTPS_SERVER_ADDRESS : MAHITM_UNITY_HTTP_SERVER_ADDRESS;
    }

    void OnApplicationQuit()
    {
        ws.Close();
    }

    public void RemoveBTUpdate(string key)
    {
        updates.Remove(key);
    }

    public void AddBTUpdate(string key, Action cb)
    {
        MethodInfo methodInfo = cb.Method;
        UpdateAttribute update = Attribute.GetCustomAttribute(methodInfo, typeof(UpdateAttribute)) as UpdateAttribute;

        updates.Add(key, new Update
        {
             Callback = cb,
             TickRate = update.TickRate,
             Subscribe = update.Subscribe
        });
    }

    public void FixedUpdate()
    {
        ticks++;

        var updateClone = new Dictionary<string, Update>(updates);

        foreach (var key in updateClone.Keys)
        {
            if (ticks % updates[key].TickRate == 0) updates[key].Callback();
            if (!updates[key].Subscribe && updates.ContainsKey(key)) updates.Remove(key);
        }

        if (batchTransforms.Count == 0) return; 

        SendMessages(
          new List<Message>() { ContructBatchTransformMessage(batchTransforms) });

        batchTransforms.Clear();
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
            MessagesContainer<GeneralBody> mc = JsonConvert.DeserializeObject<MessagesContainer<GeneralBody>>(data);

            foreach (var message in mc.messages)
            {
                if (message.type.Equals(WSMessage.Type.CREATED_ROOM))
                {
                    OnCreatedRoom(message.body.name);
                } else if (message.type.Equals(WSMessage.Type.SYNC_TICK))
                {
                    if (Mathf.Abs(ticks - message.body.ticks) > MAX_TICK_DIVERGENCE) ticks = message.body.ticks;
                } else if (message.type.Equals(WSMessage.Type.JOINED_ROOM))
                {
                    if (!users.ContainsKey(message.body.userId)) users.Add(message.body.userId, new User());
                    users[message.body.userId].username = message.body.username;
                    users[message.body.userId].isConnected = true; 
                    OnJoinedRoom(message.body.name, message.body.userId, message.body.username);
                } else if (message.type.Equals(WSMessage.Type.LEFT_ROOM))
                {
                    users[message.body.userId].isConnected = false;
                    OnLeftRoom(message.body.userId);

                } else if (message.type.Equals(WSMessage.Type.BATCH_TRANSFORM))
                {
                    OnBatchTransform(message.body.transformations);

                } else if (message.type.Equals(WSMessage.Type.SET_USER_PROPERTY))
                {
                    var userId = message.body.userId;

                    if (!users.ContainsKey(userId))
                    {
                        users.Add(userId, new User()
                        {
                            isConnected = true,
                            userId = userId,
                        });
                    }
                    if (message.body.property == "username") users[userId].username = message.body.value;

                    OnSetUserProperty(userId, message.body.property, message.body.value);
                } else if (message.type.Equals(WSMessage.Type.BROADCAST_METHOD_CALL))
                {
                    string methodName = message.body.method;
                    object[] parameters = new object[] { };

                    try
                    {
                        parameters = new object[] { message.body.parameters[0], message.body.parameters[1] };
                    }
                    catch (Exception e) { }
                    typeof(GameManager).GetMethod(methodName).Invoke(GameManager.Instance(), parameters);
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

    public IEnumerator GetAllRooms()
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

    public IEnumerator GetAllRoomClients(string room)
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

    public Message<BroadcastMethodCallBody> ContructBroadcastMethodCallMessage(string method)
    {
        BroadcastMethodCallBody body = new() { method = method, parameters = null};
        Message<BroadcastMethodCallBody> msg = new()
        {
            type = WSMessage.Type.BROADCAST_METHOD_CALL,
            body = body
        };

        return msg;
    }
    public Message<BroadcastMethodCallBody> ContructBroadcastMethodCallMessage(string method, object[] parameters)
    {
        BroadcastMethodCallBody body = new() { method = method, parameters = parameters};
        Message<BroadcastMethodCallBody> msg = new()
        {
            type = WSMessage.Type.BROADCAST_METHOD_CALL,
            body = body
        };

        return msg;
    }

    public Message<BatchTransformationBody> ContructBatchTransformMessage(List<BatchTransform> bt)
    {
        BatchTransformationBody body = new() { transformations = bt };
        Message<BatchTransformationBody> msg = new()
        {
            type = WSMessage.Type.BATCH_TRANSFORM,
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
            type = WSMessage.Type.SET_USER_PROPERTY,
            body = body
        };
        return msg; 
    }

    public Message<RoomBody> ContructJoinRoomMessage(string roomName)
    {
        RoomBody rb = new() { name = roomName };
        Message<RoomBody> msg = new()
        {
            type = WSMessage.Type.JOIN_ROOM,
            body = rb
        };

        return msg; 
    }

    public Message<RoomBody> ContructCreateRoomMessage(string roomName)
    {
        RoomBody rb = new() { name = roomName };
        Message<RoomBody> msg = new()
        {
            type = WSMessage.Type.CREATE_ROOM,
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
