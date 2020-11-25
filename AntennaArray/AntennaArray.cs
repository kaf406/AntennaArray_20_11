using System;
using System.Numerics;

namespace AntennaArray
{
    class AntennaArray : Antenna
    {
        public AntennaArrayItem[] Items;

        public override Complex Pattern(double Theta)
        {
            Complex sum = 0;

            for(var i = 0; i < Items.Length; i++)
            {
                sum += Items[i].Pattern(Theta);
            }

            return sum / Items.Length;
        }
    }

    class AntennaArrayItem : Antenna
    {
        public Antenna Item;

        public double X;

        public override Complex Pattern(double Theta)
        {
            var exp = Complex.Exp(-Complex.ImaginaryOne * X * Math.Sin(Theta));
            return Item.Pattern(Theta) * exp;
        }
    }
}
