using Controller;
using System;
using System.Threading;

namespace ConsoleEdition
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Data.Initialize();
            Data.NextRaceEvent += Visualization.OnNextRaceEvent;
            Data.NextRace();

            for (;;)
            {
                Thread.Sleep(100);
            }
        }
        
        public static void InitializeNextRace(object? Sender, EventArgs E)
        {
            Data.CurrentRace.CleanUp();
            Console.Clear();
            Data.NextRace();
            Visualization.Initialize(Data.CurrentRace);
        }
    }
}