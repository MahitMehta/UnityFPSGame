using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WSMessage; 

public class GameManager : NetworkManager
{
    public static GameManager instance;

    public Button createRoom;
    public Button joinRoom;

    public TMPro.TMP_InputField usernameField;
    public TMPro.TMP_InputField newRoomField;
    public TMPro.TMP_Dropdown roomOptions;

    public string userId;
    public string room; 

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    // 15 characters
    public string UserID_GuidBase64Shortened() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                                 .Replace("/", "_")
                                                 .Replace("+", "-")
                                                 .Substring(0, 15);
    // base 64 encoded guid without == | 22 characters 
    private string UserID_Base64GUID() => Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                                          .Substring(0, 22);
   
    protected override void Start()
    {
        base.Start();
        userId = UserID_GuidBase64Shortened();

        StartCoroutine(GetAllRooms());
        ConnectToWebSocket(userId);

        if (SceneManager.GetActiveScene().name.Equals("LobbyScene"))
        {
            createRoom.onClick.AddListener(delegate {
                SendMessages(new List<Message>() {
                    ContructUserPropertyMessage("username", userId, usernameField.text),
                    ContructCreateRoomMessage(newRoomField.text),
                });
            });

            joinRoom.onClick.AddListener(delegate {
                string selectedRoom = roomOptions.options[roomOptions.value].text;
  
                SendMessages(new List<Message>() {
                    ContructUserPropertyMessage("username", userId, usernameField.text),
                    ContructJoinRoomMessage(selectedRoom),
                });
            });
        }
    }

    public void ChangeScene() {
        Debug.Log("Change Scene");
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

        roomOptions.AddOptions(options);
        UnityMainThreadDispatcher.Instance().Enqueue(UpdateRoomOptions);

    }

    private void UpdateRoomOptions()
    {
        if (roomOptions.value.Equals(-1)) roomOptions.value = 0; 
        roomOptions.RefreshShownValue();
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
                    username = joinedUsername
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

    protected override void OnLeftRoom(string disconnectedUserId)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            if (RoomManager.Exists()) RoomManager.Instance().RemoveClient(disconnectedUserId);
        });
    }
}
