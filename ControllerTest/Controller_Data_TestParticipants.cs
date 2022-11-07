using Controller;
using Model;

namespace ControllerTest;

[TestFixture]
public class Controller_Data_TestParticipants
{
    [Test]
    public void AddParticipants_To_Race()
    {
        Data.AddParticipants();
        Assert.IsNotEmpty(Data.Competition.Participants);
    }
}