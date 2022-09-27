using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Timer = System.Timers.Timer;

namespace Controller
{
    public class Race
    {
        public Track Track { get; set; }
        public List<IParticipant> Participants { get; set; }
        public DateTime StartTime { get; set; }
        public List<Section> startGridList = new List<Section>();
        private Random _random;
        private Timer timer;

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

            //Set timer values
            timer = new Timer(500);
            timer.Elapsed += OnTimedEvent;

            GetStartGrids(Track);
            AddParticipantToStartGrid(Participants);
            Start();
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

        public void AddParticipantToStartGrid(List<IParticipant> participants)
        {
            int partCount = 0;

            for (int i = 0; i < startGridList.Count; i++)
            {
                while (partCount < participants.Count)
                {
                    foreach (var startGrid in startGridList)
                    {
                        if (startGrid.Left == null)
                        {
                            startGrid.Left = participants[partCount];
                            partCount++;
                        }
                        if (startGrid.Right == null)
                        {
                            startGrid.Right = participants[partCount];
                            partCount++;
                        }
                        else
                        {
                            i++;
                        }
                    }
                }
            }
        }


        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            var currentSection = Track.Sections.First;

            for (int i = 0; i < Track.Sections.Count; i++)
            {
                if (GetSectionData(currentSection.Value).Left != null)
                {

                    GetSectionData(currentSection.Value).DistanceLeft = GetSectionData(currentSection.Value).DistanceLeft - calculateDistanceForParticipant(GetSectionData(currentSection.Value).Left);

                    if (GetSectionData(currentSection.Value).DistanceLeft < 0 && GetSectionData(currentSection.Next.Value).Left == null)
                    {
                        GetSectionData(currentSection.Next.Value).Left = GetSectionData(currentSection.Value).Left;
                        GetSectionData(currentSection.Value).Left = null;

                    }
                }
                DriversChanged?.Invoke(this, new DriversChangedEventArgs(Track));
                currentSection = currentSection.Next;
            }
        }

        public int calculateDistanceForParticipant(IParticipant driver)
        {
            return driver.Equipment.Performance / driver.Equipment.Quality * driver.Equipment.Speed;
        }

        public void Start()
        {
            timer.Enabled = true;
        }

    }
}