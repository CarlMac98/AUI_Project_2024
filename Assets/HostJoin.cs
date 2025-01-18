using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MultiplayerUI : MonoBehaviour
{
    [SerializeField] public Button hostBtn, joinBtn;
    [SerializeField] GameManager gameManager;
    [SerializeField] GameObject networkManagerPrefab;
    [SerializeField] PythonBackendManager pythonBackendManager;
    void Awake()
    {
        AssignInputs();
    }

    void AssignInputs()
    {
        hostBtn.onClick.AddListener(delegate { NetworkManager.Singleton.StartHost(); GameManager.isServer = true; });
        joinBtn.onClick.AddListener(delegate { NetworkManager.Singleton.StartClient(); GameManager.isServer = false; });
        joinBtn.onClick.AddListener(delegate { gameManager.GoAhead(); });
    }
}
