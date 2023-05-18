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

    protected override void Start()
    {
        base.Start();
        userId = System.Guid.NewGuid().ToString();

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
        // Debug.Log("OnBatchTransform: " + transformations.Count);
        UnityMainThreadDispatcher.Instance().Enqueue(delegate
        {
            if (RoomManager.Exists()) RoomManager.Instance().HandleBatchTransformations(transformations);
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
