using Model;
using System.Timers;
using System.Xml;
using Timer = System.Timers.Timer;

namespace Controller
{
    internal enum Side
    {
        Left,
        Right
    }

    public class Race
    {
        public Track Track { get;}
        public DateTime StartTime { get; set; }
        public List<IParticipant> Participants { get; }
        public readonly Dictionary<Section, SectionData> Positions;
        public Dictionary<IParticipant, int> _lapsCompleted;
        
        public readonly Random _random;
        public readonly Timer _timer;
        
        private const int TimerInterval = 500;
        internal const int Laps = 2; //aantal laps -1 omdat het bij 0 begint

        public event EventHandler<DriversChangedEventArgs> DriversChanged;
        public event EventHandler RaceFinished;

        public SectionData GetSectionData(Section section)
        {
            if (Positions.ContainsKey(section))
            {
                return Positions[section];
            } else
                Positions.Add(section, new SectionData());
            return Positions[section];
        }

        public Race(Track track, List<IParticipant> participants)
        {
            Track = track;
            Participants = participants;
            _random = new Random(DateTime.Now.Millisecond);
            Positions = new Dictionary<Section, SectionData>();
            _lapsCompleted = new Dictionary<IParticipant, int>();

            AddItemsToDictionairy(participants);

            _timer = new Timer(TimerInterval);
            _timer.Elapsed += OnTimedEvent;

            bool side = false;
            int StartGridIndex = 0;
            int participantsToPlace = 0;

            List<Section> startGridSections = Track.Sections.Where(trackSection => trackSection.SectionType == SectionTypes.StartGrid).ToList();
            startGridSections.Reverse();
            List<Section> startGrid = startGridSections;
            
            if (Participants.Count >= startGrid.Count * 2)
                participantsToPlace = startGrid.Count * 2;
            else if (Participants.Count < startGrid.Count * 2)
                participantsToPlace = Participants.Count;
            
            for (int i = 0; i < participantsToPlace; i++)
            {
                PlaceParticipantOnTrack(Participants[i], side, startGrid[StartGridIndex]);
                side = !side;
                if (i % 2 == 1)
                    StartGridIndex++;
            }

            foreach (IParticipant participant in Participants)
            {
                participant.Equipment.Quality = _random.Next(6, 17);
                participant.Equipment.Performance = _random.Next(8, 20);
            }
            
            foreach (IParticipant participant in Participants)
            {
                _lapsCompleted.Add(participant, -1);
            }
        }

        public void AddItemsToDictionairy(List<IParticipant> participants)
        {
            foreach (IParticipant p in participants)
            {
                Data.Competition.timesBrokenDown.Add(p, 0);
                Data.Competition.speed.Add(p,0);
                
            }
        }

        public void PlaceParticipantOnTrack(IParticipant p, bool side, Section section)
        {
            if (side)
                GetSectionData(section).Right = p;
            else
                GetSectionData(section).Left = p;
        }

        public void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            RandomizeEquipmentFixing();
            RandomEquipmentBreaking();
            MoveAllParticipants();
            
            DriversChanged?.Invoke(this, new DriversChangedEventArgs(Data.CurrentRace.Track));

            if (CheckRaceFinished())
            {
                RaceFinished?.Invoke(this, new EventArgs());
            }
        }

        public void RandomizeEquipmentFixing()
        {
            foreach (IParticipant participant in Participants.Where(p => p.Equipment.IsBroken))
            {
                if (_random.NextDouble() < 0.05)
                {
                    participant.Equipment.IsBroken = false;
                    if (participant.Equipment.Quality > 1)
                        participant.Equipment.Quality--;
                    if (participant.Equipment.Speed > 5)
                        participant.Equipment.Speed--;
                }
            }
        }

        public void RandomEquipmentBreaking()
        {
            List<IParticipant> participantsOnTrack =
                Positions.Values.Where(a => a.Left != null).Select(a => a.Left).Concat(Positions.Values.Where(a => a.Right != null).Select(a => a.Right)).ToList();
            foreach (IParticipant participant in participantsOnTrack)
            {
                double qualityChance = (11 - (participant.Equipment.Quality * 0.5)) * 0.0005;
                if (_random.NextDouble() < qualityChance)
                {
                    participant.Equipment.IsBroken = true;
                    Data.Competition.timesBrokenDown[participant]++;
                }
            }
        }


        private void MoveAllParticipants()
        {
            LinkedListNode<Section> currentSectionNode = Track.Sections.Last;

            while (currentSectionNode != null)
            {
                MoveParticipantsSectionData(currentSectionNode.Value, currentSectionNode.Next != null ? currentSectionNode.Next.Value : Track.Sections.First?.Value);
                currentSectionNode = currentSectionNode.Previous;
            }
        }

