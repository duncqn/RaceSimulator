using System;

namespace Model
{
    public class DriversChangedEventArgs : EventArgs
    {
        public Track Track { get; }

        public DriversChangedEventArgs(Track track)
        {
            Track = track;
        }
    }
}