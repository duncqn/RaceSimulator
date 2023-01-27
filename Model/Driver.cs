using System;
using System.Collections.Generic;
using System.Text;


namespace Model
{
    public class Driver : IParticipant
    {
        public string Name { get; set; }
        public IEquipment Equipment { get; set; }
        public IParticipant.TeamColors TeamColor { get; set; }

        public Driver(String name, IEquipment equipment, IParticipant.TeamColors teamcolors)
        {
            this.Name = name;
            this.Equipment = equipment;
            this.TeamColor = teamcolors;
        }

        public int GetMovementSpeed()
        {
            return Equipment.Performance * Equipment.Speed;
        }
    }
}