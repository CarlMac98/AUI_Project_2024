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
    Color selected = new Color(1, 1, 1, 1);
    Color unselected = new Color(1, 1, 1, 0.7f);

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

        
        

        for (int j = 0; j < rectTransforms.Length; j++)
        {
            RectTransform t = rectTransforms[j];
            t.localScale = Vector3.one;
            t.gameObject.GetComponent<Image>().color = unselected;
            if (i == j)
            {
                t.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                t.gameObject.GetComponent<Image>().color = selected;
            }
        }

        //gm.charachter = i;
        if (ns.IsHost)
        {
            ns.host_char.Value = i;
        } else
        {
            //ns.cli_char.Value += i;
            ns.ChangeCharNumServerRpc(i);
        }

    }
    public void setCharacters()
    {
        if (ns.IsHost) {
            showCharacterImage.ShowCharachterImage(0, ns.host_char.Value, ns.host_name.Value.ToString());
            showCharacterImage.ShowCharachterImage(2, ns.cli_char.Value, ns.cli_name.Value.ToString());
        }
        else
        {
            showCharacterImage.ShowCharachterImage(0, ns.cli_char.Value, ns.cli_name.Value.ToString());
            showCharacterImage.ShowCharachterImage(2, ns.host_char.Value, ns.host_name.Value.ToString());
        }
    }
    public void resetCharacters() {
        for (int j = 0; j < rectTransforms.Length; j++)
        {
            RectTransform t = rectTransforms[j];
            t.localScale = Vector3.one;
            t.gameObject.GetComponent<Image>().color = selected;
        }

    }
}
