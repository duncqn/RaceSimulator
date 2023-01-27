using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Competition
    {
        public List<IParticipant> Participants { get; }
        public Queue<Track> Tracks { get;}
        public Dictionary<IParticipant, int> Speed { get; }
        public Dictionary<IParticipant, int> TimesBrokenDown { get; }
        public List<string> Tracklist { get; }

        public Competition()
        {
            Participants = new List<IParticipant>();
            Tracks = new Queue<Track>();
            Speed = new Dictionary<IParticipant, int>();
            TimesBrokenDown = new Dictionary<IParticipant, int>();
            Tracklist = new List<string>();
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