using UnityEngine;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField]
    private Button chatButton, storyButton;

    [SerializeField]
    private GameObject chat, story;
    
   
    void Start()
    {
        chat.SetActive(true);
        story.SetActive(false);

        chatButton.onClick.AddListener(SwitchPanel);
        storyButton.onClick.AddListener(SwitchPanel);
    }

    void Update()
    {
        
    }

    void SwitchPanel()
    {
        chat.SetActive(!chat.activeSelf);
        story.SetActive(!story.activeSelf);
    }
}
