using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections;
using Azure.AI.OpenAI.Assistants;
using OpenAI.Chat;
using System;


public class ChatManager : NetworkBehaviour
{
    public static ChatManager Singleton;

    [SerializeField]
    public string userName;
    [SerializeField]
    public OpenAIChatImage chatSystem;

    [SerializeField]
    private GameManager gameManager;

    public GameObject chatPanel, textObject;
    public TMP_InputField chatBox;

    public Color playerMessage, player2Message;

    public Message msg;


    [SerializeField]
    List<Message> messageList = new List<Message>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        ChatManager.Singleton = this;
    }

    void Start()
    {
        msg = new Message();
        msg.text = "";
        msg.player = Message.messageType.firstPlayerMessage;
        msg.username = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                msg.text = chatBox.text;
                chatBox.text = "";
                msg.username = userName;
                HandleChatMessage(msg);
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
    public void HandleChatMessage(Message message)
    {
        StartCoroutine(ProcessChatMessage(message));
    }
    private IEnumerator ProcessChatMessage(Message message)
    {
        if(message.player == Message.messageType.firstPlayerMessage)
        {
            sendMessageToChat("<color=blue><b>" + message.username + "</b></color>: " + message.text);
            SendChatMessageServerRpc(message.text);
        }
        else if(message.player == Message.messageType.secondPlayerMessage)
        {
            sendMessageToChat("<color=green><b>" + message.username + "</b></color>: " + message.text);
            //SendChatMessageServerRpc(message);
        }
        yield return chatSystem.SendMessageToAzureChat(message.text);
        if (!chatSystem.response.Equals("None"))
        {
            sendMessageToChat("<color=red><b>" + "Assistant" + "</b></color>: " + chatSystem.response);

            Message msg = new Message();

            msg.text = chatSystem.response;
            msg.player = Message.messageType.assistantMessage;

            SendChatMessageServerRpc(message.text);
        }
        
    }
    public void HandleInitialMessage()
    {
        StartCoroutine(InitialMessage());
    }
    private IEnumerator InitialMessage()
    {
        yield return chatSystem.InitialMessageAzureChat();
        sendMessageToChat("<color=red><b>" + "Assistant" + "</b></color>: " + chatSystem.response);
    }

    public void sendMessageToChat(string text)
    {
        
        Message newMessage = new Message();

        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);

        TMP_Text chat = newText.GetComponent<TMP_Text>();

        chat.text = newMessage.text;
        //chat.color = messageTypeColor(messageType);

        messageList.Add(newMessage);
    }

    //Color messageTypeColor(Message.messageType messageType)
    //{
    //    Color color = Color.black;

    //    switch (messageType)
    //    {
    //        case Message.messageType.firstPlayerMessage:
    //            color = playerMessage;
    //            break;
    //        case Message.messageType.secondPlayerMessage:
    //            color = player2Message;
    //            break;
    //    }

    //    return color;
    //}

    void AddMessage(Message msg)
    {
        //Message CM = Instantiate(msg, chatPanel.transform);
        ////CM.SetText(msg);
        //CM.text = msg.text;
        GameObject newText = Instantiate(textObject, chatPanel.transform);

        TMP_Text chat = newText.GetComponent<TMP_Text>();

        chat.text = msg.text;
    }

    [ServerRpc(RequireOwnership = false)]
    void SendChatMessageServerRpc(string message)
    {
        ReceiveChatMessageClientRpc(message);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(string message)
    {
        //if (message.player != Message.messageType.assistantMessage)
        //    message.player = Message.messageType.secondPlayerMessage;
        //ChatManager.Singleton.AddMessage(message);
        Message msg = new Message();
        msg.text = message;
        msg.username = "pippo";
        if (msg.username == "Assistant")
            msg.player = Message.messageType.assistantMessage;
        else
            msg.player = Message.messageType.secondPlayerMessage;
        ChatManager.Singleton.AddMessage(msg);
    }
}

public class Message
{
    public string text;
    //public TMP_Text textObject;
    public messageType player;
    public string username;

    public enum messageType
    {
        firstPlayerMessage,
        secondPlayerMessage,
        assistantMessage
    }

}

