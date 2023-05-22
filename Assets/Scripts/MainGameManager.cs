using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using WSMessage;

public class MainGameManager : MonoBehaviour
{
    public GameObject player;

    public CursorLockMode cursorLockMode = CursorLockMode.Locked;

    private static MainGameManager _instance;
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

        cursorLockMode = CursorLockMode.Locked;
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
            scene = 2,
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
                scene = 2,
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
                go.AddComponent<PlayerClone>();
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
                go.GetComponent<PlayerClone>().playerState = bt.state;
            }
        }
    }

    public static bool Exists()
    {
        return _instance != null;
    }

    public static MainGameManager Instance()
    {
        if (!Exists())
        {
            throw new Exception("Main Game Manager Doesn't Exist");
        }
        return _instance;
    }
}