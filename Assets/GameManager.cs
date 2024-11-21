using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.ParticleSystem;
using System.Runtime.CompilerServices;

public class GameManager : MonoBehaviour
{
    [SerializeField]

    //private vars

    private string playerName;
    
    private int nPlayers = 0;
    private int currentScene = 0;
    
    //public vars 
    public int charachter = 0;
    public int story;

    public GameObject[] scenes;
    public Button goButton, backButton;
    public TMP_InputField playerInput;
    public TMP_Dropdown numPlayers;
    public BackgroundHandler background;


    void Start()
    {
        //initialize first go button
        goButton.onClick.AddListener(GoAhead);

        //activate only first page
        foreach (var s in scenes)
        {
            s.SetActive(false);
        }
        scenes[0].SetActive(true);
    }

    void Update()
    {

    }

    public void GoAhead()
    {
        //check current number of scenes
        if (scenes.Length - 1 > currentScene)
        {
            //forbid passage to the third scene if name is not inserted
            if (currentScene == 1)
            {
                nPlayers = numPlayers.value + 1;
                //Debug.Log(nPlayers);    
                if (playerInput.text != "")
                {
                    playerName = playerInput.text;
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
                background.Handle();
            }


            //scenes going forward handling
            scenes[currentScene].SetActive(false);
            currentScene += 1;
            scenes[currentScene].SetActive(true);

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

    public void GoBack()
    {
        //scenes going backward handling
        if (currentScene > 0)
        {
            scenes.ToArray()[currentScene].SetActive(false);
            currentScene -= 1;
            scenes.ToArray()[currentScene].SetActive(true);
            
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
}
