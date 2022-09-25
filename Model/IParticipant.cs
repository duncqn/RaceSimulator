using System;
using System.Collections.Generic;
using System.Text;
using static Model.SectionData;

namespace Model
{
    public interface IParticipant
    {
        public string Name { get; set; }
        public int Points { get; set; }
        public IEquipment Equipment { get; set; }
        public TeamColors GetTeamColor { get; set; }

        public enum TeamColors { Red, Green, Yellow, Grey, Blue };
    }
}