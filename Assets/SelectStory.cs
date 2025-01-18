using UnityEngine;
using UnityEngine.UI;


public class SelectStory : MonoBehaviour
{
    //public Image border;
    private Button[] buttons;
    //public Outline[] outline;
    [SerializeField]
    private Image[] stories;
    [SerializeField]
    private RectTransform[] rectTransforms;

    public GameManager gm;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //border = gameObject.GetComponent<Image>();
        //outline = GetComponentsInChildren<Outline>();
        
        stories = GetComponentsInChildren<Image>();
        buttons = GetComponentsInChildren<Button>();
        //Debug.Log(stories.Length);
        rectTransforms = new RectTransform[stories.Length];

        for (int i = 0; i < stories.Length; i++)
        {
            rectTransforms[i] = stories[i].GetComponent<RectTransform>();
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

    //highlight the chosen story
    void Highlight(int i)
    {
        //Debug.Log(i);
        //foreach (var o in outline) o.enabled = false;
        //outline[i].enabled = true;

        for (int j = 0; j < rectTransforms.Length; j++)
        {
            RectTransform t = rectTransforms[j];
            t.localScale = Vector3.one;
            if (i == j) t.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }

        gm.story = i;
    }

    public void HighlightReset()
    {
        foreach (var t in rectTransforms)
        {
            t.localScale = Vector3.one;
        }
    }
}
