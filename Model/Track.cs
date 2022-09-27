using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Model.Section;

namespace Model
{
    public class Track
    {
        private SectionTypes[] sectionTypes;

        public string Name { get; set; }
        public LinkedList<Section> Sections { get; set; }

        public Track(string name, SectionTypes[] sections)
        {
            this.Name = name;
            this.Sections = ArrayToList(sections);
        }


        public LinkedList<Section> ArrayToList(SectionTypes[] sectionTypes)
        {
            var result = new LinkedList<Section>();

            foreach (SectionTypes sectionType in sectionTypes)
            {
                Section section = new Section();
                section.SectionType = sectionType;
                result.AddLast(section);
            }

            return result;
        }

        public Section GetNextSection(Section currentSection)
        {
            LinkedListNode<Section> node = Sections.Find(currentSection);

            if (node != null)
            {
                if (node.Next == null)
                {
                    return Sections.First.Value;
                }

                return node.Next.Value;
            }

            return null;
        }

        public Section GetPreviousSection(Section currentSection)
        {
            LinkedListNode<Section> node = Sections.Find(currentSection);

            if (node != null)
            {
                if (node.Previous == null)
                {
                    return Sections.Last.Value;
                }

                return node.Previous.Value;
            }

            return null;
        }
    }
}