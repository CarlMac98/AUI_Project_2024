using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


// This is used to communicate with your backend
public class OpenAIChat : MonoBehaviour
{

    private string backendEndpoint = "http://127.0.0.1:5000/api/demo";
    [SerializeField] private string promptToSend = "";

    public void SendMessageToAzure()
    {
        if (promptToSend.Length > 0)
            StartCoroutine(SendRequest(promptToSend));
    }

    private IEnumerator SendRequest(string prompt)
    {

        // Construct payload
        string jsonPayload = "{\"prompt\":\"" + prompt + "\"}";
        Debug.Log("Payload being sent: " + jsonPayload);

        // Set up the UnityWebRequest
        var request = new UnityWebRequest(backendEndpoint, "POST");
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
            string answer = request.downloadHandler.text;
            if (answer == "None")
            {
                Debug.Log("First interaction");
            }
            else
            {
                Debug.Log("Response: " + answer);
            }
        }
    }
}
