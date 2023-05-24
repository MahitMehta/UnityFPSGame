using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WSMessage;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager _instance;

    public Button createRoom;
    public Button joinRoom;

    public TMPro.TMP_InputField usernameField;
    public TMPro.TMP_InputField newRoomField;
    public TMPro.TMP_Dropdown roomOptions;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("LobbyScene"))
        {
            createRoom.onClick.AddListener(delegate {
                GameManager.Instance().SendMessages(new List<Message>() {
                    GameManager.Instance().ContructCreateRoomMessage(newRoomField.text),
                    GameManager.Instance().ContructUserPropertyMessage("username",  GameManager.Instance().userId, usernameField.text),
                });
            });

            joinRoom.onClick.AddListener(delegate {
                string selectedRoom = roomOptions.options[roomOptions.value].text;

                GameManager.Instance().SendMessages(new List<Message>() {
                    GameManager.Instance().ContructJoinRoomMessage(selectedRoom),
                    GameManager.Instance().ContructUserPropertyMessage("username",  GameManager.Instance().userId, usernameField.text),
                });
            });
        }
    }

    void Update()
    {
        
    }

    public static bool Exists()
    {
        return _instance != null;
    }

    public static LobbyManager Instance()
    {
        if (!Exists())
        {
            throw new Exception("Lobby Manager Doesn't Exist");
        }
        return _instance;
    }
}
