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
    public class Controller_Race_Start_And_CleanUp
    {
        private Race race;

        [SetUp]
        public void SetUp()
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
                new Driver("3", new Car(1, 1, 1, false), IParticipant.TeamColors.Blue),
                new Driver("4", new Car(1, 1, 1, false), IParticipant.TeamColors.Yellow),
            };

            race = new Race(track, participants);
        }

        [Test]
        public void Race_Start_WithoutErrors()
        {
            Assert.DoesNotThrow(() =>
            {
                race.Start();
            });
        }

        [Test]
        public void Race_CleanUp_WithoutErrors()
        {
            Assert.DoesNotThrow(() => { race.CleanUp(); });
        }
    }
}