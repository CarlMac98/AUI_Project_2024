using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

public class ButtonManager : MonoBehaviour
{
    [SerializeField]

    private int currentScene = 0;
    //public int numPlayers = 0;

    public GameObject firstScene, secondScene, thirdScene;
    //public GameObject[] scenes;
    public Button goButton, backButton;
    public LinkedList<GameObject> scenes = new LinkedList<GameObject>();
    
   
    void Start()
    {
        //scenes = new GameObject[firstScene, secondScene];
        scenes.AddLast(firstScene);
        scenes.AddLast(secondScene);
        scenes.AddLast(thirdScene);
        goButton.onClick.AddListener(GoAhead);
        backButton.onClick.AddListener(GoBack);

        firstScene.SetActive(true);
        //secondScene.SetActive(false);

        Debug.Log(scenes.Count);
    }

    void Update()
    {
        
    }

    public void GoAhead()
    {
        if (scenes.Count - 1 > currentScene) 
        {
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
            if(currentScene == 1)
            {
                
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
