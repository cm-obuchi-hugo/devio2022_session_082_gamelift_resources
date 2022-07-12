// See https://aka.ms/new-console-template for more information

GameLiftServerManager gameLiftServerManager = new GameLiftServerManager();

gameLiftServerManager.Start();

while (gameLiftServerManager.IsAlive)
{

}

Console.WriteLine("Program ends.");



