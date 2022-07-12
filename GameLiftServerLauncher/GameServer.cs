using Aws.GameLift.Server.Model;
using System.Diagnostics;
using System.IO;
using System;

namespace GameLiftServerLauncher
{
    class GameServer
    {
        public GameSession ManagedGameSession { get; private set; } = null;
        public Process ManagedServerProcess { get; private set; } = null;

        public GameServer(GameSession session)
        {
            ManagedGameSession = session;

            StartServer();
        }

        ~GameServer()
        {
            CloseServer();
        }


        public void StartServer()
        {
            Debug.Assert(ManagedGameSession != null);

            CreateServerProcessAndStart(ManagedGameSession.IpAddress, ManagedGameSession.Port);
        }

        void CreateServerProcessAndStart(string ip, int port)
        {
            Console.WriteLine("CreateServerProcessAndStart started");

            string dir = Directory.GetCurrentDirectory() + "/UnityServerBuild/devio2022_gamelift_session.exe";
            string arg = ip + " " + port; // Send IP and port info to Unity server process for parsing

            try
            {
                // Launch a Unity server build
                ManagedServerProcess = Process.Start(dir, arg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            // ManagedServerProcess = Process.Start(dir, arg);

            Console.WriteLine($"Started server at IP: {ip}, port: {port}");

            Console.WriteLine("CreateServerProcessAndStart ended");
        }

        public void UpdateGameSession(GameSession session)
        {
            ManagedGameSession = session;
        }


        public void CloseServer()
        {
            ManagedServerProcess?.Dispose();
            ManagedServerProcess?.Kill();

            ManagedGameSession = null;
            ManagedServerProcess = null;
        }
    }
}