using System;
using System.Collections.Generic;
using System.Linq;
using Controller;

namespace Model
{
    public static class Visualization
    {
        public static void Initialize()
        {
            DrawTrack(Data.CurrentRace.Track);
        }

        #region graphics


        private static string[] _startHorizontal =
        {
            "****",
            " 1# ",
            "2 # ",
            "****"
        };

        private static readonly string[] _startVertical =
        {
            "*  *",
            "*##*",
            "*  *",
            "*  *"
        };

        private static readonly string[] _leftCornerHorizontal =
        {
            @"/  |",
            @"   |",
            @"   /",
            "**/ "
        };

        private static readonly string[] _RightCornerNorth =
        {
            @" /**",
            @"/    ",
            "|   ",
            "|  |",
        };

        private static readonly string[] _rightCornerEast =
        {

            @"**\ ",
            @"   \",
            "   |",
            "|  |"
        };

        private static readonly string[] _RightCornerWest =
        {
            @"|  \",
            @"|   ",
            @"\   ",
            @" \**"
        };

        private static readonly string[] _straightHorizontal =
        {
            "****",
            "    ",
            "    ",
            "****"
        };


        private static readonly string[] _straightVertical =
        {
            "|  |",
            "|  |",
            "|  |",
            "|  |"
        };

        private static readonly string[] _finishHorizontal =
        {
            "****",
            "  | ",
            "  | ",
            "****"
        };

        #endregion

        private static void DrawSection(string[] sectionDrawing, int x, int y)
        {
            foreach (var item in sectionDrawing)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(item);
                y++;
            }
        }

        private static Track DrawTrack(Track track)
        {
            var sections = track.Sections;

            var x = 0;
            var y = 4;
            var direction = 1;
            var Row = Console.CursorTop;
            const int Colum = 40;

            foreach (var section in sections)
            {
                if (section.SectionType == Section.SectionTypes.StartGrid)
                {
                    var sectionStrings = ReplaceStartGridNumber(_startHorizontal, section.Left, section.Right);
                    DrawSection(sectionStrings, Colum + x, Row + y);
                }

                if (section.SectionType == Section.SectionTypes.Straight)
                {
                    if (direction == 1 || direction == 3)
                    {
                        DrawSection(_straightHorizontal, Colum + x, Row + y);
                    }

                    if (direction == 0 || direction == 2)
                    {
                        DrawSection(_straightVertical, Colum + x, Row + y);
                    }
                }

                else if (section.SectionType == Section.SectionTypes.LeftCorner)
                {
                    foreach (var item in _leftCornerHorizontal)
                    {
                        Console.SetCursorPosition(Colum + x, Row + y);
                        Console.Write(item);
                        y++;
                    }

                    direction--;
                }

                else if (section.SectionType == Section.SectionTypes.RightCorner)
                {
                    switch (direction)
                    {
                        case 1:
                            DrawSection(_rightCornerEast, Colum + x, Row + y);
                            direction++;
                            break;
                        case 2:
                            DrawSection(_leftCornerHorizontal, Colum + x, Row + y);
                            direction++;
                            break;
                        case 3:
                            DrawSection(_RightCornerWest, Colum + x, Row + y);
                            direction++;
                            break;
                        case 0:
                            DrawSection(_RightCornerNorth, Colum + x, Row + y);
                            direction++;
                            break;
                    }
                }
                else if (section.SectionType == Section.SectionTypes.Finish)
                {
                    if (direction == 1 || direction == 3)
                    {
                        DrawSection(_finishHorizontal, Colum + x, Row + y);
                    }

                    if (direction == 0 || direction == 2)
                    {
                        DrawSection(_straightVertical, Colum + x, Row + y);
                    }
                }

                if (direction == 4)
                {
                    direction = 0;
                }

                if (direction == -1)
                {
                    direction = 3;
                }

                switch (direction)
                {
                    case 1:
                        x += 4;
                        break;
                    case 2:
                        y += 4;
                        break;
                    case 3:
                        x -= 4;
                        break;
                    case 0:
                        y -= 4;
                        break;
                }
            }

            return track;
        }

        private static string ReplaceNumber(this string text, string search, string replace)
        {
            int position = text.IndexOf(search, StringComparison.Ordinal);
            if (position < 0)
            {
                return text;
            }

            return text.Substring(0, position) + replace + text.Substring(position + search.Length);
        }

        private static string[] ReplaceStartGridNumber(string[] inputStrings, IParticipant participantLeft,
            IParticipant participantRight)
        {
            string[] strings = new string[inputStrings.Length];

            string leftParticipant = participantLeft == null ? " " : participantLeft.Name.Substring(0, 1).ToUpper();
            string rightParticipant = participantRight == null ? " " : participantRight.Name.Substring(0, 1).ToUpper();

            for (int i = 0; i < strings.Length; i++)
            {
                strings[i] = inputStrings[i].ReplaceNumber("1", leftParticipant)
                    .ReplaceNumber("2", rightParticipant);
            }

            return strings;
        }


        public static void OnDriversChanged(Object source, DriversChangedEventArgs e)
        {
            Console.Clear();
            DrawTrack(e.Track);
        }


    }
}