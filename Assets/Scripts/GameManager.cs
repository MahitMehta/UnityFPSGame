using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : NetworkManager
{
    public static GameManager gm;

    public Button createRoom;
    public Button joinRoom;

    public TMPro.TMP_InputField newRoomField;
    public TMPro.TMP_Dropdown roomOptions;

    public string userId;
    public string room; 

    void Awake()
    {
        DontDestroyOnLoad(this);

        if (gm == null) gm = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        userId = System.Guid.NewGuid().ToString();

        StartCoroutine(GetAllRooms());
        ConnectToWebSocket(userId);

        if (SceneManager.GetActiveScene().name.Equals("LobbyScene"))
        {
            createRoom.onClick.AddListener(delegate {
                CreateRoom(newRoomField.text);
            });

            joinRoom.onClick.AddListener(delegate {
                string selectedRoom = roomOptions.options[roomOptions.value].text;
                JoinRoom(selectedRoom);
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
        if (RoomManager.Exists()) RoomManager.Instance().SetRoomName(room);
    }

    protected override void OnRetrievedAllClientsForRoom(string room, List<string> clients)
    {
        if (RoomManager.Exists()) RoomManager.Instance().AddClients(clients);
    }

    protected override void OnJoinedRoom(string roomName, string joinedUserId)
    {
        Debug.Log(joinedUserId + " joined " + roomName);

        if (joinedUserId.Equals(userId))
        {
            room = roomName;
            UnityMainThreadDispatcher.Instance().Enqueue(loadRoomScene());
        } else 
        {
            // Consider Refetching all Room Clients and not just adding the new user's id
            UnityMainThreadDispatcher.Instance().Enqueue(delegate
            {
                List<string> newClients = new List<string>();
                newClients.Add(joinedUserId);
                if (RoomManager.Exists()) RoomManager.Instance().AddClients(newClients);
            });
        }
    }

    protected override void OnBatchTransform(List<BatchTransform> transformations)
    {
        Debug.Log("OnBatchTransform: " + transformations.Count);
        Debug.Log("RoomManager: " + RoomManager.Exists());
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
