using System;
using System.Numerics;

namespace AntennaArray
{
    class AntennaArray : Antenna
    {
        private AntennaArrayItem[] Items;

        public AntennaArray(int N, double dx, Antenna ElementaryAntennaItem)
        {
            Items = new AntennaArrayItem[N];

            for (var i = 0; i < N; i++)
            {
                Items[i] = new AntennaArrayItem(ElementaryAntennaItem, dx * i);
            }
        }

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
        private Antenna Item;

        private double X;

        public AntennaArrayItem(Antenna ElementaryAntenna, double x)
        {
            Item = ElementaryAntenna;
            X = x;
        }

        public override Complex Pattern(double Theta)
        {
            var exp = Complex.Exp(-Complex.ImaginaryOne * X * Math.Sin(Theta));
            return Item.Pattern(Theta) * exp;
        }
    }
}
