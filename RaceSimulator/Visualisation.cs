using Controller;
using Model;

namespace ConsoleEdition
{
    public enum Direction
    {
        N,
        E,
        S,
        W
    }

    public static class Visualisation
    {
        private const int CursorStartPosX = 24;
        private const int CursorStartPosY = 16;

        private static int _cPosX;
        private static int _cPosY;

        private static Race _currentRace;
        private static Direction _currentDirection;

        #region graphics

        private static readonly string[] _horizontalFinish = { "xxxx", " 1# ", "2 # ", "xxxx" };
        private static readonly string[] _horizontalStartGrid = { "xxxx", " 1] ", "2]  ", "xxxx" };
        private static readonly string[] _cornerNe = { @" /xx", @"/1  ", @"x 2 ", @"x  /" };
        private static readonly string[] _cornerNw = { @"xx\ ", @"  1\", @" 2 x", @"\  x" };
        private static readonly string[] _cornerSe = { @"x  \", @"x 1 ", @"\2  ", @" \xx" };
        private static readonly string[] _cornerSw = { @"/  x", @" 1 x", @"  2/", @"xx/ " };
        private static readonly string[] _horizontalStraight = { "xxxx", "  1 ", " 2  ", "xxxx" };
        private static readonly string[] _verticalStraight = { "x  x", "x2 x", "x 1x", "x  x" };

        #endregion graphics

        internal static string[] SectionTypeToGraphic(SectionTypes sectionType, Direction direction)
        {
            return sectionType switch
            {
                SectionTypes.Straight => ((int)direction % 2) switch
                {
                    0 => _verticalStraight,
                    1 => _horizontalStraight,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                },
                SectionTypes.LeftCorner => (int)direction switch
                {
                    0 => _cornerNw,
                    1 => _cornerSw,
                    2 => _cornerSe,
                    3 => _cornerNe,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                },
                SectionTypes.RightCorner => (int)direction switch
                {
                    0 => _cornerNe,
                    1 => _cornerNw,
                    2 => _cornerSw,
                    3 => _cornerSe,
                    _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
                },
                SectionTypes.StartGrid => _horizontalStartGrid,
                SectionTypes.Finish => _horizontalFinish,
                _ => throw new ArgumentOutOfRangeException(nameof(sectionType), sectionType, null)
            };
        }

        public static void OnNextRaceEvent(object sender, NextRaceEventArgs e)
        {
            Initialize(e.Race);
            _currentRace.DriversChanged += OnDriversChanged;
            DrawTrack(_currentRace.Track);
        }

        public static void Initialize(Race race)
        {
            _currentRace = race;
            _currentDirection = Direction.E;
            Console.Clear();
            Console.SetCursorPosition(0, 0);
        }

        public static void DrawTrack(Track track)
        {
            _cPosX = CursorStartPosX;
            _cPosY = CursorStartPosY;
            foreach (Section trackSection in track.Sections)
            {
                DrawSingleSection(trackSection);
            }
        }

        private static void DrawSingleSection(Section section)
        {
            string[] sectionStrings = ReplacePlaceHolders(
                SectionTypeToGraphic(section.SectionType, _currentDirection),
                _currentRace.GetSectionData(section).Left, _currentRace.GetSectionData(section).Right
            );

            int tempY = _cPosY;
            foreach (string s in sectionStrings)
            {
                Console.SetCursorPosition(_cPosX, tempY);
                Console.Write(s);
                tempY++;
            }

            if (section.SectionType == SectionTypes.RightCorner)
                _currentDirection = ChangeDirectionRight(_currentDirection);
            else if (section.SectionType == SectionTypes.LeftCorner)
                _currentDirection = ChangeDirectionLeft(_currentDirection);

            ChangeCursorToNextPosition();
        }

        internal static Direction ChangeDirectionLeft(Direction d)
        {
            return (Direction)(((uint)d - 1) % 4);
        }

        internal static Direction ChangeDirectionRight(Direction d)
        {
            return (Direction)(((uint)d + 1) % 4);
        }

        private static void ChangeCursorToNextPosition()
        {
            switch (_currentDirection)
            {
                case Direction.N:
                    _cPosY -= 4;
                    break;

                case Direction.E:
                    _cPosX += 4;
                    break;

                case Direction.S:
                    _cPosY += 4;
                    break;

                case Direction.W:
                    _cPosX -= 4;
                    break;
            }
        }

        internal static string[] ReplacePlaceHolders(string[] inputStrings, IParticipant leftParticipant,
            IParticipant rightParticipant)
        {
            string[] returnStrings = new string[inputStrings.Length];
            string lP = leftParticipant == null ? " " :
                leftParticipant.Equipment.IsBroken ? "X" : leftParticipant.Name.Substring(0, 1).ToUpper();
            string rP = rightParticipant == null ? " " :
                rightParticipant.Equipment.IsBroken ? "X" : rightParticipant.Name.Substring(0, 1).ToUpper();

            for (int i = 0; i < returnStrings.Length; i++)
            {
                returnStrings[i] = inputStrings[i].Replace("1", lP).Replace("2", rP);
            }
            return returnStrings;
        }

        private static void OnDriversChanged(object sender, DriversChangedEventArgs e)
        {
            DrawTrack(e.Track);
        }
    }
}