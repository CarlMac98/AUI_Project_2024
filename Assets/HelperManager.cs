using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;
using System.Collections;
using Azure.AI.OpenAI.Assistants;
using OpenAI.Chat;
using System;


public class HelperManager : NetworkBehaviour
{
    public static HelperManager Singleton;

    [SerializeField]
    public string userName;
    [SerializeField]
    public OpenAIChatImage chatSystem;

    [SerializeField]
    private GameManager gameManager;

    [SerializeField]
    private NetSync ns;

    [SerializeField]
    //public Button storyButton, chatButton;

    public GameObject chatPanel, textObject, chatSection;

    public TMP_InputField chatBox;

    public Message msg;


    [SerializeField]
    List<Message> messageList = new List<Message>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        HelperManager.Singleton = this;
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
        if (message.player == Message.messageType.firstPlayerMessage)
        {
            //sendMessageToChat("<color=blue><b>" + message.username + "</b></color>: " + message.text);
            //ChatManager.Singleton.AddMessage(message);
            SendChatMessageServerRpc(message);
        }
        //else if(message.player == Message.messageType.secondPlayerMessage)
        //{
        //    //sendMessageToChat("<color=green><b>" + message.username + "</b></color>: " + message.text);
        //    ChatManager.Singleton.AddMessage(message);
        //    //SendChatMessageServerRpc(message);
        //}
        if (GameManager.isServer && message.player != Message.messageType.assistantMessage)
        {
            yield return chatSystem.SendMessageToAzureChat(message.username + ": " + message.text);
            if (!chatSystem.response.Equals("None"))
            {
                //sendMessageToChat("<color=red><b>" + "Assistant" + "</b></color>: " + chatSystem.response);

                Message msg = new Message();

                msg.text = chatSystem.response;
                msg.player = Message.messageType.assistantMessage;
                msg.username = "Assistant";

                SendChatMessageServerRpc(msg);
            }
        }


    }
    //public void HandleInitialMessage()
    //{
    //    StartCoroutine(InitialMessage());
    //}

    //public void HandleReset()
    //{
    //    StartCoroutine(ResetChat());
    //}
    //private IEnumerator ResetChat()
    //{
    //    messageList.Clear();
    //    yield return chatSystem.ResetStory();
    //}
    public void Deactivate()
    {
        chatBox.DeactivateInputField();
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

    void AddMessage(Message msg)
    {
        GameObject newText = Instantiate(textObject, chatPanel.transform);

        TMP_Text chat = newText.GetComponent<TMP_Text>();
        switch (msg.player)
        {
            case Message.messageType.assistantMessage:
                chat.text = "<color=red><b>" + msg.username + ":</b></color> " + msg.text;
                break;
            case Message.messageType.firstPlayerMessage:
                chat.text = "<color=blue><b>" + msg.username + ":</b></color> " + msg.text;
                break;
            case Message.messageType.secondPlayerMessage:
                chat.text = "<color=green><b>" + msg.username + ":</b></color> " + msg.text;
                break;
            default:
                chat.text = "Error";
                break;
        }
        messageList.Add(msg);
    }

    public void RemoveAllChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }



    [ServerRpc(RequireOwnership = false)]
    void SendChatMessageServerRpc(Message message)
    {
        ReceiveChatMessageClientRpc(message);
    }

    [ClientRpc]
    void ReceiveChatMessageClientRpc(Message message)
    {
        if (message.player != Message.messageType.assistantMessage && message.username != userName)
        {
            return;
        }
        else
        {
            Singleton.AddMessage(message);
        }


    }
}


