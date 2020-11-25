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

            const double Theta0 = 15 * toRad; // Угол фазирования антенной решётки

            /* --------------------------------------------------------- */


            Antenna antenna = new Vibrator();

            const double ThMin = -90 * toRad;
            const double ThMax = 90 * toRad;
            const double dTh = 1 * toRad;
            //PrintPattern(antenna, ThMin, ThMax, dTh);
            //PrintPattern(new Vibrator(), ThMin, ThMax, dTh);
            //PrintPattern(new Uniform(), ThMin, ThMax, dTh);

            const double dx = 0.0001;

            const double x1 = 0;
            const double x2 = 1;

            double S = 0;

            double x = x1;
            while (x <= x2)
            {
                var f = F(x);
                var dS = f * dx;
                S += dS;

                x += dx;
            }

            Console.WriteLine("Значение интеграла равно {0}", S);

            var result = GetIntegral(F, 0, 1, 0.001);

            var knd = 2 / GetIntegral(x => Math.Pow(antenna.Pattern(x).Magnitude, 2) * Math.Cos(x),
                -Math.PI/2, Math.PI/2, 0.00001);

            Console.WriteLine("КНД вибратора равно {0}", knd);
        }

        public static double GetIntegral(Func<double, double> f, double x1, double x2, double dx)
        {
            double S = 0;

            double x = x1;
            while (x <= x2)
            {
                var dS = f(x) * dx;
                S += dS;

                x += dx;
            }

            return S;
        }

        private static double F(double x)
        {
            return 3 * x * x;
        }

        private static void PrintPattern(Antenna antenna, double ThMin, double ThMax, double dTh)
        {
            for (var theta = ThMin; theta <= ThMax; theta += dTh)
            {
                var f = antenna.Pattern(theta);

                var value = new PatternValue();
                value.Theta = theta;
                value.Value = f;

                Console.WriteLine("{0,4:f0}  |  {1,6:F3}  |  {2,6:F3}  |  {3,7:F3}  |  {4,8:F3}",
                    theta * toDeg, f.Real, f.Imaginary,
                    value.GetValueInDB(), value.GetPhaseInDeg());
            }
        }

        private static List<PatternValue> GetPattern(
            double Theta0,
            double[] X, double Lambda,
            double ThetaMin, double ThetaMax,
            double dTheta)
        {
            var values = new List<PatternValue>();

            for (var theta = ThetaMin; theta <= ThetaMax; theta += dTheta)
            {
                var f = Pattern(theta * toRad, Theta0, X, Lambda);

                var value = new PatternValue();
                value.Theta = theta;
                value.Value = f;

                Console.WriteLine("{0,4}  |  {1,6:F3}  |  {2,6:F3}  |  {3,7:F3}  |  {4,8:F3}",
                    theta, f.Real, f.Imaginary,
                    value.GetValueInDB(), value.GetPhaseInDeg());

                values.Add(value);
            }

            return values;
        }

        private static Complex Pattern(double Theta, double Theta0, double[] X, double Lambda)
        {
            Complex result = 0;
            double k = 2 * Math.PI / Lambda;

            for (int n = 0; n < X.Length; n++)
            {
                var phase = k * X[n] * (Math.Sin(Theta) - Math.Sin(Theta0));
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
                        f.GetValueInDB(),
                        f.GetPhaseInDeg());
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

        public double GetValueInDB()
        {
            return 20 * Math.Log10(Value.Magnitude);
        }

        public double GetPhaseInDeg()
        {
            return Value.Phase * 180 / Math.PI;
        }
    }
}
