using System;

namespace Model
{
    public class DriversChangedEventArgs : EventArgs
    {
        public Track Track { get; set; }
    }
}