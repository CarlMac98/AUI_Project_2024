using System.Diagnostics;
using UnityEngine;

public class PythonBackendManager : MonoBehaviour
{
    private Process pythonProcess;

    // Start is called before the first frame update
    void Start()
    {
        StartPythonBackend();
    }

    void StartPythonBackend()
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

    private void OnApplicationQuit()
    {
        StopPythonBackend();
    }

    void StopPythonBackend()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
            pythonProcess.Dispose();
        }
    }
}
