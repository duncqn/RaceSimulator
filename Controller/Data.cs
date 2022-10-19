using Model;
using System;

namespace Controller
{
    public static class Data
    {
        public static Competition Competition { get; private set; }
        public static Race CurrentRace { get; private set; }

        public static event EventHandler<NextRaceEventArgs> NextRaceEvent;

        public static void Initialize()
        {
            Competition = new Competition();
            AddParticipants();
            AddTracks();
        }

        private static void AddParticipants()
        {
            Competition.Participants.Add(new Driver("Duncan", 0, new Car(19, 10, 20, false), IParticipant.TeamColors.Red));
            Competition.Participants.Add(new Driver("Noa", 0, new Car(18, 10, 19, false), IParticipant.TeamColors.Green));
            Competition.Participants.Add(new Driver("Michiel", 0, new Car(16, 10, 17, false), IParticipant.TeamColors.Blue));
            Competition.Participants.Add(new Driver("Damian", 0, new Car(20, 10, 14, false), IParticipant.TeamColors.Yellow));
        }

        private static void AddTracks()
        {
            Competition.Tracks.Enqueue(new Track("Ovaal", new[]
            {
                SectionTypes.StartGrid,
                SectionTypes.StartGrid,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.RightCorner,
                SectionTypes.Straight,
                SectionTypes.RightCorner,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.RightCorner,
                SectionTypes.Straight,
                SectionTypes.RightCorner,
                SectionTypes.Straight,
                SectionTypes.Finish,
            }));
            
            Competition.Tracks.Enqueue(new Track("Recht", new[]
            {
                SectionTypes.StartGrid,
                SectionTypes.StartGrid,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Straight,
                SectionTypes.Finish,
            }));
        }

        public static void NextRace()
        {
            CurrentRace?.CleanUp();
            Track currentTrack = Competition.NextTrack();
            if (currentTrack != null)
            {
                CurrentRace = new Race(currentTrack, Competition.Participants);
                NextRaceEvent?.Invoke(null, new NextRaceEventArgs() { Race = CurrentRace });
                CurrentRace.Start();
            }
        }
    }
}
