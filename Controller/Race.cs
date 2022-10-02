using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
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
        public Track Track { get; set; }
        public List<IParticipant> Participants { get; }
        public DateTime StartTime { get; set; }

        private readonly Random _random;
        internal readonly Dictionary<Section, SectionData> Positions;
        private readonly Timer _timer;
        private const int TimerInterval = 200;

        // keeping track of laps
        internal const int Laps = 3;

        private Dictionary<IParticipant, int> _lapsDriven;

        // list for finish order
        private readonly List<IParticipant> _finishOrder;

        // section times

        // lap times
        private Dictionary<IParticipant, DateTime> _participantTimeEachLap;
        

        // Race Length
        private DateTime _endTime;

        // event for drivers changed positions
        public event EventHandler<DriversChangedEventArgs> DriversChanged;

        // event for race finish
        public event EventHandler RaceFinished;

        public SectionData GetSectionData(Section section)
        {
            // look for key section in dictionary, if exists, return sectiondata for key section.
            // otherwise, create SectionData for section and return that object.
            if (!Positions.ContainsKey(section)) Positions.Add(section, new SectionData());
            return Positions[section];
        }

        public Race(Track track, List<IParticipant> participants)
        {
            Track = track;
            Participants = participants;
            _random = new Random(DateTime.Now.Millisecond);
            Positions = new Dictionary<Section, SectionData>();
            _finishOrder = new List<IParticipant>();

            _timer = new Timer(TimerInterval);
            _timer.Elapsed += OnTimedEvent;

            PlaceParticipantsOnStartGrid();
            InitializeParticipantLaps();
            RandomizeEquipment();
        }

        private void InitializeParticipantLaps()
        {
            _lapsDriven = new Dictionary<IParticipant, int>();
            // fill dictionary
            foreach (IParticipant participant in Participants)
            {
                _lapsDriven.Add(participant, -1); // participants start before the finish line, so the first time they drive over the finish, they're at lap 0
            }
        }

        internal void InitializeParticipantTimeEachLap()
        {
            _participantTimeEachLap = new Dictionary<IParticipant, DateTime>();
            foreach (IParticipant participant in Participants)
            {
                _participantTimeEachLap.Add(participant, StartTime); // first time element is starttime.
            }
        }

        public void RandomizeEquipment()
        {
            foreach (IParticipant participant in Participants)
            {
                participant.Equipment.Performance = _random.Next(5, 16); //  5 <= performance <= 15
                participant.Equipment.Quality = _random.Next(8, 21); // quality can be 1-20, but i don't generate really awful equipment
            }
        }

        public void PlaceParticipantsOnStartGrid()
        {
            // create List of startgrids, from front to back
            List<Section> startGrids = GetStartGrids();

            // look at amount of participants and amount of start places.
            int amountToPlace = 0;
            if (Participants.Count >= startGrids.Count * 2)
                amountToPlace = startGrids.Count * 2;
            else if (Participants.Count < startGrids.Count * 2)
                amountToPlace = Participants.Count;

            bool side = false; // false is left, true is right
            int currentStartGridIndex = 0;
            for (int i = 0; i < amountToPlace; i++)
            {
                // place
                PlaceParticipant(Participants[i], side, startGrids[currentStartGridIndex]);
                // flip side
                side = !side;
                // up section index on every uneven number for i
                if (i % 2 == 1)
                    currentStartGridIndex++;
            }
        }

        public List<Section> GetStartGrids()
        {
            // put all sections in list that have sectiontype StartGrid
            List<Section> startGridSections = Track.Sections.Where(trackSection => trackSection.SectionType == SectionTypes.StartGrid).ToList();

            // reverse list
            startGridSections.Reverse();

            return startGridSections;
        }

        public void PlaceParticipant(IParticipant p, bool side, Section section)
        {
            if (side)
                GetSectionData(section).Right = p;
            else
                GetSectionData(section).Left = p;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            // fix broken drivers at random
            RandomizeEquipmentFixing();
            // randomize IsBroken for pariticpants
            RandomEquipmentBreaking();

            // call method to change driverPositions.
            MoveParticipants(e.SignalTime);

            // raise driversChanged event.
            DriversChanged?.Invoke(this, new DriversChangedEventArgs() { Track = this.Track });

            // check if any participants on track, if not, race is finished.
            if (CheckRaceFinished())
            {
                _endTime = e.SignalTime;
                RaceFinished?.Invoke(this, new EventArgs());
            }
        }

        private void RandomEquipmentBreaking()
        {
            // quality of a participant is 1 to 10; meaning a 0.1 to 0.01% chance of breaking.
            List<IParticipant> participantsOnTrack =
                Positions.Values.Where(a => a.Left != null).Select(a => a.Left).Concat(Positions.Values.Where(a => a.Right != null).Select(a => a.Right)).ToList();
            foreach (IParticipant participant in participantsOnTrack)
            {
                double qualityChance = (11 - (participant.Equipment.Quality * 0.5)) * 0.0005;
                if (_random.NextDouble() < qualityChance)
                {
                    participant.Equipment.IsBroken = true;
                }
            }
        }

        private void RandomizeEquipmentFixing()
        {
            foreach (IParticipant participant in Participants.Where(p => p.Equipment.IsBroken))
            {
                // chance is 6% of being fixed.
                if (_random.NextDouble() < 0.06)
                {
                    participant.Equipment.IsBroken = false;
                    // downgrade quality of equipment by 1, assure proper bounds
                    if (participant.Equipment.Quality > 1)
                        participant.Equipment.Quality--;
                    // downgrade base speed of equipment by 1, assure proper bounds;
                    if (participant.Equipment.Speed > 5)
                        participant.Equipment.Speed--;
                }
            }
        }

        private void MoveParticipants(DateTime elapsedDateTime)
        {
            // Steps to take:
            // Traverse sections list from end to beginning. (this is so when a driver moves enough, a positions opens up for the driver behind
            LinkedListNode<Section> currentSectionNode = Track.Sections.Last;

            while (currentSectionNode != null)
            {
                // get sectiondata variables for currentSectionData and nextSectionData section
                MoveParticipantsSectionData(currentSectionNode.Value, currentSectionNode.Next != null ? currentSectionNode.Next.Value : Track.Sections.First?.Value, elapsedDateTime);

                // loop iterator
                currentSectionNode = currentSectionNode.Previous;
            }
        }

        private void MoveParticipantsSectionData(Section currentSection, Section nextSection, DateTime elapsedDateTime)
        {
            SectionData currentSectionData = GetSectionData(currentSection);
            SectionData nextSectionData = GetSectionData(nextSection);
            // first check participants on currentsectiondata and update distance.
            // only up distance when participant isn't broken.
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
                #region Both drivers ready to move

                int freePlaces = FreePlacesLeftOnSectionData(nextSectionData);
                if (freePlaces == 0)
                {
                    // no places available
                    currentSectionData.DistanceRight = 99;
                    currentSectionData.DistanceLeft = 99;
                }
                else if (freePlaces == 3)
                {
                    // move both
                    MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Left, false, elapsedDateTime);
                    MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Right, false, elapsedDateTime);
                }
                else
                {
                    // TODO: handle edge cases a little better when a choice must be made
                    if (currentSectionData.DistanceLeft >= currentSectionData.DistanceRight)
                    {
                        // prefer left
                        if (freePlaces == 1)
                            MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Left, true, elapsedDateTime); // left to left
                        else if (freePlaces == 2)
                            MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Right, true, elapsedDateTime);
                    }
                    else
                    {
                        // choose right
                        if (freePlaces == 1)
                            MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Left, true, elapsedDateTime); // left to left
                        else if (freePlaces == 2)
                            MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Right, true, elapsedDateTime);
                    }
                }

                #endregion Both drivers ready to move
            }
            else if (currentSectionData.DistanceLeft >= 100)
            {
                #region Left driver ready to move

                // for freesections, prefer same spot, otherwise take other
                int freePlaces = FreePlacesLeftOnSectionData(nextSectionData);
                if (freePlaces == 0)
                    currentSectionData.DistanceLeft = 99;
                else if (freePlaces == 3 || freePlaces == 1)
                    // move from left to left
                    MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Left, false, elapsedDateTime);
                else if (freePlaces == 2)
                    // move from left to right
                    MoveSingleParticipant(currentSection, nextSection, Side.Left, Side.Right, false, elapsedDateTime);

                #endregion Left driver ready to move
            }
            else if (currentSectionData.DistanceRight >= 100)
            {
                #region Right driver ready to move

                // for freesections, prefer same spot, otherwise take other
                int freePlaces = FreePlacesLeftOnSectionData(nextSectionData);
                if (freePlaces == 0)
                    currentSectionData.DistanceRight = 99;
                else if (freePlaces == 3 || freePlaces == 2)
                    // move from right to right
                    MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Right, false, elapsedDateTime);
                else if (freePlaces == 1)
                    // move from right to left
                    MoveSingleParticipant(currentSection, nextSection, Side.Right, Side.Left, false, elapsedDateTime);

                #endregion Right driver ready to move
            }
        }

        public int GetSpeedFromParticipant(IParticipant iParticipant) => Convert.ToInt32(Math.Ceiling(0.1 * (iParticipant.Equipment.Speed * 0.5) * iParticipant.Equipment.Performance + 18));

        internal int FreePlacesLeftOnSectionData(SectionData sd)
        {
            // values possible:
            // 0: no places possible
            // 1: left free
            // 2: right free
            // 3: both free
            int returnValue = 0;
            if (sd.Left == null)
                returnValue += 1;
            if (sd.Right == null)
                returnValue += 2;
            return returnValue;
        }

        private void MoveSingleParticipant(Section currentSection, Section nextSection, Side start, Side end, bool correctOtherSide, DateTime elapsedDateTime)
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
                            if (IsFinishSection(nextSection))
                                OnMoveUpdateLapsAndFinish(nextSectionData, Side.Right, elapsedDateTime);
                            break;

                        case Side.Left:
                            nextSectionData.Left = currentSectionData.Right;
                            nextSectionData.DistanceLeft = currentSectionData.DistanceRight - 100;
                            if (IsFinishSection(nextSection))
                                OnMoveUpdateLapsAndFinish(nextSectionData, Side.Left, elapsedDateTime);
                            break;
                    }
                    // section time
                    currentSectionData.Right = null;
                    currentSectionData.DistanceRight = 0;
                    break;

                case Side.Left:
                    switch (end)
                    {
                        case Side.Right:
                            nextSectionData.Right = currentSectionData.Left;
                            nextSectionData.DistanceRight = currentSectionData.DistanceLeft - 100;
                            if (IsFinishSection(nextSection))
                                OnMoveUpdateLapsAndFinish(nextSectionData, Side.Right, elapsedDateTime);
                            break;

                        case Side.Left:
                            nextSectionData.Left = currentSectionData.Left;
                            nextSectionData.DistanceLeft = currentSectionData.DistanceLeft - 100;
                            if (IsFinishSection(nextSection))
                                OnMoveUpdateLapsAndFinish(nextSectionData, Side.Left, elapsedDateTime);
                            break;
                    }
                    // section time
                    currentSectionData.Left = null;
                    currentSectionData.DistanceLeft = 0;
                    break;
            }

            // correct other driver's distance if required. (this is needed when only one of two drivers can move)
            if (start == Side.Right && correctOtherSide)
                currentSectionData.DistanceLeft = 99;
            else if (start == Side.Left && correctOtherSide)
                currentSectionData.DistanceRight = 99;
        }

        public int GetDistanceParticipant(IParticipant participant)
        {
            SectionData partL = Positions.Values.FirstOrDefault(part => part.Left == participant);
            SectionData partR = Positions.Values.FirstOrDefault(part => part.Right == participant);

            if (partL != null)
                return partL.DistanceLeft;
            if (partR != null)
                return partR.DistanceRight;
            return 0;
        }

        public int GetLapsParticipant(IParticipant participant) => _lapsDriven[participant];

        internal void UpdateLap(IParticipant participant, DateTime elapsedDateTime)
        {
            _lapsDriven[participant]++;
            // write lap time, update time
            if (_lapsDriven[participant] <= 0) return;
            _participantTimeEachLap[participant] = elapsedDateTime;
        }

        internal bool IsFinished(IParticipant participant) => _lapsDriven[participant] >= Laps;

        internal bool IsFinishSection(Section section)
        {
            return section.SectionType == SectionTypes.Finish;
        }

        private void OnMoveUpdateLapsAndFinish(SectionData sectionData, Side side, DateTime elapsedDateTime)
        {
            if (side == Side.Right)
            {
                UpdateLap(sectionData.Right, elapsedDateTime);
                if (!IsFinished(sectionData.Right)) return;
                _finishOrder.Add(sectionData.Right);
                sectionData.Right = null;
            }
            else if (side == Side.Left)
            {
                UpdateLap(sectionData.Left, elapsedDateTime);
                if (!IsFinished(sectionData.Left)) return;
                _finishOrder.Add(sectionData.Left);
                sectionData.Left = null;
            }
        }

        internal bool CheckRaceFinished() => Positions.Values.FirstOrDefault(a => a.Left != null || a.Right != null) == null;

        public List<IParticipant> GetFinishOrderParticipants() => _finishOrder;

        public TimeSpan GetRaceLength() => _endTime - StartTime;

        public void Start()
        {
            StartTime = DateTime.Now;
            InitializeParticipantTimeEachLap();
            _timer.Start();
        }

        public void CleanUp()
        {
            DriversChanged = null;
            _timer.Stop();
            RaceFinished = null;
        }
    }
}