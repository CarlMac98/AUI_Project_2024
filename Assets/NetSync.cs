using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class NetSync : NetworkBehaviour
{
    public NetworkVariable<int> host_char = new NetworkVariable<int>();
    public NetworkVariable<int> cli_char = new NetworkVariable<int>();

    public NetworkVariable<string> host_name = new NetworkVariable<string>();
    public NetworkVariable<string> cli_name = new NetworkVariable<string>();

    //public GameManager ;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            host_char.Value = 0;
            cli_char.Value = 0;
            host_name.Value = "";
            cli_name.Value = "";

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
    }

    //private void NetworkManager_OnClientConnectedCallback(ulong obj)
    //{
    //    StartCoroutine(StartChangingNetworkVariable());
    //}

    private void OnSomeValueChanged(int previous, int current)
    {
        Debug.Log($"Detected NetworkVariable Change: Previous: {previous} | Current: {current}");
    }


    [ServerRpc(RequireOwnership = false)]
    public void ChangeCharNumServerRpc(int i)
    {
        cli_char.Value = i;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ChangeCharNameServerRpc(string name)
    {
        cli_name.Value = name;
    }

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

