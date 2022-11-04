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
            Data.NextRaceEvent += Visualisation.OnNextRaceEvent;
            Data.NextRace();

            for (;;)
            {
                Thread.Sleep(100);
            }
        }
        
        public static void InitializeNextRace(object? Sender, EventArgs E)
        {
            Console.Clear();
            Data.NextRace();
            Visualisation.Initialize(Data.CurrentRace);
        }
    }
}