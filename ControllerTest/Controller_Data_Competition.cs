using Controller;

namespace ControllerTest;
[TestFixture]
public class Controller_Data_Competition
{

    [Test]
    public void Data_Competition_IsNotNull()
    {
        Data.Initialize();
        Assert.IsNotNull(Data.Competition);
    }
    
    [Test]
    public void Data_Competition_IsNull()
    {
        Assert.IsNotNull(Data.Competition);
    }
}