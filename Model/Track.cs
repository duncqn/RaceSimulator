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
    }
}