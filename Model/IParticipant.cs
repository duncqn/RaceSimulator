using System;
using System.Collections.Generic;
using System.Text;
using static Model.SectionData;

namespace Model
{
    public interface IParticipant
    {
        public string Name { get; }
        
        public IEquipment Equipment { get; }
        public TeamColors TeamColor { get; }

        public enum TeamColors { Red, Green, Yellow, Grey, Blue };
    }
}