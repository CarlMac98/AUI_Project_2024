using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField]
    private NetSync ns;

    [SerializeField]
    public Button storyButton, chatButton;

    public GameObject chatPanel, storyPanel, textObject, chatSection, summarySection, bubbleChat;
    public TMP_Text storySummary;
    public TMP_InputField chatBox;
    GameObject storyPanelText;

    //public Color playerMessage, player2Message;

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
        storyButton.onClick.AddListener(Summary);
        chatButton.onClick.AddListener(Chat);
        msg = new Message();
        msg.text = "";
        msg.player = Message.messageType.firstPlayerMessage;
        msg.username = "";
        storyPanelText = Instantiate(textObject, storyPanel.transform);
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
            yield return chatSystem.SendMessageToAzureChat(message.username + ": "+ message.text);
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
    public void HandleInitialMessage()
    {
        StartCoroutine(InitialMessage());
    }
    public IEnumerator CreateStory(int st)
    {
        yield return StartCoroutine(chatSystem.HandleCreateStory(st));
        //VisualizeSummary();
    }

    public void Summary()
    {
        Color brownOn = new Color(0.6156863f, 0.5137255f, 0.3254902f, 1f);
        Color brownOff = new Color(0.6156863f, 0.5137255f, 0.3254902f, 0f);
        chatSection.SetActive(false);
        chatButton.gameObject.GetComponent<Image>().color = brownOff;
        summarySection.SetActive(true);
        storyButton.gameObject.GetComponent<Image>().color = brownOn;
        ns.AskSummaryServerRpc();
        //if (GameManager.isServer)
        //{
        //    StartCoroutine(RequestSummary());
        //}
        //else
        //{
        //    askSummaryServerRpc();
        //}
    }
    public void VisualizeSummary() {

        TMP_Text chat = storyPanelText.GetComponent<TMP_Text>();
        //TMP_Text recap = recapPanelText.GetComponent<TMP_Text>();

        //chat.text = Story.Summary;
        chat.text = ns.recap.Value.ToString();
        storySummary.text = ns.recap.Value.ToString();
    }
    public void Chat() {
        Color brownOn = new Color(0.6156863f, 0.5137255f, 0.3254902f, 1f);
        Color brownOff = new Color(0.6156863f, 0.5137255f, 0.3254902f, 0f);
        chatSection.SetActive(true);
        chatButton.gameObject.GetComponent<Image>().color = brownOn;
        summarySection.SetActive(false);
        storyButton.gameObject.GetComponent<Image>().color = brownOff;
    }
    public IEnumerator RequestSummary() { 
        yield return chatSystem.SendRequestSummmary();
        //VisualizeSummary();
    }
    private IEnumerator InitialMessage()
    {
        yield return chatSystem.InitialMessageAzureChat();
        Message msg = new Message();

        msg.text = chatSystem.response;
        msg.player = Message.messageType.assistantMessage;
        msg.username = "Assistant";

        SendChatMessageServerRpc(msg);
    }
    public void HandleReset()
    {
        StartCoroutine(ResetChat());
    }
    private IEnumerator ResetChat()
    {
        messageList.Clear();
        yield return chatSystem.ResetStory();
    }
    public void Deactivate() {
        chatBox.DeactivateInputField();
    }
    public void NextSceneReset() 
    {
        //yield return new WaitForSeconds(5);
        messageList.Clear();
        RemoveAllChildren(chatPanel);
        chatBox.ActivateInputField();
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
    void askSummaryServerRpc()
    {
        askSummaryClientRpc();
    }
    [ClientRpc]
    void askSummaryClientRpc()
    {
        if (GameManager.isServer)
        {
            StartCoroutine(RequestSummary());
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
        if (message.player != Message.messageType.assistantMessage && message.username != userName){
            message.player = Message.messageType.secondPlayerMessage;
            Debug.Log("Second player wrote");
            HandleChatMessage(message);
        }
        
        ChatManager.Singleton.AddMessage2(message);
    }
}

[System.Serializable]
public class Message : INetworkSerializable
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

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref text);
        //serializer.SerializeValue(ref textObject);
        serializer.SerializeValue(ref player);
        serializer.SerializeValue(ref username);
    }
}

