using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;


// This is used to communicate with your backend
public class OpenAIChatImage : MonoBehaviour
{

    private string backendEndpointChat = "http://127.0.0.1:7000/api/chat";
    private string backEndpointImage = "http://127.0.0.1:7000/api/image";
    public string response = "";
    public IEnumerator SendMessageToAzureChat(string promptToSend)
    {
        if (promptToSend.Length > 0)
            yield return StartCoroutine(SendRequest(promptToSend));
    }
    public IEnumerator RequestToAzureImage(string prompt)
    {    
        yield return StartCoroutine(RequestImage(prompt));
    }
    private IEnumerator SendRequest(string prompt)
    {
        string cleanPrompt = prompt.Trim();
        // Construct payload
        string jsonPayload = "{\"prompt\":\"" + cleanPrompt + "\"}";
        Debug.Log("Payload being sent: " + jsonPayload);

        // Set up the UnityWebRequest
        var request = new UnityWebRequest(backendEndpointChat, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
        }
        else
        {
            response = request.downloadHandler.text;
            if (response == "None")
            {
                Debug.Log("First interaction");
            }
            else
            {
                Debug.Log("Response: " + response);
            }
        }
    }

    private IEnumerator RequestImage(string prompt)
    {
        string cleanPrompt = prompt.Trim();
        // Construct payload
        string jsonPayload = "{}";
        Debug.Log("Payload being sent: " + jsonPayload);

        // Set up the UnityWebRequest
        var request = new UnityWebRequest(backEndpointImage, "POST");
        Debug.Log("Endpoint being called: " + backEndpointImage);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error: " + request.error);
            Debug.Log($"Response Code: {request.responseCode}");
            Debug.Log($"Response Text: {request.downloadHandler.text}");
        }
        else
        {
            Debug.Log("Image downloaded correctly");
            GameObject imageComponent = GameObject.Find("Bckg");
            RawImage backgroundImage = imageComponent.GetComponent<RawImage>();
            Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Images/Backgrounds/image0.png");
            backgroundImage.texture = newTexture;
        }
    }
}
