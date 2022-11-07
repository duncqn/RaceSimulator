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

        public Driver(String Name, IEquipment Equipment, IParticipant.TeamColors Teamcolors)
        {
            this.Name = Name;
            this.Equipment = Equipment;
            this.TeamColor = Teamcolors;
        }

        public int GetMovementSpeed()
        {
            return Equipment.Performance * Equipment.Speed;
        }
    }
}