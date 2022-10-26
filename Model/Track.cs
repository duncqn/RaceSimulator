using System.Collections.Generic;
using System.Drawing;

namespace Model
{
    public class Track
    {
        public string Name { get; set; }
        public LinkedList<Section> Sections { get; set; }
        public Direction StartDirection { get; }
        public Color Background { get; }

        public const int Width = 150;
        public const int Height = 150;

        public Track(string name, SectionTypes[] sections)
        {
            Name = name;
            Sections = GenerateSections(sections);
        }

        private LinkedList<Section> GenerateSections(SectionTypes[] sectionTypes)
        {
            LinkedList<Section> sections = new LinkedList<Section>();
            if (sectionTypes == null)
                return sections;
            foreach (SectionTypes sectionType in sectionTypes)
            {
                sections.AddLast(new Section(sectionType));
            }

            return sections;
        }

        public enum Direction
        {
            Up,
            Left,
            Down,
            Right
        }
    }
}