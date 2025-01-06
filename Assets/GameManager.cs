using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.ParticleSystem;
using System.Runtime.CompilerServices;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;
    //private vars
    public static bool isServer;
    public static bool imageGenerated = false;
    private int nPlayers = 0;
    private int currentScene = 0;
    [SerializeField]
    ChatManager chatManager;
    [SerializeField]
    HelperManager helperManager;
    [SerializeField]
    NetSync ns;
    [SerializeField]
    private Button menu, help, X, cont, exit; //menuBackButt,
    [SerializeField]
    private GameObject menuBckg, helpChat;
    
    //public vars

    public string playerName;
    //public int charachter = 0;
    public int story;

    public GameObject[] scenes;
    public Button goButton, backButton;
    public TMP_InputField playerInput;
    public TMP_Dropdown numPlayers;
    public BackgroundHandler background;
    private bool storyCreated;
    private bool reset = false;

    private void Awake()
    {
        GameManager.Singleton = this;
    }
    void Start()
    {
        storyCreated = false;
        //initialize first go button
        goButton.onClick.AddListener(GoAhead);

        menu.onClick.AddListener(ShowMenu);
        cont.onClick.AddListener(CloseMenu);
        //menuBackButt.onClick.AddListener(CloseMenu);
        help.onClick.AddListener(ShowHelper);        
        X.onClick.AddListener(CloseHelper);
        exit.onClick.AddListener(ExitGame);


        menuBckg.SetActive(false);
        helpChat.SetActive(false);

        //activate only first page
        foreach (var s in scenes)
        {
            s.SetActive(false);
        }
        scenes[0].SetActive(true);
    }

    void Update()
    {
        //if (ns.next_scene.Value)
        //{
        //    ns.next_scene.Value = false;
        //    StartCoroutine(GoToNextScene());           
        //}
        if (imageGenerated)
        {
            Debug.Log("Image generated");
            imageGenerated = false;
            goButton.gameObject.SetActive(true);
        }
        
    }

    public void GoAhead()
    {
        Debug.Log(currentScene);
        //check current number of scenes

        if (scenes.Length - 1 > currentScene)
        {
            //forbid passage to the third scene if name is not inserted
            if (currentScene == 1)
            {
                //if (!imageGenerated) {
                //    background.HandleImageRequest();
                //    imageGenerated = true;
                //}
                nPlayers = numPlayers.value + 1;
                playerInput.ActivateInputField();
    
                if (playerInput.text != "")
                {
                    playerName = playerInput.text;
                    //if(isServer)
                    //{
                    //    ns.host_name.Value = playerName;
                    //}
                    //else
                    //    ns.ChangeCharNameServerRpc(playerName);
                }
                else
                {
                    playerInput.placeholder.GetComponent<TMP_Text>().text = "Dai un nome al tuo personaggio prima!";
                    playerName = "";
                    return;
                }
            }

            //change the background when the game itself starts
            if (currentScene == 4)
            {
                chatManager.userName = playerName;
                helperManager.userName = playerName;

                background.HandleOff();
                if (!reset && isServer)
                {
                    chatManager.HandleReset();
                    reset = true;
                }
            }

            //scenes going forward handling
            scenes[currentScene].SetActive(false);
            if (!isServer && currentScene == 2)
            {
                currentScene++;
            }
            currentScene += 1;
            scenes[currentScene].SetActive(true);
            if (currentScene == 4 && isServer && !storyCreated)
            {        
                StartCoroutine(chatManager.CreateStory(story));
                storyCreated = true;   
            }
            //if (currentScene == 4 && !isServer)
            //{
            //    chatManager.VisualizeSummary();
            //}
            if (currentScene == 5 && isServer)
            {
                chatManager.HandleInitialMessage();
            }
            if (currentScene == 4)
            {
                background.HandleImageRequest();
            }
            //update buttons
            if (!GameObject.Find("Go").IsUnityNull())
            {
                goButton = GameObject.Find("Go").GetComponent<Button>();
                goButton.onClick.RemoveAllListeners();
                goButton.onClick.AddListener(GoAhead);
            }
            if (!GameObject.Find("Back").IsUnityNull())
            {
                backButton = GameObject.Find("Back").GetComponent<Button>();
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(GoBack);
            }
            if (currentScene == 4)
            {
                goButton.gameObject.SetActive(false);
            }
        }

    }

    public void GoBack()
    {
        //scenes going backward handling
        if (currentScene > 0)
        {
            if (currentScene == 4)
            {
                goButton.gameObject.SetActive(true);
            }
            scenes.ToArray()[currentScene].SetActive(false);
            if (!isServer && currentScene == 4)
            {
                currentScene--;
            }
            currentScene -= 1;
            scenes.ToArray()[currentScene].SetActive(true);

            if (currentScene == 4)
            {
                background.HandleOn();
                background.HandleImageRequest();
            }

            //update buttons
            if (!GameObject.Find("Go").IsUnityNull())
            {
                goButton = GameObject.Find("Go").GetComponent<Button>();
                goButton.onClick.RemoveAllListeners();
                goButton.onClick.AddListener(GoAhead);
            }
            if (!GameObject.Find("Back").IsUnityNull())
            {
                backButton = GameObject.Find("Back").GetComponent<Button>();
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(GoBack);
            }
        }
    }

    public IEnumerator GoToNextScene()
    {
        chatManager.Deactivate();
        yield return new WaitForSeconds(5);
        if (isServer)
        {
            ns.askSummary.Value = true;
        }
        else
        {
            yield return new WaitForSeconds(5);
        }
        chatManager.NextSceneReset();
        GoBack();

        if (!GameObject.Find("Go").IsUnityNull()) 
        {
            backButton.gameObject.SetActive(false);
            goButton.gameObject.SetActive(false);
        }
        //yield return StartCoroutine(background.ProcessImageRequest());

        //update riassuntozzo
        yield return new WaitForSeconds(20);
        GoAhead();
    }

    private void ShowMenu()
    {
       menuBckg.SetActive(true);
    }

    private void CloseMenu()
    {
        menuBckg.SetActive(false);
    }

    private void ExitGame()
    {

    }

    private void ShowHelper()
    {
        helpChat.SetActive(true);
    }

    private void CloseHelper()
    {
        helpChat.SetActive(false);
    }
}
