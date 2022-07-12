using System;
using System.Collections;
using System.Threading.Tasks;
using System.Linq;

using UnityEngine;

using Mirror;
using kcp2k;



public class NetworkManagerDevIO : NetworkManager
{
    #region GameServer

#if UNITY_SERVER

    public override void Start()
    {
        StartHeadlessServer();
    }

#endif
    public GameLiftHeadlessServer headlessServer = null;

    void StartHeadlessServer()
    {
        Debug.Log("Mirror Server : StartHeadlessServer()");

        if (headlessServer.ParseNetwork())
        {
            base.networkAddress = headlessServer.NetworkAddress;
            GetComponent<KcpTransport>().Port = headlessServer.Port;

            StartServer();
        }
    }


    #endregion

    #region GameClient
    public GameLiftClientManager clientManager = null; // Assigned in the inspector
    private Task connectionTask = null;

    public void ConnectToGame()
    {
        StartCoroutine(ConnectToGameRoutine());
    }

    private IEnumerator ConnectToGameRoutine()
    {
        if (clientManager == null) yield break; // TODO: warning or throw a error

        while (!IsReadyToJoinGameSession())
        {
            if (connectionTask != null && connectionTask.Status != TaskStatus.RanToCompletion)
            {
                Debug.Log($"Client: Wait for Connection Task to be complete");
            }
            else
            {
                Debug.Log($"Client: Run clientManager.PrepareGameConnection()");
                connectionTask = Task.Run(() => clientManager.PrepareGameConnection());
            }

            yield return new WaitForSeconds(1.0f);

            // TODO: What if GameSession/PlayerSession never ready?
        }

        Debug.Log("Client: JoinGameSession()");
        JoinGameSession();
    }

    public bool IsReadyToJoinGameSession()
    {
        return clientManager.ManagedPlayerSession != null;
    }

    public void JoinGameSession()
    {
        var playerSession = clientManager.ManagedPlayerSession;

        if (playerSession != null)
        {
            networkAddress = playerSession.IpAddress;
            GetComponent<KcpTransport>().Port = Convert.ToUInt16(playerSession.Port);

            Debug.Log($"PlayerSession IP:{playerSession.IpAddress}, Port:{playerSession.Port}");

            StartClient();
        }
    }

    #endregion

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        SpawnPlayer(conn);
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // call base functionality (actually destroys the player)
        base.OnServerDisconnect(conn);
    }


    [SerializeField]
    private Transform[] spawningPositions = new Transform[4];

    void SpawnPlayer(NetworkConnectionToClient conn)
    {

        var playerPos = GetRandomPosition();
        GameObject player = Instantiate(playerPrefab, playerPos, Quaternion.identity);
        NetworkServer.AddPlayerForConnection(conn, player);
    }

    Vector3 GetRandomPosition()
    {
        Vector3 pos = Vector3.zero;

        pos.y = 1.5f;

        pos.x = UnityEngine.Random.Range(-10, 10);
        pos.z = UnityEngine.Random.Range(-10, 10);

        return pos;
    }



}
