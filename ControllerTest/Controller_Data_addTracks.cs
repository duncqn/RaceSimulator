using Controller;
using Model;

namespace ControllerTest;

[TestFixture]
public class Controller_Race_CheckNextRace
{
    [Test]
    public void AddTracks_Race()
    {
        Data.AddTracks();
        Assert.IsNotEmpty(Data.Competition.Tracks);
    }
}