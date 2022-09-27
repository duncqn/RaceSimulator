using System;
using System.ComponentModel;

namespace Model
{
    public enum Side
    {
        Right,
        Left
    }

    public class SectionData
    {
        public IParticipant Left;
        public int DistanceLeft;
        public IParticipant Right;
        public int DistanceRight;

        public SectionData()
        {
        }

        public (IParticipant, int) GetDataBySide(Side side)
        {
            return side switch
            {
                Side.Left => (Left, DistanceLeft),
                Side.Right => (Right, DistanceRight),
            };
        }

        public void SetDataBySide(Side side, IParticipant participant, int distance)
        {
            switch (side)
            {
                case Side.Left:
                    Left = participant;
                    DistanceLeft = distance;
                    break;
                case Side.Right:
                    Right = participant;
                    DistanceRight = distance;
                    break;
            }
        }
    }
}