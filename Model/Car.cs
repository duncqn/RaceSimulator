using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    public class Car : IEquipment
    {
        public int Quality { get; set; }
        public int Performance { get; set; }
        public int Speed { get; set; }
        public bool IsBroken { get; set; }

        public Car(int Quality, int Performance, int Speed, bool isBroken)
        {
            this.Quality = Quality;
            this.Performance = Performance;
            this.Speed = Speed;
            this.IsBroken = IsBroken;
        }
    }
}