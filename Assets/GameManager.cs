using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEngine.ParticleSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField]

    private string playerName;
    private int nPlayers = 0;
    public int charachter = 0;
    public int story;
    
    private int currentScene = 0;
    
    //public GameObject firstScene, secondScene, thirdScene, fourthScene;
    public GameObject[] scenes;
    public Button goButton, backButton;
    //public LinkedList<GameObject> scenes = new LinkedList<GameObject>();
    public TMP_InputField playerInput;
    public TMP_Dropdown numPlayers;


    void Start()
    {
        //scenes = new GameObject[firstScene, secondScene];
        //scenes.AddLast(firstScene);
        //scenes.AddLast(secondScene);
        //scenes.AddLast(thirdScene);
        //scenes.AddLast(fourthScene);
        //scenes = GameObject.FindGameObjectsWithTag("Scene");
        goButton.onClick.AddListener(GoAhead);
        //backButton.onClick.AddListener(GoBack);

        foreach (var s in scenes)
        {
            s.SetActive(false);
        }
        scenes[0].SetActive(true);
        //Debug.Log(scenes.Count);
    }

    void Update()
    {

    }

    public void GoAhead()
    {
        if (scenes.Length - 1 > currentScene)
        {
            if (currentScene == 1)
            {
                nPlayers = numPlayers.value + 1;
                Debug.Log(nPlayers);    
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
            scenes.ToArray()[currentScene].SetActive(false);
            currentScene += 1;
            scenes.ToArray()[currentScene].SetActive(true);

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
        if (currentScene > 0)
        {
            scenes.ToArray()[currentScene].SetActive(false);
            currentScene -= 1;
            scenes.ToArray()[currentScene].SetActive(true);

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
