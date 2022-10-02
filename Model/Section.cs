namespace Model
{
    public class Section
    {
        public SectionTypes SectionType { get; set; }

        public Section(SectionTypes sectionType)
        {
            SectionType = sectionType;
        }

        public override string ToString()
        {
            return SectionType.ToString();
        }
    }
}