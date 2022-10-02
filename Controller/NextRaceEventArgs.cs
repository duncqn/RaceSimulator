using System;

namespace Controller
{
    public class NextRaceEventArgs : EventArgs
    {
        public Race Race { get; set; }
    }
}