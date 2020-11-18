using System;
using System.Collections.Generic;
using System.Numerics;

namespace AntennaArray
{
    class Program
    {
        static void Main(string[] args)
        {
            const double f0 = 3; // Частота в ГГц
            const double c = 30; // Скорость света в см * ГГц
            const double lambda0 = c / f0; // Длина волны см

            const double toRad = Math.PI / 180;
            const double toDeg = 1 / toRad;
            const double Th07 = 5 * toRad; // Половина ширины луча

            // 2Th07 = 51 * lambda0/L
            const double L1 = 51 * lambda0 / (2 * Th07 * toDeg); // Размер апертуры в см

            const double Th0max = 45 * toRad; // Сектор сканирования (максимальное отклонение луча)

            /* --------------------------------------------------------- */

            Console.WriteLine("Расчёт ДН антенной решётки");
            Console.WriteLine("Частота {0} ГГц, длина волны {1} см", f0, lambda0);

            Console.WriteLine("Требуется обеспечить ширину луча {0} градусов", 2 * Th07 * toDeg);

            Console.WriteLine("Размер апертуры решётки {0} см ({1}м)", L1, L1 / 100);

            var dx = lambda0 / (1 + Math.Abs(Math.Sin(Th0max)));

            Console.WriteLine("Шаг между элементами решётки {0:f2} см", dx);

            int N = (int)Math.Ceiling(L1 / dx); // Число элементов в решётке

            Console.WriteLine("Число элементов в решётке {0}", N);

            var L = (N - 1) * dx; // Физический размер апертуры

            double[] X = new double[N];
            for (var i = 0; i < X.Length; i++)
            {
                X[i] = i * dx - L / 2;
            }

            //Complex F = Pattern(0, X, lambda0);

            const double ThetaMin = -180;
            const double ThetaMax = 180;
            const double dTheta = 1;

            Console.WriteLine("Расчёт ДН");
            Console.WriteLine("Theta |  Re(F)   |  Im(F)   | Abs(F) db |Phase(F) deg");

            List<PatternValue> values = new List<PatternValue>();

            for (var theta = ThetaMin; theta <= ThetaMax; theta += dTheta)
            {
                var f = Pattern(theta * toRad, X, lambda0);

                Console.WriteLine("{0,4}  |  {1,6:F3}  |  {2,6:F3}  |  {3,7:F3}  |  {4,8:F3}",
                    theta, f.Real, f.Imaginary, 
                    20*Math.Log10(f.Magnitude), f.Phase * toDeg);

                PatternValue value = new PatternValue();
                value.Theta = theta;
                value.Value = f;

                values.Add(value);
            }
        }

        private static Complex Pattern(double Theta, double[] X, double Lambda)
        {
            Complex result = 0;
            double k = 2 * Math.PI / Lambda;

            for (int n = 0; n < X.Length; n++)
            {
                var phase = k * X[n] * Math.Sin(Theta);
                var f = Complex.Exp(new Complex(0, phase));

                result += f;
            }

            return result / X.Length;
        }
    }

    struct PatternValue
    {
        public double Theta;

        public Complex Value;
    }
}
