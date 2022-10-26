using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Competition
    {
        public List<IParticipant> Participants { get; set; }
        public Queue<Track> Tracks { get; set; }
        public Dictionary<IParticipant, string> totalRaceTime;
        public Dictionary<IParticipant, int> points;
        public Dictionary<IParticipant, string> equipment;
        public Dictionary<IParticipant, int> timesBrokenDown;

        public Competition()
        {
            Participants = new List<IParticipant>();
            Tracks = new Queue<Track>();
            totalRaceTime = new Dictionary<IParticipant, string>();
            points = new Dictionary<IParticipant, int>();
            equipment = new Dictionary<IParticipant, string>();
            timesBrokenDown = new Dictionary<IParticipant, int>();
        }

        public Track NextTrack()
        {
            if (Tracks.Count > 0)
            {
                return Tracks.Dequeue();
            }
            return null;
        }
    }
}