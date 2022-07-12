using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Amazon;
using Amazon.CognitoIdentity;
using Amazon.GameLift;
using Amazon.GameLift.Model;

using UnityEngine;


public class GameLiftClientManager : MonoBehaviour
{
    public AmazonGameLiftClient GameLiftClient { get; private set; } = null;
    public string PlayerUID { get; private set; } = string.Empty;

    public List<GameSession> ActiveGameSessionList { get; private set; } = null;

    public GameSession ManagedGameSession { get; private set; } = null;
    public PlayerSession ManagedPlayerSession { get; private set; } = null;

    private bool isLocalTest = false; // 最初ローカルテストの際はtrueでした

    public GameLiftClientManager()
    {
        PlayerUID = Guid.NewGuid().ToString();

        if(isLocalTest)
        {
            GameLiftClient = new AmazonGameLiftClient("fake", "fake", new AmazonGameLiftConfig() { ServiceURL = "http://localhost:9080" });
        }
        else
        {
            // Initialize the Amazon Cognito credentials provider
            CognitoAWSCredentials credentials = new CognitoAWSCredentials (
                "your cognito identity pool ID", // Identity pool ID
                RegionEndpoint.APNortheast1 // Region
            );

            GameLiftClient = new AmazonGameLiftClient(credentials, RegionEndpoint.APNortheast1);
        }

        Debug.Log($"GameLift Log: Player UID is {PlayerUID}");
    }

    async public Task PrepareGameConnection()
    {
        // Check if client already holds a PlayerSession
        if (ManagedPlayerSession != null)
        {
            // If PlayerSession is available, get the latest status of the GameSession, which the PlayerSession targets to
            // TODO: Might need to refresh GameSession info for several time
            await RefreshGameSessionInfo(ManagedPlayerSession.GameSessionId);

            // If the GameSession is not able to be joined, set it to null, for later to switch to another GameSession
            if (!IsGameSessionAvailable(ManagedGameSession))
            {
                ManagedGameSession = null;

                // Try find another available GameSession
                await FetchAvailableGameSessions();
                SetManagedGameSession(0);
            }
        }
        else
        {
            // If PlayerSession is not available, fetch the all the GameSessions available from the server
            await FetchAvailableGameSessions();

            // If there are GameSessions can be connected to, pick one of them, apply a request to create a PlayerSession from that GameSession
            // Set the first GameSession to join.
            // TODO: May need better strategy to pick which GameSession to join
            SetManagedGameSession(0);
        }

        // If there is no GameSession available, create one
        if (ManagedGameSession == null)
        {
            await CreateGameSessionAsync();
        }

        // TODO: What if create GameSession failed? 
        while(ManagedGameSession.Status != GameSessionStatus.ACTIVE)
        {
            // TODO: WebGL multi-thread issue
            //Thread.Sleep(1000);

            Debug.Log("await RefreshGameSessionInfo");
            // Update GameSession status until it's ACTIVE(can be joined)
            await RefreshGameSessionInfo(ManagedGameSession.GameSessionId);

            // TODO: What if it's been tried many times and GameSession is still not ACTIVE?
        }

        Debug.Log("await CreatePlayerSessionAsync");
        // Create a PlayerSession from that GameSession
        await CreatePlayerSessionAsync();

        // Connect to the game by the information(IP, ports, etc.) of the PlayerSession
        // 

    }

    private bool IsGameSessionAvailable(GameSession session)
    {
        bool isAvailable =
            (session != null) &&
            (session.Status == GameSessionStatus.ACTIVE) &&
            (session.MaximumPlayerSessionCount > session.CurrentPlayerSessionCount);

        return isAvailable;
    }

    public bool IsHeathy()
    {
        // TODO: Check GameLiftClient status
        return true;
    }

    public void SetManagedGameSession(int i)
    {
        if(ActiveGameSessionList.Any())
        {
            ManagedGameSession = ActiveGameSessionList.ElementAt(i);
        }
    }

    async public Task FetchAvailableGameSessions()
    {
        var request = new DescribeGameSessionsRequest();

        if(isLocalTest)
        {
            request.FleetId = "fleet-test";
        }
        else
        {
            request.FleetId = "fleet-your-fleet-id";
        }


        Debug.Log($"Client : await DescribeGameSessionsAsync");
        var response = await GameLiftClient.DescribeGameSessionsAsync(request);
        Debug.Log($"Client : finish DescribeGameSessionsAsync");

        ActiveGameSessionList = response.GameSessions.
            Where(session => IsGameSessionAvailable(session)).ToList();
    }

    async public Task CreateGameSessionAsync()
    {
        if (ManagedGameSession != null) return;

        var request = new CreateGameSessionRequest();

        if (isLocalTest)
        {
            request.FleetId = "fleet-test";
        }
        else
        {
            request.FleetId = "fleet-your-fleet-id";
        }

        request.CreatorId = PlayerUID;
        request.MaximumPlayerSessionCount = 4;

        Debug.Log($"Client : await CreateGameSessionAsync");
        var response = await GameLiftClient.CreateGameSessionAsync(request);
        Debug.Log($"Client : finish CreateGameSessionAsync");

        if (response.GameSession != null)
        {
            Debug.Log($"Client : GameSession Created!");
            Debug.Log($"Client : GameSession ID {response.GameSession.GameSessionId}!");
            ManagedGameSession = response.GameSession;
        }
        else
        {
            Console.Error.WriteLine($"Client : Failed creating GameSession!");
        }
    }

    // PlayerSession contains the ip
    async public Task CreatePlayerSessionAsync()
    {
        if (ManagedGameSession == null) return;

        var request = new CreatePlayerSessionRequest();
        request.GameSessionId = ManagedGameSession.GameSessionId;
        request.PlayerId = PlayerUID;

        //Debug.Log($"Client : Sleep for a while");
        //Thread.Sleep(5000);

        Debug.Log($"Client : await CreatePlayerSessionAsync");
        var response = await GameLiftClient.CreatePlayerSessionAsync(request);
        Debug.Log($"Client : finish CreatePlayerSessionAsync");

        if (response.PlayerSession != null)
        {
            Debug.Log($"Client : PlayerSession Created!");
            Debug.Log($"Client : PlayerSession ID {response.PlayerSession.PlayerSessionId}!");
            ManagedPlayerSession = response.PlayerSession;
        }
        else
        {
            Console.Error.WriteLine($"Client : Failed creating PlayerSession!");
        }
    }

    async public Task RefreshGameSessionInfo(string gameSessionId)
    {

        var request = new DescribeGameSessionsRequest();
        request.GameSessionId = gameSessionId;

        Debug.Log($"Client : await DescribeGameSessionDetailsAsync ");
        var response = await GameLiftClient.DescribeGameSessionsAsync(request);
        Debug.Log($"Client : finish DescribeGameSessionDetailsAsync ");

        if (response.GameSessions.First() != null)
        {
            Debug.Log($"Client : ConnectToGame() DescribeGameSessionDetailsAsync session available ");
            ManagedGameSession = response.GameSessions.First();
        }
    }

    public bool IsGameSessionActive()
    {
        if (ManagedGameSession == null) return false;

        return ManagedGameSession.Status == GameSessionStatus.ACTIVE;
    }

}