using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using WSMessage;
using Debug = UnityEngine.Debug;

public class RoomManager : MonoBehaviour
{
    public TMPro.TMP_Dropdown clients;
    public TMPro.TMP_Text roomNameLabel;
    public GameObject player;
    public Button startGame, goToLockeroom, select;
    public Camera lockeroom, main;
    public bool inLockeroom = false;
    public CursorLockMode cursorLockMode = CursorLockMode.Locked;
    public Vector3 lockeroomPos;
    public Vector3 lockeroomRot;
    public GameObject charSelect;

    private static RoomManager _instance;

    // top right bottom left 
    public List<int> playerStateRT = new() { 0, 0, 0, 0, 0, 0, 0}; // realtime

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
        select.onClick.AddListener(delegate
        {
            charSelect.GetComponent<CharacterSelect>().select();
        });
        lockeroom.enabled = false;
        startGame.onClick.AddListener(delegate {
            GameManager.Instance().SendMessages(new List<Message>() {
                    GameManager.Instance().ContructBroadcastMethodCallMessage("StartGame")
            });
        });
        goToLockeroom.onClick.AddListener(delegate {
            /*if (!inLockeroom)
            {
                main.enabled = false;
                lockeroom.enabled = true;
            }*/
            //StartCoroutine(switchCamPos(!inLockeroom));
            main.enabled = inLockeroom;
            lockeroom.enabled = !inLockeroom;
            inLockeroom = !inLockeroom;
            //goToLockeroom.GetComponentInChildren<TMPro>()
        });

        player.AddComponent<Rigidbody>();
        player.AddComponent<Player>();
        player.GetComponent<Rigidbody>().freezeRotation = false;



        cursorLockMode = CursorLockMode.Confined;
        Cursor.lockState = cursorLockMode;

        GameManager.Instance().AddBTUpdate("player:room", PlayerBatchTranform);

    }

    public IEnumerator<Transform> switchCamPos(bool toLockerRoom)
    {
        if (toLockerRoom)
        {
            while(lockeroomPos!=lockeroom.transform.position || lockeroomRot != lockeroom.transform.eulerAngles)
            {
                lockeroom.transform.position = Vector3.Lerp(main.transform.position, lockeroomPos, (main.transform.position - lockeroom.transform.position).magnitude / (main.transform.position - lockeroomPos).magnitude + Time.deltaTime);
                lockeroom.transform.position = Vector3.Lerp(main.transform.eulerAngles, lockeroomRot, (main.transform.eulerAngles - lockeroom.transform.eulerAngles).magnitude / (main.transform.eulerAngles - lockeroomRot).magnitude + Time.deltaTime);
            }
        }
        else
        {
            while(main.transform.position !=lockeroom.transform.position || main.transform.eulerAngles != lockeroom.transform.position)
            {
                lockeroom.transform.position = Vector3.Lerp(lockeroomPos, main.transform.position, (main.transform.position - lockeroom.transform.position).magnitude * (1 / (main.transform.position - lockeroomPos).magnitude) + Time.deltaTime);
                lockeroom.transform.position = Vector3.Lerp(lockeroomRot, main.transform.eulerAngles, (main.transform.eulerAngles - lockeroom.transform.eulerAngles).magnitude * (1 / (main.transform.eulerAngles - lockeroomRot).magnitude) + Time.deltaTime);
            }
            main.enabled = true;
            lockeroom.enabled = false;
        }
        yield return null;

    }

    public void OnLeftRoom(string userId)
    {

    }

    public void OnEnterRoom()
    {
    
 
    }

    [Update(TickRate = 1, Subscribe = true)]
    private void PlayerBatchTranform() {
        BatchTransform bt = new()
        {
            go = player.name,
            pf = player.GetComponent<Player>().wizardClass,
            type = BTType.Transform,
            ticks = GameManager.Instance().ticks,
            scene = 1,
            userId = GameManager.Instance().userId,
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
        GameManager.Instance().batchTransforms.Add(bt);
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            if (cursorLockMode == CursorLockMode.Locked) cursorLockMode = CursorLockMode.None;
            else cursorLockMode = CursorLockMode.Locked;

            Cursor.lockState = cursorLockMode;
        }

    }

    public void HandleBatchTransformations(List<BatchTransform> transformations)
    {
        foreach (BatchTransform bt in transformations)
        {
            GameObject go = null;

            if (bt.type != BTType.Instantiate) go = GameObject.Find(bt.go + ":" + bt.userId);
            if (go == null)
            {
                go = Instantiate(
                    Resources.Load(bt.pf, typeof(GameObject)),
                    new Vector3(bt.position[0], bt.position[1], bt.position[2]),
                    Quaternion.Euler(new Vector3(bt.rotation[0], bt.rotation[1], bt.rotation[2]))
                ) as GameObject;
                go.name = bt.go + ":" + bt.userId;
                if (bt.pf.EndsWith("Wizard")) go.AddComponent<PlayerClone>();
            }

            if (bt.pf.EndsWith("Wizard") && bt.type == BTType.Transform)
            {
                if (go.GetComponent<Interpolator>() == null)
                {
                    go.AddComponent<Interpolator>();
                    
                    go.GetComponent<Interpolator>().lastPosition = new Vector3(bt.position[0], bt.position[1], bt.position[2]);
                    go.GetComponent<Interpolator>().targetPosition = new Vector3(bt.position[0], bt.position[1], bt.position[2]);

                    go.GetComponent<Interpolator>().lastRotation = Quaternion.Euler(new Vector3(bt.rotation[0], bt.rotation[1], bt.rotation[2]));
                    go.GetComponent<Interpolator>().targetRotation = Quaternion.Euler(new Vector3(bt.rotation[0], bt.rotation[1], bt.rotation[2]));


                    go.GetComponent<Interpolator>().previousTicksRotation = bt.ticks;
                    go.GetComponent<Interpolator>().previousTicksPosition = bt.ticks;
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
            return x.text == disconnectedClientId || x.text == GameManager.Instance().GetUser(disconnectedClientId).username;
        });
        clients.options.Remove(filteredOptions);
        clients.RefreshShownValue();
    }

    public void RefreshUsernames()
    {
        List<TMPro.TMP_Dropdown.OptionData> options = new();

        foreach (string client in GameManager.Instance().users.Keys)
        {
            var user = GameManager.Instance().GetUser(client);
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
            var user = GameManager.Instance().GetUser(client.userId);
            options.Add(new TMPro.TMP_Dropdown.OptionData(user != null ? user.username : client.userId));
        }

        clients.AddOptions(options);

        if (clients.value.Equals(-1)) clients.value = 0;
        clients.RefreshShownValue();
    }
}