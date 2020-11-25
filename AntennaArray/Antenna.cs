using System;
using System.Numerics;

namespace AntennaArray
{
    abstract class Antenna
    {
        public abstract Complex Pattern(double Theta);

        //public double GetKND()
        //{

        //}
    }

    class Uniform : Antenna
    {
        public override Complex Pattern(double Theta)
        {
            return 1;
        }
    }

    class Vibrator : Antenna
    {
        /// <summary>Нормированная длина вибратора (длина, нормированная к длине волны)</summary>
        public double Length = 0.5;

        public override Complex Pattern(double Theta)
        {
            // (cos(k*L * sin(Theta)) - cos(k*L)) / (cos(Theta) * (1 - cos(k*L)))
            // (cos(2*PI*L * sin(Theta)) - cos(2*PI*L)) / (cos(Theta) * (1 - cos(2*PI*L)))

            var l = Math.PI * 2 * Length;

            return (Math.Cos(l * Math.Sin(Theta)) - Math.Cos(l)) / (Math.Cos(Theta) * (1 - Math.Cos(l)));
        }
    }

    class Rupor : Antenna
    {
        public override Complex Pattern(double Theta)
        {
            var f = Math.Cos(Theta);
            return f * f;
            //return Math.Pow(f, 2);
        }
    }
}
