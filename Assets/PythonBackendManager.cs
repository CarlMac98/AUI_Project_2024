using System.Diagnostics;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class PythonBackendManager : MonoBehaviour
{
    private Process pythonProcess;
    private string flaskShutdownURL = "http://127.0.0.1:7001/shutdown";

    // Start is called before the first frame update
    void Start()
    {
        StartPythonBackend();
    }

    public void StartPythonBackend()
    {
        string pythonExecutable = "python"; // or specify the full path to python.exe
        string scriptPath = "server.py"; // Replace with your script path

        pythonProcess = new Process();
        pythonProcess.StartInfo.FileName = pythonExecutable;
        pythonProcess.StartInfo.Arguments = scriptPath;
        pythonProcess.StartInfo.UseShellExecute = false;
        pythonProcess.StartInfo.RedirectStandardOutput = true;
        pythonProcess.StartInfo.RedirectStandardError = true;
        pythonProcess.StartInfo.CreateNoWindow = true;

        pythonProcess.Start();

        // Optional: Log output or errors
        pythonProcess.OutputDataReceived += (sender, e) => UnityEngine.Debug.Log($"Python Output: {e.Data}");
        pythonProcess.ErrorDataReceived += (sender, e) => UnityEngine.Debug.LogError($"Python Error: {e.Data}");
        pythonProcess.BeginOutputReadLine();
        pythonProcess.BeginErrorReadLine();
    }
    public void StopFlaskServer()
    {
        StartCoroutine(SendShutdownRequest());
    }

    private IEnumerator SendShutdownRequest()
    {
        string jsonPayload = "";
        UnityEngine.Debug.Log("Server shutdown");

        var request = new UnityWebRequest(flaskShutdownURL, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Send the request and wait for a response
        yield return request.SendWebRequest();
    }
    private void OnApplicationQuit()
    {
        StopPythonBackend();
    }

    void StopPythonBackend()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            StopFlaskServer();
            pythonProcess.Kill();
            pythonProcess.Dispose();
        }
    }
}
