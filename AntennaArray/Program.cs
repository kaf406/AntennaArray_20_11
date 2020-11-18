using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace AntennaArray
{
    class Program
    {
        const double toRad = Math.PI / 180;
        const double toDeg = 1 / toRad;

        static void Main(string[] args)
        {
            const double f0 = 3; // Частота в ГГц
            const double c = 30; // Скорость света в см * ГГц
            const double lambda0 = c / f0; // Длина волны см

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
            const double dTheta = 0.1;

            Console.WriteLine("Расчёт ДН");
            Console.WriteLine("Theta |  Re(F)   |  Im(F)   | Abs(F) db |Phase(F) deg");

            List<PatternValue> values = new List<PatternValue>();

            for (var theta = ThetaMin; theta <= ThetaMax; theta += dTheta)
            {
                var f = Pattern(theta * toRad, X, lambda0);

                Console.WriteLine("{0,4}  |  {1,6:F3}  |  {2,6:F3}  |  {3,7:F3}  |  {4,8:F3}",
                    theta, f.Real, f.Imaginary,
                    20 * Math.Log10(f.Magnitude), f.Phase * toDeg);

                PatternValue value = new PatternValue();
                value.Theta = theta;
                value.Value = f;

                values.Add(value);
            }

            WritePatternToFile(values, "pattern.txt");

            var max_angle = GetMainMaxAngle(values);

            var beam_width = GetPatternWidth(values, max_angle);

            Console.WriteLine("Положение главного максимума {0:f2} градусов", max_angle);
            Console.WriteLine("Ширина диаграммы направленности {0:f2} градусов", beam_width);
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

        private static void WritePatternToFile(List<PatternValue> Values, string FileName)
        {
            using (StreamWriter writer = new StreamWriter(FileName))
            {
                writer.WriteLine("Theta;Re(F);Im(F);Abs(F)db;Phase(F)deg");

                foreach (var f in Values)
                {
                    writer.WriteLine("{0};{1};{2};{3};{4}",
                        f.Theta, f.Value.Real, f.Value.Imaginary,
                        20 * Math.Log10(f.Value.Magnitude),
                        f.Value.Phase * toDeg);
                }

            }
        }

        private static double GetMainMaxAngle(List<PatternValue> Values)
        {
            var min_angle = double.PositiveInfinity;
            var theta_0_index = 0;
            for (var i = 0; i < Values.Count; i++)
            {
                var abs_angle = Math.Abs(Values[i].Theta);

                if (abs_angle < min_angle)
                {
                    min_angle = abs_angle;
                    theta_0_index = i;
                }
            }

            var max_pattern_value = Values[theta_0_index].Value.Magnitude;
            var max_index = theta_0_index;
            for (var i = theta_0_index; i < Values.Count; i++)
            {
                if (Values[i].Value.Magnitude > max_pattern_value)
                {
                    max_pattern_value = Values[i].Value.Magnitude;
                    max_index = i;
                }
            }

            for (var i = theta_0_index; i >= 0; i--)
            {
                if (Values[i].Value.Magnitude > max_pattern_value)
                {
                    max_pattern_value = Values[i].Value.Magnitude;
                    max_index = i;
                }
            }

            return Values[max_index].Theta;
        }

        private static double GetPatternWidth(List<PatternValue> Values, double Theta0)
        {
            var max_index = 0;
            for (var i = 0; i < Values.Count; i++)
            {
                if (Math.Abs(Values[i].Theta - Theta0) < 0.0000001)
                {
                    max_index = i;
                    break;
                }
            }

            var max = Values[max_index].Value.Magnitude;

            double right_beam_edge = 0;
            for (var i = max_index; i < Values.Count; i++)
            {
                var f = Values[i].Value.Magnitude;

                var db = 20 * Math.Log10(f / max);
                if (db < -3)
                {
                    right_beam_edge = Values[i].Theta;
                    break;
                }
            }

            double left_beam_edge = 0;
            for (var i = max_index; i >= 0; i--)
            {
                var f = Values[i].Value.Magnitude;

                var db = 20 * Math.Log10(f / max);
                if (db < -3)
                {
                    left_beam_edge = Values[i].Theta;
                    break;
                }
            }

            return right_beam_edge - left_beam_edge;
        }
    }

    struct PatternValue
    {
        public double Theta;

        public Complex Value;
    }
}
