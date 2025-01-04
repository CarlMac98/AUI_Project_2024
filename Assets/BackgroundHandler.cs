using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class BackgroundHandler : MonoBehaviour
{

    [SerializeField]
    private GameObject[] bckgs;
    [SerializeField]
    private OpenAIChatImage imageSystem;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //GameObject imageBackground = new GameObject("ImageBackground");
        //imageSystem = imageBackground.AddComponent<OpenAIChatImage>();
        foreach (var bc in bckgs)
        {
            bc.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Handle()
    {
        bckgs[2].SetActive(false);
        bckgs[3].SetActive(false);
    }

    public void HandleImageRequest()
    {
        StartCoroutine(ProcessImageRequest());
    }
    private IEnumerator ProcessImageRequest()
    {
        yield return imageSystem.RequestToAzureImage();

    }


}
