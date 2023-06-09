using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WSMessage;

public class GameManager : NetworkManager
{
    private static GameManager _instance;

    public string userId;
    public string room; 

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }
   
    protected override void Start()
    {
        base.Start();
        userId = Utilities.UserID_GuidBase64Shortened();

        StartCoroutine(GetAllRooms());
        ConnectToWebSocket(userId);
    }

    public void StartGame() {
        RemoveBTUpdate("player:room");
        SceneManager.LoadScene("GameScene");
    }

    protected override void OnCreatedRoom(string name)
    {
        List<string> newRooms = new List<string>();
        newRooms.Add(name);
        AddRoomsToDropdown(newRooms);
;    }

    protected override void OnRetrievedAllRooms(List<string> rooms)
    {
        AddRoomsToDropdown(rooms);
    }

    protected override void OnSetUserProperty(string userId, string property, string value)
    {
        if (!RoomManager.Exists()) return;

        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            if (property == "username")
            {
                RoomManager.Instance().RefreshUsernames();
            }
        });
    }

    private void AddRoomsToDropdown(List<string> rooms) {
        List<TMPro.TMP_Dropdown.OptionData> options = new List<TMPro.TMP_Dropdown.OptionData>();

        foreach (string room in rooms) options.Add(new TMPro.TMP_Dropdown.OptionData(room));

        LobbyManager.Instance().roomOptions.AddOptions(options);
        UnityMainThreadDispatcher.Instance().Enqueue(UpdateRoomOptions);

    }

    private void UpdateRoomOptions()
    {
        if (LobbyManager.Instance().roomOptions.value.Equals(-1)) LobbyManager.Instance().roomOptions.value = 0;
        LobbyManager.Instance().roomOptions.RefreshShownValue();
    }

    private IEnumerator loadRoomScene()
    {
        yield return SceneManager.LoadSceneAsync("RoomScene");
        StartCoroutine(GetAllRoomClients(room));
        if (RoomManager.Exists())
        {
            RoomManager.Instance().OnEnterRoom();
            RoomManager.Instance().SetRoomName(room);
        }
    }

    protected override void OnRetrievedAllClientsForRoom(string room, List<User> clients)
    {
        if (RoomManager.Exists()) RoomManager.Instance().AddClients(clients);
    }

    protected override void OnJoinedRoom(string roomName, string joinedUserId, string joinedUsername)
    {
        if (joinedUserId.Equals(userId))
        {
            room = roomName;
            UnityMainThreadDispatcher.Instance().Enqueue(loadRoomScene());
        } else 
        {
            // Consider Refetching all Room Clients and not just adding the new user's id
            UnityMainThreadDispatcher.Instance().Enqueue(delegate
            {
                RoomManager.Instance().OnEnterRoom();
                List<User> newClients = new List<User>();
                newClients.Add(new User()
                {
                    userId = joinedUserId,
                    username = joinedUsername,
                });
                if (RoomManager.Exists()) RoomManager.Instance().AddClients(newClients);
            });
        }
    }

    protected override void OnBatchTransform(List<BatchTransform> transformations)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            List<BatchTransform> roomTransformations = new();
            List<BatchTransform> gameTransformations = new();

            foreach (var t in transformations)
            {
                if (t.scene == 1) roomTransformations.Add(t);
                else if (t.scene == 2) gameTransformations.Add(t);
            }

            if (MainGameManager.Exists()) MainGameManager.Instance().HandleBatchTransformations(gameTransformations);
            if (RoomManager.Exists()) RoomManager.Instance().HandleBatchTransformations(roomTransformations);
        });
    }

    protected override void OnLeaveGame(string disconnectedUserId)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            if (MainGameManager.Exists())
            {
                MainGameManager.Instance().OnLeftRoom(disconnectedUserId);
                if (userId == disconnectedUserId) StartCoroutine(loadRoomScene());
            }
        });
    }

    protected override void OnLeftRoom(string disconnectedUserId)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            if (RoomManager.Exists()) RoomManager.Instance().RemoveClient(disconnectedUserId);
            if (MainGameManager.Exists()) MainGameManager.Instance().OnLeftRoom(disconnectedUserId);
        });
    }

    public static bool Exists()
    {
        return _instance != null;
    }

    public static GameManager Instance()
    {
        if (!Exists())
        {
            throw new Exception("Game Manager Doesn't Exist");
        }
        return _instance;
    }

    public User getUser()
    {
        return users[userId];
    }

    public void updateSkin(string userId, string skin)
    {
        if(userId!=null && skin!=null && GameObject.Find("Player:" + userId)!=null) GameObject.Find("Player:" + userId).GetComponent<PlayerClone>().updateSkin(skin);
    }
}
