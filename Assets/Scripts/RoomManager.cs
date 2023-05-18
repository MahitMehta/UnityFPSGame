using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;
using WSMessage;

public class RoomManager : MonoBehaviour
{
    public TMPro.TMP_Dropdown clients;
    public TMPro.TMP_Text roomNameLabel;
    public GameObject player;
    public Button startGame;

    private static RoomManager _instance;
    private float elapsedTime = 0;

    private Vector3 vel = new Vector3(0,0,0); 

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
    }

    public void OnLeftRoom(string userId)
    {
       
    }

    public void OnEnterRoom()
    {
        BatchTransform bt = new BatchTransform()
        {
            go = player.name,
            type = "transform",
            userId = GameManager.instance.userId,
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
            }
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
        player.GetComponent<Rigidbody>().velocity = new Vector3(vel.x, player.GetComponent<Rigidbody>().velocity.y, vel.z);

        // Temporary | For Testing
        if (Input.GetKeyDown(KeyCode.C))
        {
            GameManager.instance.SendMessages(new List<Message>() {
                    GameManager.instance.ContructUserPropertyMessage("username", GameManager.instance.userId, "New Name"),
             });
        }

        
        if (elapsedTime > 0.033f)

        {
            BatchTransform btTransform = new ()
            {
                go = player.name,
                ts= GetNanoseconds(),
                type = "transform",
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
                }
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
        foreach (BatchTransform bt in transformations) {
            GameObject go; 
            if (GameObject.Find(bt.go + ":" + bt.userId) != null)
            {
                go = GameObject.Find(bt.go + ":" + bt.userId);
            } else
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

                // go.transform.rotation = Quaternion.Euler(bt.rotation[0], bt.rotation[1], bt.rotation[2]);
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
