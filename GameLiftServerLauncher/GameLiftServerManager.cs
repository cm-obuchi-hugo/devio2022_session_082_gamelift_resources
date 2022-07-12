using Aws.GameLift.Server;
using Aws.GameLift.Server.Model;

namespace GameLiftServerLauncher
{
    class GameLiftServerManager
    {
        public bool IsAlive { get; private set; } = false;
        
        private readonly int portMin = 7000;
        private readonly int portMax = 20000;

        private int port = int.MinValue;

        // A instance managing Unity server build
        private GameServer server = null;

        public GameLiftServerManager()
        {
            
        }

        public void Start()
        {
            InitGameLiftSdk();
        }

        private int GetValidPort()
        {
            // For now (March 2022), the GameLift seems not support multi-thread for single EC2 instance(session),
            // But in the future, when multi-thread is supported, multi-port method will be needed

            // int port = new Random().Next(portMin, portMax);

            // return port;
            return 9080;
        }

        private void InitGameLiftSdk()
        {
            //InitSDK will establish a local connection with GameLift's agent to enable further communication.
            Console.WriteLine(GameLiftServerAPI.GetSdkVersion().Result);
            
            port = GetValidPort();

            var initSDKOutcome = GameLiftServerAPI.InitSDK();
            if (initSDKOutcome.Success)
            {
                ProcessParameters processParameters = new ProcessParameters(
                    this.OnStartGameSession,
                    this.OnUpdateGameSession,
                    this.OnProcessTerminate,
                    this.OnHealthCheck,
                    port, //This game server tells GameLift that it will listen on port 7777 for incoming player connections.
                    new LogParameters(new List<string>()
                    {
                    //Here, the game server tells GameLift what set of files to upload when the game session ends.
                    //GameLift will upload everything specified here for the developers to fetch later.
                    "/local/game/logs/myserver.log"
                    }));

                //Calling ProcessReady tells GameLift this game server is ready to receive incoming game sessions!
                var processReadyOutcome = GameLiftServerAPI.ProcessReady(processParameters);
                if (processReadyOutcome.Success)
                {
                    // Set Server to alive when ProcessReady() returns success
                    IsAlive = true;

                    Console.WriteLine("ProcessReady success.");
                }
                else
                {
                    IsAlive = false;
                    Console.WriteLine("ProcessReady failure : " + processReadyOutcome.Error.ToString());
                }
            }
            else
            {
                IsAlive = true;
                Console.WriteLine("InitSDK failure : " + initSDKOutcome.Error.ToString());
            }
        }

        private void OnStartGameSession(GameSession gameSession)
        {
            server = new GameServer(gameSession);

            //When a game session is created, GameLift sends an activation request to the game server and passes along the game session object containing game properties and other settings.
            //Here is where a game server should take action based on the game session object.
            //Once the game server is ready to receive incoming player connections, it should invoke GameLiftServerAPI.ActivateGameSession()
            Console.WriteLine($"Server : OnStartGameSession() called");
            GameLiftServerAPI.ActivateGameSession();
        }

        private void OnUpdateGameSession(UpdateGameSession updateGameSession)
        {
            server.UpdateGameSession(updateGameSession.GameSession);

            //When a game session is updated (e.g. by FlexMatch backfill), GameLiftsends a request to the game
            //server containing the updated game session object.  The game server can then examine the provided
            //matchmakerData and handle new incoming players appropriately.
            //updateReason is the reason this update is being supplied.
            Console.WriteLine($"Server : OnUpdateGameSession() called");
        }

        private void OnProcessTerminate()
        {
            //OnProcessTerminate callback. GameLift will invoke this callback before shutting down an instance hosting this game server.
            //It gives this game server a chance to save its state, communicate with services, etc., before being shut down.
            //In this case, we simply tell GameLift we are indeed going to shutdown.
            Console.WriteLine($"Server : OnProcessTerminate() called");
            GameLiftServerAPI.ProcessEnding();
        }

        private bool OnHealthCheck()
        {
            //This is the HealthCheck callback.
            //GameLift will invoke this callback every 60 seconds or so.
            //Here, a game server might want to check the health of dependencies and such.
            //Simply return true if healthy, false otherwise.
            //The game server has 60 seconds to respond with its health status. GameLift will default to 'false' if the game server doesn't respond in time.
            //In this case, we're always healthy!
            Console.WriteLine($"Server : OnHealthCheck() called");
            return true;
        }

        private void OnApplicationQuit()
        {
            //Make sure to call GameLiftServerAPI.Destroy() when the application quits. This resets the local connection with GameLift's agent.
            GameLiftServerAPI.Destroy();
            server = null;

            IsAlive = false;
        }
    }
}