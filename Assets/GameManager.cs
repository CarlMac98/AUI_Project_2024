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
using Unity.Netcode;
using System.IO;
using System.Xml;

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
    OpenAIChatImage openAIChatImage;
    [SerializeField]
    PythonBackendManager pythonBackendManager;
    [SerializeField]
    SelectCharacter selectCharacter;
    [SerializeField]
    SelectStory selectStory;
    [SerializeField]
    GameObject[] serverClientScene;
    [SerializeField]
    MultiplayerUI multiplayerUI;


    [SerializeField]
    private Button quit, menu, help, X, cont, exit, download, endGame; //menuBackButt,
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
        Singleton = this;
    }
    void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += (clientId) =>
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                ExitGame();
                Debug.Log("Disconnected from the server.");
                // Handle disconnection (e.g., return to main menu)
            }
        };



        storyCreated = false;
        //initialize first go button
        goButton.onClick.AddListener(GoAhead);

        quit.onClick.AddListener(QuitGame);

        menu.onClick.AddListener(ShowMenu);
        cont.onClick.AddListener(CloseMenu);
        //menuBackButt.onClick.AddListener(CloseMenu);
        help.onClick.AddListener(ShowHelper);        
        X.onClick.AddListener(CloseHelper);
        exit.onClick.AddListener(ExitGame);
        download.onClick.AddListener(DownladStory);
        endGame.onClick.AddListener(ExitGame);


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
        //if (imageGenerated)
        //{
        //    Debug.Log("Image generated");
        //    imageGenerated = false;
        //    goButton.gameObject.SetActive(true);
        //}

    }

    public void GoAhead()
    {
        Debug.Log(currentScene);
        //check current number of scenes

        if (scenes.Length - 1 > currentScene)
        {
            if(currentScene > 0)
            {
                backButton.gameObject.SetActive(true);
            }
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
                    playerInput.text = "";
                }
                else
                {
                    playerInput.placeholder.GetComponent<TMP_Text>().text = "Dai un nome al tuo personaggio prima!";
                    playerName = "";
                    return;
                }
                setServerPlayerName(playerName);
            }

            //change the background when the game itself starts
            if (currentScene == 4)
            {
                chatManager.userName = playerName;
                helperManager.userName = playerName;
                

                background.HandleOff();
                //if (!reset && isServer)
                //{
                //    chatManager.HandleReset();
                //    reset = true;
                //}
            }

            //scenes going forward handling
            scenes[currentScene].SetActive(false);
            if (!isServer && currentScene == 2)
            {
                serverClientScene[0].SetActive(false);
                serverClientScene[1].SetActive(true);
            }
            if (isServer && currentScene == 2)
            {
                serverClientScene[0].SetActive(true);
                serverClientScene[1].SetActive(false);
            }
            currentScene += 1;
            scenes[currentScene].SetActive(true);
            if (currentScene == 4 && isServer && !storyCreated)
            {
                ns.storyReady.Value = true;
                StartCoroutine(chatManager.CreateStory(story));
                storyCreated = true;   
            }
            if (currentScene == 5 && isServer)
            {
                chatManager.HandleInitialMessage();
                chatManager.chatBox.interactable = true;
            }
            if (currentScene == 4)
            {
                download.gameObject.SetActive(false);
                endGame.gameObject.SetActive(false);
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
            if(isServer && currentScene == 3)
            {
                selectStory.HighlightReset();
            }
            if (!ns.storyReady.Value && !isServer && currentScene == 3)
            {
                goButton.gameObject.SetActive(false);
            }
            if (ns.storyReady.Value && !isServer && currentScene == 3)
            {
                goButton.gameObject.SetActive(true);
            }
            if (currentScene == 4)
            {
                if (imageGenerated)
                {
                    goButton.gameObject.SetActive(true);
                }
                else
                {
                    goButton.gameObject.SetActive(false);
                }
            }
            if (currentScene == 5)
            {
                selectCharacter.setCharacters();
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
                serverClientScene[0].SetActive(false);
                serverClientScene[1].SetActive(true);
            }
            if (isServer && currentScene == 4)
            {
                serverClientScene[0].SetActive(true);
                serverClientScene[1].SetActive(false);
            }
            currentScene -= 1;
            scenes.ToArray()[currentScene].SetActive(true);

            if (currentScene == 4)
            {
                download.gameObject.SetActive(false);
                endGame.gameObject.SetActive(false);
                backButton.gameObject.SetActive(false);
                background.HandleOn();
                if(!ns.conclusione.Value)
                {
                    background.HandleImageRequest();
                }
                else
                {
                    // Attivare i tasti di download con tasto di uscita
                    download.gameObject.SetActive(true);
                    endGame.gameObject.SetActive(true);
                }
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
        if(currentScene == 0)
        {
            NetworkManager.Singleton.Shutdown();
            multiplayerUI.hostBtn.onClick.AddListener(delegate { NetworkManager.Singleton.StartHost(); GameManager.isServer = true; });
        }
    }

    public void DownladStory()
    {
        string path = Path.Combine(Application.persistentDataPath, "Storia.txt");
        File.WriteAllText(path, chatManager.storySummary.text);
        Debug.Log($"Text saved to: {path}");
    }

    public void StoryReady() {
        if(currentScene == 3 && !isServer)
        {
            goButton.gameObject.SetActive(true);
        }
    }

    public IEnumerator GoToNextScene()
    {
        Debug.Log("Next scene");
        chatManager.Deactivate();
        yield return new WaitForSeconds(5);
        if (isServer)
        {
            ns.AskSummaryServerRpc();
            yield return new WaitForSeconds(5);
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
        //if(!ns.conclusione.Value)
        //{
        //    yield return new WaitForSeconds(20);
        //    GoAhead();
        //}

    }

    void ResetGameUI()
    {
        CloseMenu();
        CloseHelper();
        scenes[currentScene].SetActive(false);
        currentScene = 0;
        scenes[currentScene].SetActive(true);

        if (!GameObject.Find("Go").IsUnityNull())
        {
            goButton = GameObject.Find("Go").GetComponent<Button>();
            goButton.onClick.RemoveAllListeners();
            goButton.onClick.AddListener(GoAhead);
        }

        chatManager.userName = "";
        helperManager.userName = "";
        storyCreated = false;
        imageGenerated = false;
        selectCharacter.resetCharacters();
        background.HandleOn();
    }
    public void setServerPlayerName(string name)
    {
        Debug.Log("Changing player name");
        if (GameManager.isServer)
        {
            ns.host_name.Value = name;
        }
        else
        {
            ns.SetNameServerRpc(name);
            Debug.Log("Name successfully changed into: " + name);

        }
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
        chatManager.HandleReset();
        //pythonBackendManager.StopPythonBackend();
        ResetGameUI();
        NetworkManager.Singleton.Shutdown();
        multiplayerUI.hostBtn.onClick.AddListener(delegate { NetworkManager.Singleton.StartHost(); GameManager.isServer = true; });

        //pythonBackendManager.StartPythonBackend();
    }

    private void QuitGame()
    {
        openAIChatImage.ResetStory();
        pythonBackendManager.StopPythonBackend();
        
        #if UNITY_STANDALONE
                Application.Quit();
        #endif
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
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
