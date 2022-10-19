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
        internal readonly Dictionary<Section, SectionData> Positions;
        private Dictionary<IParticipant, int> _lapsCompleted;
        private readonly Random _random;
        private readonly Timer _timer;
        private const int TimerInterval = 150;
        internal const int Laps = 3;
        private readonly List<IParticipant> _finishOrder;
        private Dictionary<IParticipant, DateTime> _participantTimeEachLap;
        private DateTime _endTime;

        public event EventHandler<DriversChangedEventArgs> DriversChanged;
        public event EventHandler RaceFinished;

        public SectionData GetSectionData(Section section)
        {
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

            List<Section> startGridSections = Track.Sections.Where(trackSection => trackSection.SectionType == SectionTypes.StartGrid).ToList();
            startGridSections.Reverse();
            List<Section> startGrid = startGridSections;
            
            
            //deelnemers plaatsen op de baan.
            int participantsToPlace = 0;
            if (Participants.Count >= startGrid.Count * 2)
                participantsToPlace = startGrid.Count * 2;
            else if (Participants.Count < startGrid.Count * 2)
                participantsToPlace = Participants.Count;

            bool side = false;
            int StartGridIndex = 0;
            for (int i = 0; i < participantsToPlace; i++)
            {
                PlaceParticipant(Participants[i], side, startGrid[StartGridIndex]);
                side = !side;
                if (i % 2 == 1)
                    StartGridIndex++;
            }
            
            //Equipment random maken voor de deelnemers
            foreach (IParticipant participant in Participants)
            {
                participant.Equipment.Quality = _random.Next(6, 17);
                participant.Equipment.Performance = _random.Next(8, 20);
            }
            
            //rondes voor de deelnemers inladen.
            _lapsCompleted = new Dictionary<IParticipant, int>();
            // fill dictionary
            foreach (IParticipant participant in Participants)
            {
                _lapsCompleted.Add(participant, -1);
            }
            
            
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
            foreach (IParticipant participant in Participants.Where(p => p.Equipment.IsBroken))
            {
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

            List<IParticipant> participantsOnTrack = Positions.Values.Where(a => a.Left != null).Select(a => a.Left).Concat(Positions.Values.Where(a => a.Right != null).Select(a => a.Right)).ToList();
            foreach (IParticipant participant in participantsOnTrack)
            {
                double qualityChance = (11 - (participant.Equipment.Quality * 0.5)) * 0.0005;
                if (_random.NextDouble() < qualityChance)
                {
                    participant.Equipment.IsBroken = true;
                }
            }

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

        public int GetLapsParticipant(IParticipant participant) => _lapsCompleted[participant];

        internal void UpdateLap(IParticipant participant, DateTime elapsedDateTime)
        {
            _lapsCompleted[participant]++;
            // write lap time, update time
            if (_lapsCompleted[participant] <= 0) return;
            
            _participantTimeEachLap[participant] = elapsedDateTime;
        }

        internal bool IsFinished(IParticipant participant) => _lapsCompleted[participant] >= Laps;

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
            
            _participantTimeEachLap = new Dictionary<IParticipant, DateTime>();
            foreach (IParticipant participant in Participants)
            {
                _participantTimeEachLap.Add(participant, StartTime); // first time element is starttime.
            }
            
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