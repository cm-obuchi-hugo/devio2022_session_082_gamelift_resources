using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameLiftHeadlessServer : MonoBehaviour
{
    public string NetworkAddress { get; private set; } = string.Empty; // IP
    public ushort Port { get; private set; } = ushort.MinValue; // Port

    public bool ParseNetwork()
    {
        Console.WriteLine($"Entered StartHeadLess()");

        while (true)
        {
            // Parse env params which sent by the launcher
            string[] args = Environment.GetCommandLineArgs();

            // No [ip], [port] args available
            if(args == null) return false;

            // Parse [ip] and [port]
            if (args[1] != "0" && args[2] != "0")
            {
                Console.WriteLine($"Target IP is {args[1]}, target port is {args[2]}");

                NetworkAddress = args[1];
                Port = ushort.Parse(args[2]);

                break;
            }

        }

        return true;
    }
}
