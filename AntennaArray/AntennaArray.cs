using System;
using System.Numerics;

namespace AntennaArray
{
    class AntennaArray1D : Antenna
    {
        private AntennaArrayItem1D[] Items;

        public AntennaArray1D(int N, double dx, Antenna ElementaryAntennaItem)
        {
            Items = new AntennaArrayItem1D[N];

            for (var i = 0; i < N; i++)
            {
                Items[i] = new AntennaArrayItem1D(ElementaryAntennaItem, dx * i);
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

    class AntennaArrayItem1D : Antenna
    {
        private Antenna Item;

        private double X;

        public AntennaArrayItem1D(Antenna ElementaryAntenna, double x)
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