        private void MoveParticipantsSectionData(Section currentSection, Section nextSection)
        {
            SectionData currentSectionData = GetSectionData(currentSection);
            SectionData nextSectionData = GetSectionData(nextSection);
            if (currentSectionData.Left != null && !currentSectionData.Left.Equipment.IsBroken)
            {
                currentSectionData.DistanceLeft += GetSpeedFromParticipant(currentSectionData.Left);
            }

            if (currentSectionData.Right != null && !currentSectionData.Right.Equipment.IsBroken)
            {
                currentSectionData.DistanceRight += GetSpeedFromParticipant(currentSectionData.Right);
            }

            if (currentSectionData.DistanceLeft >= 100 && currentSectionData.DistanceRight >= 100)
            {
                int freePlaces = 0;
                if (nextSectionData.Left == null)
                    freePlaces += 1;
                if (nextSectionData.Right == null)
                    freePlaces += 2;
                if (freePlaces == 0)
                {
                    currentSectionData.DistanceRight = 99;
                    currentSectionData.DistanceLeft = 99;
                }
                else if (freePlaces == 3)
                {
                    MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Left, false);
                    MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Right, false);
                }
                else
                {
                    if (currentSectionData.DistanceLeft >= currentSectionData.DistanceRight)
                    {
                        if (freePlaces == 1)
                            MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Left, true); // left to left
                        else if (freePlaces == 2)
                            MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Right, true);
                    }
                    else
                    {
                        if (freePlaces == 1)
                            MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Left, true); // left to left
                        else if (freePlaces == 2)
                            MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Right, true);
                    }
                }
            }
            else if (currentSectionData.DistanceLeft >= 100)
            {
                int freePlaces = 0;
                if (nextSectionData.Left == null)
                    freePlaces += 1;
                if (nextSectionData.Right == null)
                    freePlaces += 2;
                if (freePlaces == 0)
                    currentSectionData.DistanceLeft = 99;
                else if (freePlaces == 3 || freePlaces == 1)
                    MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Left, false);
                else if (freePlaces == 2)
                    MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Right, false);
            }
            else if (currentSectionData.DistanceRight >= 100)
            {
                int freePlaces = 0;
                if (nextSectionData.Left == null)
                    freePlaces += 1;
                if (nextSectionData.Right == null)
                    freePlaces += 2;
                if (freePlaces == 0)
                    currentSectionData.DistanceRight = 99;
                else if (freePlaces == 3 || freePlaces == 2)
                    MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Right, false);
                else if (freePlaces == 1)
                    MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Left, false);
            }
        }

        public int GetSpeedFromParticipant(IParticipant iParticipant)
        {
            var speed = Convert.ToInt32(
                Math.Ceiling(0.4 * (iParticipant.Equipment.Speed * 0.5) * iParticipant.Equipment.Performance + 18));
            Data.Competition.speed[iParticipant] = speed;
            return speed;
        }

        private void MoveSingleParticipant(Section currentSection, Section nextSection, Side start, Side end, bool correctOtherSide)
        {
            SectionData currentSectionData = GetSectionData(currentSection);
            SectionData nextSectionData = GetSectionData(nextSection);
            switch (start)
            {
                case Side.Right:
                    switch (end)
                    {
                        case Side.Right:
                            nextSectionData.Right = currentSectionData.Right;
                            nextSectionData.DistanceRight = currentSectionData.DistanceRight - 100;
                            if (CheckIfFinishSection(nextSection))
                                OnMoveUpdateLapsAndFinish(nextSectionData, Side.Right);
                            break;

                        case Side.Left:
                            nextSectionData.Left = currentSectionData.Right;
                            nextSectionData.DistanceLeft = currentSectionData.DistanceRight - 100;
                            if (CheckIfFinishSection(nextSection))
                                OnMoveUpdateLapsAndFinish(nextSectionData, Side.Left);
                            break;
                    }

                    currentSectionData.Right = null;
                    currentSectionData.DistanceRight = 0;
                    break;

                case Side.Left:
                    switch (end)
                    {
                        case Side.Right:
                            nextSectionData.Right = currentSectionData.Left;
                            nextSectionData.DistanceRight = currentSectionData.DistanceLeft - 100;
                            if (CheckIfFinishSection(nextSection))
                                OnMoveUpdateLapsAndFinish(nextSectionData, Side.Right);
                            break;

                        case Side.Left:
                            nextSectionData.Left = currentSectionData.Left;
                            nextSectionData.DistanceLeft = currentSectionData.DistanceLeft - 100;
                            if (CheckIfFinishSection(nextSection))
                                OnMoveUpdateLapsAndFinish(nextSectionData, Side.Left);
                            break;
                    }
                    
                    currentSectionData.Left = null;
                    currentSectionData.DistanceLeft = 0;
                    break;
            }
            
            if (start == Side.Right && correctOtherSide)
                currentSectionData.DistanceLeft = 99;
            else if (start == Side.Left && correctOtherSide)
                currentSectionData.DistanceRight = 99;
        }
        
        public void UpdateLap(IParticipant participant)
        {
            _lapsCompleted[participant]++;
        }

        public bool IsFinished(IParticipant participant) => _lapsCompleted[participant] >= Laps;

        public bool CheckIfFinishSection(Section section)
        {
            return section.SectionType == SectionTypes.Finish;
        }

        private void OnMoveUpdateLapsAndFinish(SectionData sectionData, Side side)
        {
            if (side == Side.Right)
            {
                UpdateLap(sectionData.Right);
                if (!IsFinished(sectionData.Right)) return;
                sectionData.Right = null;
            }
            else if (side == Side.Left)
            {
                UpdateLap(sectionData.Left);
                if (!IsFinished(sectionData.Left)) return;
                sectionData.Left = null;
            }
        }

        public bool CheckRaceFinished() => Positions.Values.FirstOrDefault(a => a.Left != null || a.Right != null) == null;
        
        public void Start()
        {
            _timer.Start();
            StartTime = DateTime.Now;
        }

        public void CleanUp()
        {
            DriversChanged = null;
            _timer.Stop();
            RaceFinished = null;
        }
    }
}