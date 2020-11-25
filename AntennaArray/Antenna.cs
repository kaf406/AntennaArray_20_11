using System;
using System.Numerics;

namespace AntennaArray
{
    abstract class Antenna
    {
        public abstract Complex Pattern(double Theta);

        public double GetKND()
        {
            const double th_min = -Math.PI / 2;
            const double th_max = Math.PI / 2;
            const double dth = 0.1 * Math.PI / 180;

            var integral = MathAnalysis.GetIntegral(
                th => Math.Pow(Pattern(th).Magnitude, 2) * Math.Cos(th),
                th_min, th_max, dth);

            return 2 / integral;
        }
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

            var A = Math.Cos(l * Math.Sin(Theta)) - Math.Cos(l);
            var B = Math.Cos(Theta) * (1 - Math.Cos(l));

            if (A == 0 && B == 0)
                return 1;
            else
                return A / B;
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
