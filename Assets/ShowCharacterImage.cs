using UnityEngine;

public class ShowCharacterImage : MonoBehaviour
{
    [SerializeField]
    private GameObject[] images;
    [SerializeField]
    private GameManager gameManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SelectCharachterImage(gameManager.charachter);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectCharachterImage(int n)
    {
        foreach (var im in images)
        {
            im.SetActive(false);
        }
        images[n].SetActive(true);
    }
}
