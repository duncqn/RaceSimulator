using Controller;
using Model;
using static Model.IParticipant;

namespace ControllerTest
{
    [TestFixture]
    public class Controller_Race_GetSectionData
    {
        private Race _testRace { get; set; }
        private Track _track { get; set; }
        private List<IParticipant> _participants { get; set; }

        [SetUp]
        public void SetUp()
        {
            Data.Initialize();
            Data.NextRace();
        }

        [TearDown]
        public void TearDown()
        {
            Data.CurrentRace.CleanUp();
            Data.CurrentRace = null;
        }
        

        [Test]
        public void GetSectionData_ReturnsData()
        {
            _track = new Track("test", new SectionTypes[] { SectionTypes.StartGrid, SectionTypes.Straight, SectionTypes.LeftCorner, SectionTypes.RightCorner, SectionTypes.Finish });
            _participants = new List<IParticipant>();
            _participants.Add(new Driver("Driver1", new Car(1, 1, 1, false),TeamColors.Blue));
            _testRace = new Race(_track, _participants);

            foreach (var section in Data.CurrentRace.Track.Sections)
            {
                var SectionData = Data.CurrentRace.GetSectionData(section);
                Assert.NotNull(SectionData);
            }
        }
    }
}