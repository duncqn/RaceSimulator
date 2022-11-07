namespace Model
{
    public class Section
    {
        public SectionTypes SectionType { get;}

        public Section(SectionTypes sectionType)
        {
            SectionType = sectionType;
        }
    }
}