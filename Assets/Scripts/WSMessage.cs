using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WSMessage {
    [System.Serializable]
    public class RoomBody
    {
        public string name;
    }

    [System.Serializable]
    public class JoinedRoomBody
    {
        public string name;
        public string userId;
        public string username; 
    }

    [System.Serializable]
    public class UserPropertyBody
    {
        public string property;
        public string value;
        public string userId;
    }

    [System.Serializable]
    public class LeftRoomBody
    {
        public string userId;
    }

    [System.Serializable]
    public class Message
    {
        public string type;
    }

    [System.Serializable]
    public class Message<T> : Message
    {
        public T body;
    }

    [System.Serializable]
    public class RoomsResponse
    {
        public List<string> rooms;
    }

    [System.Serializable]
    public class RoomClientsResponse
    {
        public string room;
        public List<User> clients;
    }

    [System.Serializable]
    public class BatchTransform
    {
        public string type; // transform
        public string go; // gameobject name
        public long ts;
        public int scene; // Scene Number
        public string userId;
        public List<float> position; // type = transform
        public List<float> rotation; // type = transform
        public List<int> state;
    }

    [System.Serializable]
    public class BatchTransformationBody
    {
        public List<BatchTransform> transformations;
    }

    [System.Serializable]
    public class BroadcastMethodCallBody
    {
        public string method; 
    }

    [System.Serializable]
    public class User
    {
        public string userId;
        public string username;
        public bool isConnected; 
    }

    public class MessagesContainer
    {
        public List<Message> messages;
    }

    [System.Serializable]
    public class MessagesContainer<T> 
    {
        public List<Message<T>> messages;
    }

    [System.Serializable]
    public class GeneralBody 
    {
        // broadcast method call body
        public string method { get; set;  }

        // room body 
        public string name { get; set; }

        // ticks
        public string ticks { get; set; }

        // left room body 
        public string userId { get; set; }

        // joined room body
        public string username; 
        // public string name; duplicate
        // public string userId; duplicate

        // user property body 
        public string property { get; set; }
        public string value { get; set; }
        // public string userId; duplicate

        // batch transform body 
        public List<BatchTransform> transformations { get; set; }
    }
}
