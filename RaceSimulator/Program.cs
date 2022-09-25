using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Controller;
using Model;

namespace RaceSimulator
{
    public class Program
    {
        static void Main(string[] args)
        {
            Data.Initialize();
            Visualization.Initialize();

            for (; ; )
            {
                Thread.Sleep(100);
            }
        }
    }
}
