using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;
using Microsoft.Identity.Client;
using ProcGenMusic;
using Unity.Netcode;
using UnityEngine.Rendering;


// This is used to communicate with your backend
public class OpenAIChatImage : MonoBehaviour
{
    [SerializeField]
    private NetSync ns;
    [SerializeField]
    public MusicGenerator mGenerator;

    [System.Serializable]
    public class Response
    {
        public bool next_scene;
        public string response;
        public int scala;
    }

    private Dictionary<string, string> backendEndpoint = new Dictionary<string, string> {
        {"chat", "http://127.0.0.1:7001/api/chat"},
        {"init", "http://127.0.0.1:7001/api/init"},
        {"image", "http://127.0.0.1:7001/api/image"},
        {"reset", "http://127.0.0.1:7001/api/reset"},
        {"summary", "http://127.0.0.1:7001/api/summary"},
        {"create_story", "http://127.0.0.1:7001/api/create_story"},
        {"help", "http://127.0.0.1:7001/api/help"}
    };
    public string response = "";
    public string summary = "";
    public string help = "";
    public IEnumerator SendMessageToAzureChat(string promptToSend)
    {
        if (promptToSend.Length > 0)
            yield return StartCoroutine(SendRequest(promptToSend));
    }
    public IEnumerator SendHelpMessage(string promptToSend, string user)
    {
        if (promptToSend.Length > 0)
            yield return StartCoroutine(SendHelpRequest(promptToSend, user));
    }
    public IEnumerator HandleCreateStory(int story)
    {
        yield return StartCoroutine(SendStory(story));
    }
    public IEnumerator InitialMessageAzureChat()
    {
        yield return StartCoroutine(SendInitialRequest());
    }
    public IEnumerator SendRequestSummmary() {
        yield return StartCoroutine(RequestSummary());
    }
    public IEnumerator RequestToAzureImage()
    {    
        yield return StartCoroutine(RequestImage());
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
            string response_json = request.downloadHandler.text;
            Debug.Log("Response: " + response_json);

            try
            {
                Response parsedResponse = JsonUtility.FromJson<Response>(response_json);

                Debug.Log("Next Scene: " + parsedResponse.next_scene);
                //Debug.Log("Response Message: " + parsedResponse.response);

                // cambiare variabile network cambio scena
                
                response = parsedResponse.response;
                ns.next_scene.Value = parsedResponse.next_scene;
                if (parsedResponse.next_scene && mGenerator.gameObject.activeInHierarchy)
                {
                    switch (parsedResponse.scala)
                    {
                        case 0:
                            mGenerator.ConfigurationData.Scale = Scale.Major;
                            break;
                        case 1:
                            mGenerator.ConfigurationData.Scale = Scale.HarmonicMajor;
                            break;
                        case 2:
                            mGenerator.ConfigurationData.Scale = Scale.NatMinor;
                            break;
                        case 3:
                            mGenerator.ConfigurationData.Scale = Scale.Major;
                            mGenerator.ConfigurationData.Mode = Mode.Lydian;
                            break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to parse JSON: " + ex.Message);
            }

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
    public IEnumerator SendStory(int story)
    {
        // Construct payload
        string jsonPayload = "{\"n_story\":\"" + story + "\"}";
        Debug.Log("Payload being sent: " + jsonPayload);

        // Set up the UnityWebRequest
        var request = new UnityWebRequest(backendEndpoint["create_story"], "POST");
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
            summary = request.downloadHandler.text;
            ns.recap.Value = summary;
            
            Debug.Log("Intro: " + summary);
        }
    }
    public IEnumerator SendHelpRequest(string prompt, string user)
    {
        string cleanPrompt = prompt.Trim();
        // Construct payload
        string jsonPayload = "{\"domanda\":\"" + cleanPrompt + "\", \"utente\":\""+ user + "\"}";
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
            help = request.downloadHandler.text;            
            Debug.Log("Help: " + help);
            
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
 
            //Debug.Log(ns.recap.Value);
        }
    }

    private IEnumerator RequestSummary() {
        // Construct payload
        string jsonPayload = "";
        Debug.Log("Summary request");

        // Set up the UnityWebRequest
        var request = new UnityWebRequest(backendEndpoint["summary"], "POST");
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
            summary = request.downloadHandler.text;
            //Story.Summary = summary;
            ns.recap.Value = summary;
            Debug.Log("Summary: " + summary);
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

    private IEnumerator RequestImage()
    {
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
            //yield return RequestImage();
        }
        else
        {
            
            string directory_image = request.downloadHandler.text;
            yield return new WaitForSeconds(0.5f);
            Texture2D newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(directory_image);
            byte[] imageBytes = newTexture.EncodeToPNG(); // Or EncodeToJPG if needed
            DebugRpc();
            ReceiveImageRpc(imageBytes);
            
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    //public void SendImageToClient(byte[] imageBytes)
    //{
    //    // Send the image to all clients
    //    ReceiveImageClientRpc(imageBytes);
    //}
    [Rpc(SendTo.ClientsAndHost)]
    public void DebugRpc()
    {
        Debug.Log("Receiving image");
    }
    [Rpc(SendTo.ClientsAndHost)]
    public void ReceiveImageRpc(byte[] imageBytes)
    {
        // Reconstruct the image on the client
        Texture2D receivedTexture = new Texture2D(2, 2); // Placeholder size, will resize
        receivedTexture.LoadImage(imageBytes);
        GameObject imageComponent = GameObject.Find("Bckg");
        RawImage backgroundImage = imageComponent.GetComponent<RawImage>();
        backgroundImage.texture = receivedTexture;
        GameManager.imageGenerated = true;
        Debug.Log("Image downloaded correctly");
    }
}
