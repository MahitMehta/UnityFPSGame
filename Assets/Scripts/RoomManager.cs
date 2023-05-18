using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomManager : MonoBehaviour
{
    public TMPro.TMP_Dropdown clients;
    public TMPro.TMP_Text roomNameLabel;
    public Button startGame;
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
        player.AddComponent<Rigidbody>();
        if (SceneManager.GetActiveScene().name.Equals("RoomScene"))
        {
            startGame.onClick.AddListener(delegate
            {
                SceneManager.LoadScene("GameScene");
            });
        }
    }

    public void OnEnterRoom()
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
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 change = new Vector3(0, 0, 1) * Time.deltaTime;
            player.transform.position += change; 
        }

        if (Input.GetKey(KeyCode.A))
        {
            Vector3 change = new Vector3(-1, 0, 0) * Time.deltaTime;
            player.transform.position += change;
        }


        if (Input.GetKey(KeyCode.D))
        {
            Vector3 change = new Vector3(1, 0, 0) * Time.deltaTime;
            player.transform.position += change;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Vector3 change = new Vector3(1, 0, -1) * Time.deltaTime;
            player.transform.position += change;
        }

        if (elapsedTime > 0.033f)
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
                },
            };

            List<NetworkManager.BatchTransform> bts = new() { bt };
            GameManager.gm.BroadcastBatchTransform(bts);
            elapsedTime = 0;
        }

        elapsedTime += Time.deltaTime;
    }

    private float lastBatch = DateTime.Now.Millisecond;
    private Coroutine interpolate;
    private bool interpolatin = false;
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

           if (go.GetComponent<Interpolator>() == null)
            {
                Interpolator t = go.AddComponent<Interpolator>();
            }
            go.GetComponent<Interpolator>().setTargetPos(new Vector3(bt.vector[0], bt.vector[1], bt.vector[2]));


            //time between the batches
            Debug.Log(DateTime.Now.Millisecond - lastBatch);





            //go.transform.position = new Vector3(bt.vector[0], bt.vector[1], bt.vector[2]);
        }
        lastBatch = DateTime.Now.Millisecond;
    }

    //not used
    public IEnumerator Interpolate(GameObject from, NetworkManager.BatchTransform to, float timeSinceLastBatch)
    {
        interpolatin = true;
        Vector3 fromPos = from.transform.position;
        Vector3 toPos = new Vector3(to.vector[0], to.vector[1], to.vector[2]);
        //from.transform.position = toPos;
        //toPos = toPos * 2 - fromPos;
        //Quaternion toRot = new Quaternion(to.rotVector[0], to.rotVector[1], to.rotVector[2], to.rotVector[3]);
        float timePassed = 0;
        while (timePassed < timeSinceLastBatch)
        {
            Debug.Log("percent " + timePassed / timeSinceLastBatch);
           
            from.transform.position = Vector3.LerpUnclamped(fromPos, toPos, timePassed/timeSinceLastBatch);
            timePassed += Time.deltaTime * 1000;

            yield return null;
        }
        interpolatin = false;
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
