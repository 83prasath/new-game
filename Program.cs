// File name: Program.cs (Inside the plane folder)
using System;

namespace SpaceGame
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new instance of your game and launch it
            Game mySpaceGame = new Game();
            // Enemy myEnemy=new Enemy();
            // myEnemy.Draw(); // Example draw call for the enemy
            mySpaceGame.Run();
        }
    }
}
