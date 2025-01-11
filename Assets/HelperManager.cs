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

    public GameObject chatPanel, textObject,  bubbleChat; //chatSection,

    public TMP_InputField chatBox;

    public Message msg;


    [SerializeField]
    List<Message> messageList = new List<Message>();


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Singleton = this;
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
                HandleHelpMessage(msg);
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
    public void HandleHelpMessage(Message message)
    {
        StartCoroutine(ProcessHelpMessage(message));
    }
    private IEnumerator ProcessHelpMessage(Message message)
    {
        //if (message.player == Message.messageType.firstPlayerMessage)
        //{
        //    SendHelpMessageServerRpc(message);
        //}

        if (GameManager.isServer && message.player != Message.messageType.assistantMessage)
        {
            yield return chatSystem.SendHelpMessage(message.text, message.username);
            if (!chatSystem.response.Equals("None"))
            {
                //sendMessageToChat("<color=red><b>" + "Assistant" + "</b></color>: " + chatSystem.response);

                Message msg = new Message();

                msg.text = chatSystem.response;
                msg.player = Message.messageType.assistantMessage;
                msg.username = "Assistant";

                SendHelpMessageServerRpc(msg);
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

    void AddMessage2(Message msg)
    {
        GameObject newText = Instantiate(bubbleChat, chatPanel.transform);

        TMP_Text user = newText.transform.Find("Username").GetComponent<TMP_Text>();
        TMP_Text msgText = newText.transform.Find("Msg").GetComponent<TMP_Text>();

        user.text = msg.username;
        msgText.text = msg.text;

        // Force TextMeshPro to update before adjusting size
        msgText.ForceMeshUpdate();

        // Adjust the RectTransform height to match the preferred height of the text
        RectTransform messageRect = msgText.GetComponent<RectTransform>();
        messageRect.sizeDelta = new Vector2(messageRect.sizeDelta.x, msgText.preferredHeight);



        Image bubbleBackground = newText.GetComponent<Image>();
        switch (msg.player)
        {
            case Message.messageType.assistantMessage:
                bubbleBackground.color = new Color(240f / 255f, 165f / 255f, 165f / 255f);
                break;
            case Message.messageType.firstPlayerMessage:
                bubbleBackground.color = new Color(165f / 255f, 224f / 255f, 240f / 255f);
                break;
            case Message.messageType.secondPlayerMessage:
                bubbleBackground.color = new Color(209f / 255f, 240f / 255f, 165f / 255f);
                break;
            default:
                bubbleBackground.color = new Color(1f, 1f, 1f);
                break;
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(newText.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(chatPanel.GetComponent<RectTransform>());

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
    void SendHelpMessageServerRpc(Message message)
    {
        //if (message.username == userName)
        ReceiveHelpMessageClientRpc(message);
    }

    [ClientRpc]
    void ReceiveHelpMessageClientRpc(Message message)
    {
        if (message.player != Message.messageType.assistantMessage && message.username != userName)
        {
            return;
        }
        else
        {
            Singleton.AddMessage2(message);
        }


    }
}


