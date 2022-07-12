// vis2k: GUILayout instead of spacey += ...; removed Update hotkeys to avoid
// confusion if someone accidentally presses one.
using UnityEngine;

using System.Threading.Tasks;

namespace Mirror
{
    /// <summary>Shows NetworkManager controls in a GUI at runtime.</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Network/Network Manager HUD")]
    [RequireComponent(typeof(NetworkManagerDevIO))]
    [HelpURL("https://mirror-networking.gitbook.io/docs/components/network-manager-hud")]
    public class DevIONetworkHUD : MonoBehaviour
    {
        NetworkManagerDevIO manager;

        public int offsetX;
        public int offsetY;

        void Awake()
        {
            manager = GetComponent<NetworkManagerDevIO>();
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, 300, 9999));
            if (!NetworkClient.isConnected && !NetworkServer.active)
            {
                StartButtons();

                if (manager.IsReadyToJoinGameSession())
                {
                    GUILayout.Label($"Session Ready");
                }
            }
            else
            {
                StatusLabels();
            }

            // client ready
            if (NetworkClient.isConnected && !NetworkClient.ready)
            {
                if (GUILayout.Button("Client Ready"))
                {
                    NetworkClient.Ready();
                    if (NetworkClient.localPlayer == null)
                    {
                        NetworkClient.AddPlayer();
                    }
                }
            }

            StopButtons();

            GUILayout.EndArea();
        }

        void StartButtons()
        {
            if (!NetworkClient.active)
            {
                // For testing game logic locally
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Start Local Server"))
                {
                    manager.StartServer();
                }
                else if (GUILayout.Button("Start Local Client"))
                {
                    manager.networkAddress = "localhost";
                    manager.StartClient();
                }

                GUILayout.EndHorizontal();

                // Client + IP
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Join A Game"))
                {
                    manager.ConnectToGame();
                }

                GUILayout.EndHorizontal();

            }
            else
            {
                // Connecting
                GUILayout.Label($"Connecting to {manager.networkAddress}..");
                if (GUILayout.Button("Cancel Connection Attempt"))
                {
                    manager.StopClient();
                }
            }
        }

        void StatusLabels()
        {
            // client only
            if (NetworkClient.isConnected)
            {
                GUILayout.Label($"<b>Client</b>: connected to {manager.networkAddress} via {Transport.activeTransport}");
            }
        }

        void StopButtons()
        {

            // stop client if client-only
            if (NetworkClient.isConnected)
            {
                if (GUILayout.Button("Disconnect"))
                {
                    manager.StopClient();
                }
            }

        }
    }
}
