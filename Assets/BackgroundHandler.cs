using UnityEngine;
using UnityEngine.UI;

public class BackgroundHandler : MonoBehaviour
{

    [SerializeField]
    private RawImage[] bckgs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Handle()
    {
        //bckgs[0].enabled = true;
        //bckgs[1].enabled = true;
        bckgs[2].enabled = false;
        bckgs[3].enabled = false;
    }


}
