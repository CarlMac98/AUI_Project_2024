using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System;
using Unity.Collections;

public class NetSync : NetworkBehaviour
{
    public NetworkVariable<int> host_char = new NetworkVariable<int>();
    public NetworkVariable<int> cli_char = new NetworkVariable<int>();

    public NetworkVariable<bool> next_scene = new NetworkVariable<bool>();
    public NetworkVariable<bool> askSummary = new NetworkVariable<bool>();
    public NetworkVariable<bool> storyReady = new NetworkVariable<bool>();
    public NetworkVariable<bool> conclusione = new NetworkVariable<bool>();

    public NetworkVariable<FixedString32Bytes> host_name = new NetworkVariable<FixedString32Bytes>();
    public NetworkVariable<FixedString32Bytes> cli_name = new NetworkVariable<FixedString32Bytes>();

    public NetworkVariable<FixedString4096Bytes> recap = new NetworkVariable<FixedString4096Bytes>();
    

    //[SerializeField]
    //public GameManager gameManager;



    //public GameManager ;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            host_char.Value = 0;
            cli_char.Value = 0;
            next_scene.Value = false;
            askSummary.Value = false;
            storyReady.Value = false;
            conclusione.Value = false;

            host_name.Value = "Giocatore 1";
            cli_name.Value = "Giocatore 2";

            //if (NetworkManager.ConnectedClients.Count > 1)
            //{
            //    var clientId = NetworkManager.ConnectedClientsList[1].ClientId; // Example: Assign ownership to first client
            //    NetworkObject.ChangeOwnership(clientId);
            //}
            ////NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }
        else
        {

            if (host_char.Value != 0)
            {
                Debug.LogWarning($"NetworkVariable was {host_char.Value} upon being spawned" +
                    $" when it should have been {0}");
            }
            else
            {
                Debug.Log($"NetworkVariable is {host_char.Value} when spawned.");
                //cli_char.Value = 0;
            }
        }
        host_char.OnValueChanged += OnSomeValueChanged;
        cli_char.OnValueChanged += OnSomeValueChanged;
        askSummary.OnValueChanged += OnAskChanged;
        recap.OnValueChanged += OnRecapChanged;
        next_scene.OnValueChanged += OnSceneChanged;
        storyReady.OnValueChanged += OnStoryReadyChanged;
    }

    private void OnSceneChanged(bool previous, bool current)
    {
        if (current && GameManager.isServer)
        {
            Debug.Log("Next scene requested");
            next_scene.Value = false;
            ChangeSceneClientRpc();
        }
    }

    [ClientRpc]
    public void ChangeSceneClientRpc()
    {
        StartCoroutine(GameManager.Singleton.GoToNextScene());
    }

    //private void NetworkManager_OnClientConnectedCallback(ulong obj)
    //{
    //    StartCoroutine(StartChangingNetworkVariable());
    //}

    private void OnSomeValueChanged(int previous, int current)
    {
        Debug.Log($"Detected NetworkVariable Change: Previous: {previous} | Current: {current}");
    }
    private void OnStoryReadyChanged(bool previous, bool current)
    {
        if (current && !GameManager.isServer)
        {
            //storyReady.Value = false;
            Debug.Log("Story is ready");
            GameManager.Singleton.StoryReady();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void ChangeCharNumServerRpc(int i)
    {
        cli_char.Value = i;
    }
    [ServerRpc(RequireOwnership = false)]
    public void SetNameServerRpc(string name)
    {
        Debug.Log("Changing name to: " + name);
        cli_name.Value = name;
    }
    [ServerRpc(RequireOwnership = false)]
    public void AskSummaryServerRpc()
    {
        askSummary.Value = true;
    }
    private void OnAskChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            askSummary.Value = false;
            if (GameManager.isServer)
            {
                StartCoroutine(ChatManager.Singleton.RequestSummary());
            }
        }
    }

    private void OnRecapChanged(FixedString4096Bytes previousValue, FixedString4096Bytes newValue)
    {
        ChatManager.Singleton.VisualizeSummary();
    }
    //[ServerRpc(RequireOwnership = false)]
    //public void ChangeCharNameServerRpc(string name)
    //{
    //    cli_name.Value = name;
    //    Debug.Log(cli_name.Value);
    //}

    //private IEnumerator StartChangingNetworkVariable()
    //{
    //    var count = 0;
    //    var updateFrequency = new WaitForSeconds(0.5f);
    //    while (count < 4)
    //    {
    //        m_SomeValue.Value += m_SomeValue.Value;
    //        yield return updateFrequency;
    //    }
    //    NetworkManager.OnClientConnectedCallback -= NetworkManager_OnClientConnectedCallback;
    //}
}

