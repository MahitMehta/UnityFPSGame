using System;
using System.Collections.Generic;

namespace WSMessage {
    public enum Type {
        JOIN_ROOM,
        JOINED_ROOM,
        CREATE_ROOM,
        CREATED_ROOM,
        LEFT_ROOM,
        SET_USER_PROPERTY,
        BATCH_TRANSFORM,
        BROADCAST_METHOD_CALL,
        SYNC_TICK
    }

    public enum BTType
    {
        Transform,
        Instantiate
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class UpdateAttribute : Attribute
    {
        public bool Subscribe;
        public int TickRate;

        public UpdateAttribute(bool Subscribe = false, int TickRate = 1)
        {
            this.Subscribe = Subscribe;
            this.TickRate = TickRate; 
        }
    }

    public class Update
    {
        public Action Callback { get; set;  }
        public bool Subscribe { get; set; } = false; 
        public int TickRate { get; set;  } = 1; 
    }

    [Serializable]
    public class RoomBody
    {
        public string name;
    }

    [Serializable]
    public class JoinedRoomBody
    {
        public string name;
        public string userId;
        public string username; 
    }

    [Serializable]
    public class UserPropertyBody
    {
        public string property;
        public string value;
        public string userId;
    }

    [Serializable]
    public class LeftRoomBody
    {
        public string userId;
    }

    [Serializable]
    public class Message
    {
        public Type type;
    }

    [Serializable]
    public class Message<T> : Message
    {
        public T body;
    }

    [Serializable]
    public class RoomsResponse
    {
        public List<string> rooms;
    }

    [Serializable]
    public class RoomClientsResponse
    {
        public string room;
        public List<User> clients;
    }

    [Serializable]
    public class BatchTransform
    {
        public BTType type; // transform
        public string go; // gameobject name
        public int ticks; 
        public string pf; // prefab name 
        public long ts;
        public int scene; // Scene Number
        public string userId;
        public List<float> position; // type = transform
        public List<float> rotation; // type = transform
        public List<int> state;
    }

    [Serializable]
    public class BatchTransformationBody
    {
        public List<BatchTransform> transformations;
    }

    [Serializable]
    public class BroadcastMethodCallBody
    {
        public string method;
        public object[] parameters;
    }

    [Serializable]
    public class User
    {
        public string userId;
        public string username;
        public bool isConnected;
        public string wizardClass = "FireWizard";
    }

    public class MessagesContainer
    {
        public List<Message> messages;
    }

    [Serializable]
    public class MessagesContainer<T> 
    {
        public List<Message<T>> messages;
    }

    [Serializable]
    public class GeneralBody 
    {
        // broadcast method call body
        public string method { get; set;  }
        
        // broadcast method call parameters
        public List<string> parameters { get; set;  }

        // room body 
        public string name { get; set; }

        // sync tick body and batch transformation body 
        public int ticks { get; set; }

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
