using System;
using System.Collections.Generic;
using System.Text;
using static Model.SectionData;

namespace Model
{
    public class Section : SectionData
    {
        public SectionTypes SectionType;
        public enum SectionTypes { Straight, LeftCorner, RightCorner, StartGrid, Finish };
    }
}