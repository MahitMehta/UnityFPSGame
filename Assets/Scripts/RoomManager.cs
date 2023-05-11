using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public TMPro.TMP_Dropdown clients;
    public TMPro.TMP_Text roomNameLabel;
    public GameObject player; 

    private static RoomManager _instance;
    private float elapsedTime = 0; 

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
              
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 change = new Vector3(1, 0, 0) * Time.deltaTime;
            player.transform.position += change; 

            if (elapsedTime > 0.050f)
            {
                NetworkManager.BatchTransform bt = new NetworkManager.BatchTransform()
                {
                    go = player.name,
                    type = "position",
                    userId = GameManager.gm.userId,
                    vector = new List<float>() {
                    player.transform.position.x,
                    player.transform.position.y,
                    player.transform.position.z
                }
                };

                List<NetworkManager.BatchTransform> bts = new() { bt };
                GameManager.gm.BroadcastBatchTransform(bts);
                elapsedTime = 0; 
            }
        }

        elapsedTime += Time.deltaTime;
    }

    public void HandleBatchTransformations(List<NetworkManager.BatchTransform> transformations)
    {
        Debug.Log("Count: " + transformations.Count);
        foreach (NetworkManager.BatchTransform bt in transformations) {
            GameObject go; 
            if (GameObject.Find(bt.go + ":" + bt.userId) != null)
            {
                go = GameObject.Find(bt.go + ":" + bt.userId);
            } else
            {
                Debug.Log("Instantiate: " + bt.go);
                go = Instantiate(Resources.Load(bt.go, typeof(GameObject))) as GameObject;
                go.name = bt.go + ":" + bt.userId;

            }
            go.transform.position = new Vector3(bt.vector[0], bt.vector[1], bt.vector[2]);
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
        TMPro.TMP_Dropdown.OptionData filteredOptions = clients.options.Find((x) => x.text == disconnectedClientId);
        clients.options.Remove(filteredOptions);
        clients.RefreshShownValue();
    }

    public void AddClients(List<string> newClients)
    {
        List<TMPro.TMP_Dropdown.OptionData> options = new List<TMPro.TMP_Dropdown.OptionData>();

        foreach (string client in newClients) options.Add(new TMPro.TMP_Dropdown.OptionData(client));

        clients.AddOptions(options);

        if (clients.value.Equals(-1)) clients.value = 0;
        clients.RefreshShownValue();
    }
}
