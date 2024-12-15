using UnityEngine;
using UnityEngine.UI;


public class SelectCharacter : MonoBehaviour
{
    [SerializeField]
    private ShowCharacterImage showCharacterImage;
    [SerializeField]
    private Button[] buttons;
    //public Outline[] outline;
    [SerializeField]
    private Image[] characters;
    [SerializeField]
    private RectTransform[] rectTransforms;

    public GameManager gm;
    public NetSync ns;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //border = gameObject.GetComponent<Image>();
        //outline = GetComponentsInChildren<Outline>();
        characters = GetComponentsInChildren<Image>();
        buttons = GetComponentsInChildren<Button>();
        
        //Debug.Log(characters.Length);
        rectTransforms = new RectTransform[characters.Length];

        for (int i = 0; i < characters.Length; i++)
        {
            
            rectTransforms[i] = characters[i].GetComponent<RectTransform>();
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int idx = i;    
            //Debug.Log(i);
            buttons[i].onClick.AddListener(() => Highlight(idx));
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //highlight chosen charachter
    void Highlight(int i)
    {
        //Debug.Log(i);
        //foreach (var o in outline) o.enabled = false;
        //outline[i].enabled = true;

        int hs = 0, cl = 2 ;

        for (int j = 0; j < rectTransforms.Length; j++)
        {
            RectTransform t = rectTransforms[j];
            t.localScale = Vector3.one;
            if(i == j) t.localScale = new Vector3(1.1f, 1.1f, 1.1f);
        }

        //gm.charachter = i;
        if (ns.IsHost)
        {
            ns.host_char.Value = i;
        }

        else
        {
            hs = 2;
            cl = 0;
            //ns.cli_char.Value += i;
            ns.ChangeCharNumServerRpc(i);
        }

        showCharacterImage.ShowCharachterImage(hs, ns.host_char.Value);
        showCharacterImage.ShowCharachterImage(cl, ns.cli_char.Value);

    }
}
