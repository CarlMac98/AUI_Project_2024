using UnityEngine;

public class BackgroundHandler : MonoBehaviour
{

    [SerializeField]
    private GameObject[] bckgs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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


}
