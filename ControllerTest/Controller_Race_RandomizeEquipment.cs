using Controller;
using Model;

namespace ControllerTest;

[TestFixture]
public class Controller_Race_RandomizeEquipment
{
    private List<IParticipant> _participant;
    private Race _race;


    [SetUp]
    public void SetUp()
    {
        var sections = new[]
        {
            SectionTypes.StartGrid
        };

        var track = new Track("Track", sections);

        _participant = new List<IParticipant>
        {
            new Driver("Driver1", new Car(1, 1, 1, false), IParticipant.TeamColors.Red),
            new Driver("Driver2", new Car(1, 1, 1, false), IParticipant.TeamColors.Green),
            new Driver("Driver3", new Car(1, 1, 1, false), IParticipant.TeamColors.Blue),
            new Driver("Driver4", new Car(1, 1, 1, false), IParticipant.TeamColors.Yellow), 
        };

        _race = new Race(track, _participant);
    }
    
    [Test]
    public void Change_Performance_Quality()
    {
        _race.RandomEquipmentBreaking();

        foreach (var participant in _participant)
        {
            Assert.IsTrue(participant.Equipment.Quality != 1 && participant.Equipment.Performance != 1);
        }
    }
}