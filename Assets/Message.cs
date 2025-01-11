using Unity.Netcode;

[System.Serializable]
public class Message : INetworkSerializable
{
    public string text;
    //public TMP_Text textObject;
    public messageType player;

    public string username;

    public string dest = "";

    public enum messageType
    {
        firstPlayerMessage,
        secondPlayerMessage,
        assistantMessage
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref text);
        //serializer.SerializeValue(ref textObject);
        serializer.SerializeValue(ref player);
        serializer.SerializeValue(ref username);
        serializer.SerializeValue(ref dest);
    }
}
