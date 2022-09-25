using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class SectionData
    {
        public IParticipant Left;
        public int DistanceLeft;
        public IParticipant Right;
        public int DistanceRight;

        public SectionData()
        {
            DistanceLeft = 100;
            DistanceRight = 100;
        }
    }
}