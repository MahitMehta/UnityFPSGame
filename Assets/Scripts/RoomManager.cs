using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WSMessage;

public class RoomManager : MonoBehaviour
{
    public TMPro.TMP_Dropdown clients;
    public TMPro.TMP_Text roomNameLabel;
    public GameObject player;
    public Button startGame;

    public CursorLockMode cursorLockMode = CursorLockMode.Locked; 

    private static RoomManager _instance;
    private float elapsedTime = 0;

    // top right bottom left 
    public List<int> playerStateRT = new() { 0, 0, 0, 0 }; // realtime

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    void OnDestroy()
    {
        _instance = null;
    }

    private void Start()
    {
        player.AddComponent<Rigidbody>();
        player.AddComponent<Player>();
        player.GetComponent<Rigidbody>().freezeRotation = true;

        startGame.onClick.AddListener(delegate {
            GameManager.instance.SendMessages(new List<Message>() {
                    GameManager.instance.ContructBroadcastMethodCallMessage("ChangeScene")
            });
        });

        cursorLockMode = CursorLockMode.Confined;
        Cursor.lockState = cursorLockMode;

    }

    public void OnLeftRoom(string userId)
    {

    }

    public void OnEnterRoom()
    {
        BatchTransform bt = new BatchTransform()
        {
            go = "GreenWizard",
            type = "transform",
            userId = GameManager.instance.userId,
            scene = 1,
            ts = GetNanoseconds(),
            rotation = new List<float>() {
                    player.transform.eulerAngles.x,
                    player.transform.eulerAngles.y,
                    player.transform.eulerAngles.z
            },
            position = new List<float>() {
                player.transform.position.x,
                player.transform.position.y,
                player.transform.position.z
            },
            state = playerStateRT
        };

        List<BatchTransform> bts = new() { bt };
        GameManager.instance.SendMessages(
                new List<Message>() { GameManager.instance.ContructBatchTransformMessage(bts) });
    }

    public long GetNanoseconds()
    {
        double timestamp = Stopwatch.GetTimestamp();
        double nanoseconds = 1_000_000_000.0 * timestamp / Stopwatch.Frequency;

        return (long)nanoseconds;
    }

    public void Update()
    {
        // Temporary | For Testing | Still Some Unresolved Issues when updating user properties
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameManager.instance.SendMessages(new List<Message>() {
                    GameManager.instance.ContructUserPropertyMessage("username", GameManager.instance.userId, "New Name"),
             });
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (cursorLockMode == CursorLockMode.Locked) cursorLockMode = CursorLockMode.None;
            else cursorLockMode = CursorLockMode.Locked;

            Cursor.lockState = cursorLockMode;
        }


        if (elapsedTime >= 0.025f)

        {
            BatchTransform btTransform = new()
            {
                go = "GreenWizard",
                ts = GetNanoseconds(),
                type = "transform",
                scene = 1,
                userId = GameManager.instance.userId,
                position = new List<float>() {
                    player.transform.position.x,
                    player.transform.position.y,
                    player.transform.position.z
                },
                rotation = new List<float>() {
                    player.transform.eulerAngles.x,
                    player.transform.eulerAngles.y,
                    player.transform.eulerAngles.z
                },
                state = playerStateRT
            };



            List<BatchTransform> bts = new() { btTransform };
            GameManager.instance.SendMessages(
                new List<Message>() { GameManager.instance.ContructBatchTransformMessage(bts) });
            elapsedTime = 0;
        }

        elapsedTime += Time.deltaTime;
    }

    public void HandleBatchTransformations(List<BatchTransform> transformations)
    {
        foreach (BatchTransform bt in transformations)
        {
            GameObject go;
            if (GameObject.Find(bt.go + ":" + bt.userId) != null)
            {
                go = GameObject.Find(bt.go + ":" + bt.userId);
            }
            else
            {
                go = Instantiate(Resources.Load(bt.go, typeof(GameObject))) as GameObject;
                go.name = bt.go + ":" + bt.userId;
            }

            if (bt.type == "transform")
            {
                if (go.GetComponent<Interpolator>() == null)
                {
                    go.AddComponent<Interpolator>();
                    go.GetComponent<Interpolator>().lastPosition = new Vector3(bt.position[0], bt.position[1], bt.position[2]);
                    go.GetComponent<Interpolator>().targetPosition = new Vector3(bt.position[0], bt.position[1], bt.position[2]);

                    go.GetComponent<Interpolator>().lastRotation = Quaternion.Euler(new Vector3(bt.rotation[0], bt.rotation[1], bt.rotation[2]));
                    go.GetComponent<Interpolator>().targetRotation = Quaternion.Euler(new Vector3(bt.rotation[0], bt.rotation[1], bt.rotation[2]));

                    go.GetComponent<Interpolator>().previousTSPosition = bt.ts;
                    go.GetComponent<Interpolator>().previousTSRotation = bt.ts;
                }

                go.GetComponent<Interpolator>().AddPosition(bt);
                go.GetComponent<Interpolator>().AddRotation(bt);
            }
        }
    }

    public static bool Exists()
    {
        return _instance != null;
    }

    public static RoomManager Instance()
    {
        if (!Exists())
        {
            throw new Exception("Room Manager Doesn't Exist");
        }
        return _instance;
    }


    public void SetRoomName(string roomName)
    {
        roomNameLabel.text = "Room: " + roomName;
    }

    public void RemoveClient(string disconnectedClientId)
    {
        TMPro.TMP_Dropdown.OptionData filteredOptions = clients.options.Find((x) =>
        {
            return x.text == disconnectedClientId || x.text == GameManager.instance.GetUser(disconnectedClientId).username;
        });
        clients.options.Remove(filteredOptions);
        clients.RefreshShownValue();
    }

    public void RefreshUsernames()
    {
        List<TMPro.TMP_Dropdown.OptionData> options = new();

        foreach (string client in GameManager.instance.users.Keys)
        {
            var user = GameManager.instance.GetUser(client);
            if (!user.isConnected) continue;
            options.Add(new TMPro.TMP_Dropdown.OptionData(user != null ? user.username : client));
        }

        clients.options = options;

        if (clients.value.Equals(-1)) clients.value = 0;
        clients.RefreshShownValue();
    }

    public void AddClients(List<User> newClients)
    {
        List<TMPro.TMP_Dropdown.OptionData> options = new();

        foreach (var client in newClients)
        {
            var user = GameManager.instance.GetUser(client.userId);
            options.Add(new TMPro.TMP_Dropdown.OptionData(user != null ? user.username : client.userId));
        }

        clients.AddOptions(options);

        if (clients.value.Equals(-1)) clients.value = 0;
        clients.RefreshShownValue();
    }
}