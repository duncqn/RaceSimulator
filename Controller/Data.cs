using System;
using Model;

namespace Controller
{
    public static class Data
    {
        public static Competition Competition { get; set; }

        public static Race CurrentRace;

        public static void AddParticipants()
        {
            Competition.Participants.Add(new Driver("Duncan", 0, new Car(0, 0, 0, false), IParticipant.TeamColors.Blue));
            Competition.Participants.Add(new Driver("Noa", 0, new Car(0, 0, 0, false), IParticipant.TeamColors.Red));
            Competition.Participants.Add(new Driver("Michiel", 0, new Car(0, 0, 0, false), IParticipant.TeamColors.Green));
            Competition.Participants.Add(new Driver("Damian", 0, new Car(0, 0, 0, false), IParticipant.TeamColors.Yellow));
        }

        public static void AddTracks()
        {
            //Initialize tracks
            var Dronten = new Track("Dronten", new Section.SectionTypes[]
                {
                    Section.SectionTypes.StartGrid,
                    Section.SectionTypes.StartGrid,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.RightCorner,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.RightCorner,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.RightCorner,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.RightCorner,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Finish,


                });
            var Recht = new Track("Recht", new Section.SectionTypes[]
                {
                    Section.SectionTypes.StartGrid,
                    Section.SectionTypes.StartGrid,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Straight,
                    Section.SectionTypes.Finish,

                });

            Competition.Tracks.Enqueue(Dronten);
            Competition.Tracks.Enqueue(Recht);
        }


        public static Race NextRace()
        {
            Track track = Competition.NextTrack();

            if (track == null)
            {
                return null;
            }
            return CurrentRace = new Race(track, Competition.Participants);
        }

        public static void Initialize()
        {
            Competition = new Competition();
            AddParticipants();
            AddTracks();
            NextRace();
        }
    }
}