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

            for (; ; )
            {
                Thread.Sleep(100);
            }
        }
    }
}