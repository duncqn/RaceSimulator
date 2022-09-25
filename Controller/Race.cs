using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace Controller
{
    public class Race
    {
        public Track Track { get; set; }
        public List<IParticipant> Participants { get; set; }
        public DateTime StartTime { get; set; }
        public List<Section> startGridList = new List<Section>();
        private Random _random;

        public delegate void onDriversChanged(object Sender, DriversChangedEventArgs dirversChangedEventArgs);

        public event onDriversChanged DriversChanged;

        private Dictionary<Section, SectionData> _positions;

        public SectionData GetSectionData(Section section)
        {
            if (!_positions.ContainsKey(section)) _positions.Add(section, new SectionData());

            return _positions[section];
        }

        public Race(Track track, List<IParticipant> participant)
        {
            Track = track;
            Participants = participant;
            StartTime = DateTime.Now;
            _random = new Random();
            _positions = new Dictionary<Section, SectionData>();

            GetStartGrids(Track);
            AddParticipantToRace(Participants);
        }

        public void RandomizeEquipment()
        {
            foreach (var participant in Participants)
            {
                participant.Equipment.Quality = _random.Next(1, 100);
                participant.Equipment.Speed = _random.Next(1, 100);
            }
        }

        public List<Section> GetStartGrids(Track track)
        {
            foreach (var section in track.Sections.Where(section => section.SectionType == Section.SectionTypes.StartGrid))
            {
                startGridList.Add(section);
            }
            startGridList.Reverse();

            return startGridList;
        }

        public void AddParticipantToRace(List<IParticipant> participants)
        {
            int driver = 0;

            for (int i = 0; i < startGridList.Count; i++)
            {
                while (driver < participants.Count)
                {
                    foreach (var startGrid in startGridList)
                    {
                        if (startGrid.Left == null)
                        {
                            startGrid.Left = participants[driver];
                            driver++;
                        }
                        if (startGrid.Right == null)
                        {
                            startGrid.Right = participants[driver];
                            driver++;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
        }
    }
}