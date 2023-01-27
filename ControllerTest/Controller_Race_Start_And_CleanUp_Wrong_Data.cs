using Controller;
using Model;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using static Model.Section;

namespace ControllerTest
{
    [TestFixture]
    public class Controller_Race_Start_And_CleanUp_Wrong_Data
    {
        [Test]
        public void Race_Start_WithoutErrors()
        {
            var sections = new[]
            {
                SectionTypes.StartGrid, SectionTypes.Straight, SectionTypes.Straight, SectionTypes.Finish
            };

            var track = new Track("Track", sections);

            var participants = new List<IParticipant>
            {
                new Driver("1", new Car(1, 1, 1, false), IParticipant.TeamColors.Red),
                new Driver("2", new Car(1, 1, 1, false), IParticipant.TeamColors.Green),
            };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Race(track, participants);
            });
        }
    }
}