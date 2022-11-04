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
        public Dictionary<IParticipant, int> equipment;
        public Dictionary<IParticipant, int> timesBrokenDown;
        public List<string> tracklist;

        public Competition()
        {
            Participants = new List<IParticipant>();
            Tracks = new Queue<Track>();
            totalRaceTime = new Dictionary<IParticipant, string>();
            equipment = new Dictionary<IParticipant, int>();
            timesBrokenDown = new Dictionary<IParticipant, int>();
            tracklist = new List<string>();
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