using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowCharacterImage : MonoBehaviour
{
    [SerializeField]
    private Sprite[] images;
    [SerializeField]
    private GameManager gameManager;
    [SerializeField]
    private Image[] imgContainers;

    //private Transform tr;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ShowCharachterImage(1, 4);         
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowCharachterImage(int i,int n)
    {
        //foreach (var im in images)
        //{
        //    im.SetActive(false);
        //}
        //images[n].SetActive(true);
        Image cont = imgContainers[i];
        Sprite img = images[n];

        cont.sprite = img;
    }
}
