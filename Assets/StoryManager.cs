using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class StoryManager : MonoBehaviour
{
    [SerializeField]
    public string userName;

    [SerializeField]
    private GameManager gameManager;

    //public int maxMessages = 25;

    public GameObject chatPanel, textObject;//, wholeChat;
    public TMP_InputField chatBox;

    public Color playerMessage, info;


    [SerializeField]
    List<Message> messageList = new List<Message>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //userName = gameManager.playerName;
        //wholeChat.SetActive(false);
        //chatButton.onClick.AddListener(ShowChat);
    }

    // Update is called once per frame
    void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                sendMessageToChat(userName + ": " + chatBox.text, Message.messageType.playerMessage);
                chatBox.text = "";
            }
        }

        else
        {
            if (!chatBox.isFocused && Input.GetKeyDown(KeyCode.Return))
            {
                chatBox.ActivateInputField();
            }
        }
    }
    public void sendMessageToChat(string text, Message.messageType messageType)
    {
        //if (messageList.Count >= maxMessages)
        //{
        //    Destroy(messageList[0].textObject.gameObject);
        //    messageList.Remove(messageList[0]);
        //}

        Message newMessage = new Message();

        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);

        newMessage.textObject = newText.GetComponent<TMP_Text>();

        newMessage.textObject.text = newMessage.text;
        newMessage.textObject.color = messageTypeColor(messageType);

        messageList.Add(newMessage);
    }

    Color messageTypeColor(Message.messageType messageType)
    {
        Color color = info;

        switch (messageType)
        {
            case Message.messageType.playerMessage:
                color = playerMessage;
                break;
        }

        return color;
    }
}

//[System.Serializable]
//public class Message
//{
//    public string text;
//    public TMP_Text textObject;

//    public enum messageType
//    {
//        playerMessage,
//        info
//    }
//}

