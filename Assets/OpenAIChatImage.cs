using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;


// This is used to communicate with your backend
public class OpenAIChatImage : MonoBehaviour
{

    private Dictionary<string, string> backendEndpoint = new Dictionary<string, string> {
        {"chat", "http://127.0.0.1:7001/api/chat"},
        {"init", "http://127.0.0.1:7001/api/init"},
        {"image", "http://127.0.0.1:7001/api/image"},
        {"reset", "http://127.0.0.1:7001/api/reset"}
    };
    public string response = "";
    public IEnumerator SendMessageToAzureChat(string promptToSend)
    {
        if (promptToSend.Length > 0)
            yield return StartCoroutine(SendRequest(promptToSend));
    }
    public IEnumerator InitialMessageAzureChat()
    {
        yield return StartCoroutine(SendInitialRequest());
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
        var request = new UnityWebRequest(backendEndpoint["chat"], "POST");
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
    private IEnumerator SendInitialRequest()
    {
        // Construct payload
        string jsonPayload = "";
        Debug.Log("Initial message sent");

        // Set up the UnityWebRequest
        var request = new UnityWebRequest(backendEndpoint["init"], "POST");
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
            Debug.Log("Response: " + response);
            
        }
    }
    public IEnumerator ResetStory()
    {
        // Construct payload
        string jsonPayload = "";
        Debug.Log("Reset");

        // Set up the UnityWebRequest
        var request = new UnityWebRequest(backendEndpoint["reset"], "POST");
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
            Debug.Log("Reset done");

        }
    }

    private IEnumerator RequestImage(string prompt)
    {
        string cleanPrompt = prompt.Trim();
        // Construct payload
        string jsonPayload = "{}";
        Debug.Log("Payload being sent: " + jsonPayload);

        // Set up the UnityWebRequest
        var request = new UnityWebRequest(backendEndpoint["image"], "POST");
        Debug.Log("Endpoint being called: " + backendEndpoint["image"]);
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
