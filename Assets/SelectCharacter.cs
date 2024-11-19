using UnityEngine;
using UnityEngine.UI;


public class SelectCharacter : MonoBehaviour
{
    [SerializeField]
    //public Image border;
    public Button[] button;
    public Outline[] outline;
    public GameManager gm;
    //private bool toggle = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //border = gameObject.GetComponent<Image>();
        outline = GetComponentsInChildren<Outline>();
        button = GetComponentsInChildren<Button>();

        for (int i = 0; i < button.Length; i++)
        {
            int idx = i;    
            //Debug.Log(i);
            button[i].onClick.AddListener(() => ActivateBorder(idx));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ActivateBorder(int i)
    {
        //Debug.Log(i);
        foreach (var o in outline) o.enabled = false;
        outline[i].enabled = true;

        gm.charachter = i;
    }
}
