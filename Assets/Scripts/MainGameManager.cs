using Cinemachine;
using Microlight.MicroBar;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WSMessage;
using static Unity.VisualScripting.Member;

public class MainGameManager : MonoBehaviour
{
    public static bool dropped;
    public GameObject player;
    public float supplyDropCoolDown;
    public float supplyDropTime;

    public CursorLockMode cursorLockMode = CursorLockMode.Locked;

    private static MainGameManager _instance;
    public GameObject ammo;
    public GameObject cinemachine;

    public TMPro.TMP_Text hp, shield, mana;
    public GameObject shieldBar, hPBar, manaBar;
    public MicroBar shieldBarScript, hPBarScript, manaBarScript;
    
    public int health = 100;
    public int shielding = 0;

    public List<GameObject> powerups;

    // top right bottom left sprint attk1 attk2
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
        

        shieldBarScript = shieldBar.GetComponent<MicroBar>();
        manaBarScript = manaBar.GetComponent<MicroBar>();
        hPBarScript = hPBar.GetComponent<MicroBar>();

        shieldBarScript.SetMaxHealth(100);
        shieldBarScript.UpdateHealthBar(0, true);
        manaBarScript.SetMaxHealth(100);
        hPBarScript.SetMaxHealth(100);

        takeDamage();
        string wizardClass = GameManager.Instance().getUser().wizardClass;

        player = Instantiate(Resources.Load(wizardClass), new Vector3(UnityEngine.Random.Range(-8, -4), 1, -5), Quaternion.identity) as GameObject;
        cinemachine.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
        player.AddComponent<Rigidbody>();

        //should be changed based on selections;
        player.AddComponent<Player>().wizardClass = wizardClass;
        player.name = "Player";

        player.GetComponent<Rigidbody>().freezeRotation = true;

        cursorLockMode = CursorLockMode.Locked;
        Cursor.lockState = cursorLockMode;

        GameManager.Instance().AddBTUpdate("player:game", PlayerBatchTranform);

        Debug.Log(playerStateRT);
    }

    public void OnLeftRoom(string userId)
    {
        Debug.Log("Attempting to delete: " + userId);
        GameObject go = GameObject.Find("Player:" + userId);
        Debug.Log("go: " + go.name);
        Destroy(go);
    }

    [Update(TickRate = 1, Subscribe = true)]
    private void PlayerBatchTranform()
    {
        BatchTransform bt = new()
        {
            go = player.name,
            pf = GameManager.Instance().users[GameManager.Instance().userId].wizardClass,
            ticks = GameManager.Instance().ticks,
            type = BTType.Transform,
            scene = 2,
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
        if (supplyDropTime <= 0 && !dropped)
        {
            makeSupplyDrop();
            supplyDropTime = supplyDropCoolDown;
        }
        supplyDropTime -= Time.deltaTime; 

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (cursorLockMode == CursorLockMode.Locked) cursorLockMode = CursorLockMode.None;
            else cursorLockMode = CursorLockMode.Locked;

            Cursor.lockState = cursorLockMode;
        }
        //hit taken/healed
        if (GameManager.Instance().getUser().shield != shielding || GameManager.Instance().getUser().hp != health) takeDamage();

    }

    public void HandleBatchTransformations(List<BatchTransform> transformations)
    {
        foreach (BatchTransform bt in transformations)
        {
            GameObject go = null;
            
            if (bt.type != BTType.Instantiate) go = GameObject.Find(bt.go + ":" + bt.userId);
            if (go == null)
            {
                if (bt.pf.EndsWith("Wizard") && GameManager.Instance().users[bt.userId].hp <= 0) return;  
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
                    go.GetComponent<Interpolator>().previousTicksPosition= bt.ticks;
                }

                go.GetComponent<Interpolator>().AddPosition(bt);
                go.GetComponent<Interpolator>().AddRotation(bt);
                go.GetComponent<PlayerClone>().playerState = bt.state;
            } else if (bt.pf.Contains("ball") && bt.type == BTType.Instantiate)
            {
                go.AddComponent<BallMove>().isClone = true; 
                go.GetComponent<BallMove>().source = GameObject.Find(player.name + ":" + bt.userId);
            }
        }

        //triggers
        playerStateRT[5] = 0;
        playerStateRT[6] = 0;

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

    public void takeDamage()
    {
        //getting hit
        health = GameManager.Instance().getUser().hp;
        shielding = GameManager.Instance().getUser().shield;
        hp.text = health.ToString();
        shield.text = shielding.ToString();
        shieldBarScript.UpdateHealthBar(shielding);
        hPBarScript.UpdateHealthBar(health);
    }

    public void makeSupplyDrop()
    {
        var r = new System.Random();
        for(int i = 0; i < 100; i++)
        {
            Debug.Log(r.Next(0, powerups.Count));
        }
        GameObject drop = powerups[r.Next(0, powerups.Count)];
        var s = Instantiate(drop, new Vector3((float)(r.NextDouble() * 25) - 17.5f, 20, (float)r.NextDouble() * 10 - 10), Quaternion.identity);
        BatchTransform bt = new()
        {
            go = s.name,
            pf = drop.name,
            type = BTType.Instantiate,
            scene = 2,
            userId = GameManager.Instance().userId,
            position = new List<float>() {
                    s.transform.position.x,
                    s.transform.position.y,
                    s.transform.position.z
            },
            rotation = new List<float>() {
                    s.transform.eulerAngles.x,
                    s.transform.eulerAngles.y,
                    s.transform.eulerAngles.z
                },
        };
        GameManager.Instance().batchTransforms.Add(bt);
    }
}