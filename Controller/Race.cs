using System.Diagnostics;
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
        public DateTime StartTime { get; private set; }
        public Dictionary<IParticipant, int> LapsCompleted { get; }
        private Dictionary<Section, SectionData> Positions { get; }
        public List<IParticipant> Participants { get; }

        private readonly Random _random;
        private readonly Timer _timer;
        
        private const int TimerInterval = 500;
        private const int Laps = 2; //aantal laps -1 omdat het bij 0 begint

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
            if (participants.Count < 3)
            {
                throw new ArgumentOutOfRangeException(nameof(participants), "There must be a least 3 participants to start a race.");
            }
            
            Track = track;
            Participants = participants;
            _random = new Random(DateTime.Now.Millisecond);
            Positions = new Dictionary<Section, SectionData>();
            LapsCompleted = new Dictionary<IParticipant, int>();

            AddItemsToDictionairy(participants);

            _timer = new Timer(TimerInterval);
            _timer.Elapsed += OnTimedEvent;

            bool side = false;
            int startGridIndex = 0;
            int participantsToPlace = 0;

            List<Section> startGridSections = Track.Sections.Where(trackSection => trackSection.SectionType == SectionTypes.StartGrid).ToList();
            startGridSections.Reverse();
            List<Section> startGrid = startGridSections;

            try
            {
                if (Participants.Count <= startGrid.Count * 2)
                {
                    participantsToPlace = startGrid.Count * 2;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(participants), "Track does not have enough start places.");
                }
                
                for (int i = 0; i < participantsToPlace; i++)
                {
                    PlaceParticipantOnTrack(Participants[i], side, startGrid[startGridIndex]);
                    side = !side;
                    if (i % 2 == 1)
                        startGridIndex++;
                }
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e);
            }

            foreach (IParticipant participant in Participants)
            {
                participant.Equipment.Quality = _random.Next(6, 17);
                participant.Equipment.Performance = _random.Next(8, 20);
            }
            
            foreach (IParticipant participant in Participants)
            {
                LapsCompleted.Add(participant, -1);
            }
        }

        private void AddItemsToDictionairy(List<IParticipant> participants)
        {
            foreach (IParticipant p in participants)
            {
                Data.Competition.TimesBrokenDown.Add(p, 0);
                Data.Competition.Speed.Add(p,0);
                
            }
        }

        private void PlaceParticipantOnTrack(IParticipant p, bool side, Section section)
        {
            if (side)
                GetSectionData(section).Right = p;
            else
                GetSectionData(section).Left = p;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
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

        private void RandomizeEquipmentFixing()
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
                double qualityChance = (11 - (participant.Equipment.Quality * 0.5)) * 0.005;
                if (_random.NextDouble() < qualityChance)
                {
                    participant.Equipment.IsBroken = true;
                    Data.Competition.TimesBrokenDown[participant]++;
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

        private int GetSpeedFromParticipant(IParticipant iParticipant)
        {
            var speed = Convert.ToInt32(
                Math.Ceiling(0.4 * (iParticipant.Equipment.Speed * 0.5) * iParticipant.Equipment.Performance + 18));
            Data.Competition.Speed[iParticipant] = speed;
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
        
        private void UpdateLap(IParticipant participant)
        {
            LapsCompleted[participant]++;
        }

        private bool IsFinished(IParticipant participant) => LapsCompleted[participant] >= Laps;

        private bool CheckIfFinishSection(Section section)
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

        private bool CheckRaceFinished() => Positions.Values.FirstOrDefault(a => a.Left != null || a.Right != null) == null;
        
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