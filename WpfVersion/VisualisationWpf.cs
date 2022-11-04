using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using Model;
using Controller;
using static Model.IParticipant;

namespace WpfVersion;

public static class VisualisationWpf
{
    private static int _actualDirection = 1;
    private static int _positionX, _positionY;


    public static void Initialize()
    {
    }

    #region graphics

    private const string _startGridHorizontal =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\_horizontalStartGrid.png";

    private static string _leftCornerH =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\leftCornerN.png";

    private static string _rightCornerN =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\rightCornerN.png";

    private static string _rightCornerE =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\rightCornerE.png";

    private static string _rightCornerW =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\rightCornerW.png";

    private static string _straightHorizontal =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\_horizontalStraight.png";

    private static string _straightVertical =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\_verticalStraight.png";

    private static string _finishHorizontal =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\_horizontalFinish.png";

    private static string _raceAutoBlauw =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\carBlue.png";

    private static string _raceAutoGeel =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\carYellow.png";

    private static string _raceAutoGrijs =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\carGrey.png";

    private static string _raceAutoGroen =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\carGreen.png";

    private static string _raceAutoRood =
        "C:\\Users\\dunca\\Documents\\GitHub\\RaceSimulator\\WpfVersion\\afbeeldingen\\carRed.png";

    #endregion

    private static void trackOnScreen(Graphics g, Bitmap image)
    {
        g.DrawImage(image, new Point((_positionX * Track.Width), (_positionY * Track.Height)));
    }

    public static BitmapSource DrawTrack(Track track, int width, int height)
    {
        Bitmap bitmaptrack = Cache.GenerateBitmap(width, height);
        using (Graphics g = Graphics.FromImage(bitmaptrack))
        {
            foreach (Section section in track.Sections)
            {
                SectionData sectionData = Data.CurrentRace.GetSectionData(section);
                IParticipant participant1 = sectionData?.Right;
                IParticipant participant2 = sectionData?.Left;
                GetDirection();
                if (section.SectionType == SectionTypes.StartGrid)
                {
                       trackOnScreen(g,
                           participantsOnScreen(Cache.GetImageData(_startGridHorizontal, Track.Width, Track.Height),
                                participant1, participant2));

                }

                if (section.SectionType == SectionTypes.Straight)
                {
                    if (_actualDirection is 1 or 3)
                    {
                        trackOnScreen(g,
                            participantsOnScreen(Cache.GetImageData(_straightHorizontal, Track.Width, Track.Height),
                                participant1, participant2));
                    }

                    if (_actualDirection is 0 or 2)
                    {
                        trackOnScreen(g,
                            participantsOnScreen(Cache.GetImageData(_straightVertical, Track.Width, Track.Height),
                                participant1, participant2));
                    }
                }
                else if (section.SectionType == SectionTypes.RightCorner)
                {
                    switch (_actualDirection)
                    {
                        case 1:
                            trackOnScreen(g,
                                participantsOnScreen(Cache.GetImageData(_rightCornerE, Track.Width, Track.Height),
                                    participant1, participant2));
                            _actualDirection++;
                            break;
                        case 2:
                            trackOnScreen(g,
                                participantsOnScreen(Cache.GetImageData(_leftCornerH, Track.Width, Track.Height),
                                    participant1, participant2));
                            _actualDirection++;
                            break;
                        case 3:
                            trackOnScreen(g,
                                participantsOnScreen(Cache.GetImageData(_rightCornerW, Track.Width, Track.Height),
                                    participant1, participant2));
                            _actualDirection++;
                            break;
                        case 0:
                            trackOnScreen(g,
                                participantsOnScreen(Cache.GetImageData(_rightCornerN, Track.Width, Track.Height),
                                    participant1, participant2));
                            _actualDirection++;
                            break;
                    }
                }

                else if (section.SectionType == SectionTypes.Finish)
                {
                    if (_actualDirection is 1 or 3)
                    {
                        trackOnScreen(g,
                            participantsOnScreen(Cache.GetImageData(_finishHorizontal, Track.Width, Track.Height),
                                participant1, participant2));
                    }

                    if (_actualDirection is 0 or 2)
                    {
                        trackOnScreen(g,
                            participantsOnScreen(Cache.GetImageData(_straightVertical, Track.Width, Track.Height),
                                participant1, participant2));
                    }
                }
            }
        }

        return Cache.CreateBitmapSourceFromGdiBitmap(bitmaptrack);
    }

    public static void GetDirection()
    {
        if (_actualDirection == 4)
        {
            _actualDirection = 0;
        }

        if (_actualDirection == -1)
        {
            _actualDirection = 3;
        }

        switch (_actualDirection)
        {
            case 1:
                _positionX += 1;
                break;
            case 2:
                _positionY += 1;
                break;
            case 3:
                _positionX -= 1;
                break;
            case 0:
                _positionY -= 1;
                break;
        }
    }

    public static string TeamColor(TeamColors teamcolor)
    {
        if (teamcolor == TeamColors.Blue)
        {
            return _raceAutoBlauw;
        }

        if (teamcolor == TeamColors.Yellow)
        {
            return _raceAutoGeel;
        }

        if (teamcolor == TeamColors.Grey)
        {
            return _raceAutoGrijs;
        }

        if (teamcolor == TeamColors.Green)
        {
            return _raceAutoGroen;
        }

        if (teamcolor == TeamColors.Red)
        {
            return _raceAutoRood;
        }
        return "";
    }

    private static void ParticipantOnTrack(Bitmap track, Bitmap participant, int _positionX, int _positionY)
    {
        using (Graphics g = Graphics.FromImage(track))
        {
            g.DrawImage(participant, new Point(_positionX, _positionY));
        }
    }

    private static Bitmap participantsOnScreen(Bitmap track, IParticipant participant1, IParticipant participant2)
    {
        if (participant1 != null)
        {
            ParticipantOnTrack(track,
                Rotate90(Cache.GetImageData(TeamColor(participant1.TeamColor), 50, 50),
                    (_actualDirection - 1) * 90), 15, 60);
        }

        if (participant2 != null)
        {
            ParticipantOnTrack(track,
                Rotate90(Cache.GetImageData(TeamColor(participant2.TeamColor), 50, 50),
                    (_actualDirection - 1) * 90), 60, 15);
        }

        return track;
    }

    private static Bitmap Rotate90(Bitmap bm, float angle)
    {
        Bitmap bt = new Bitmap(bm.Width, bm.Height);
        Graphics g = Graphics.FromImage(bt);
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        g.TranslateTransform((float)bm.Height / 2, (float)bm.Width / 2);
        g.RotateTransform(angle);
        g.TranslateTransform(-(float)bm.Height / 2, -(float)bm.Width / 2);
        g.DrawImage(bm, new Point(0, 0));
        return bt;
    }
}