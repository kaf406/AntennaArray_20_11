using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.Intrinsics;

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

            var M = new Matrix(3, 3);

            M[0, 0] = 5;
            M[0, 1] = 4;
            M[0, 2] = 7;

            for(var i = 0; i < M.M; i++)
                for (var j = 0; j < M.N; j++)
                    M[i, j] = i + j;

            var X = new Vector2D(5, 7);
            var Y = new Vector2D(10, 12);

            var Z = X + Y;
            var Z1 = X * 5;
            var Z2 = X / 5;

            Console.WriteLine("|X| = {0}; angle(X) = {1}", X.Radius, X.Angle);
            Console.WriteLine("|Y| = {0}; angle(Y) = {1}", Y.Radius, Y.Angle);

            Console.ReadLine();

            //Vibrator antenna = new Vibrator(0.2);
            //var f_30 = antenna.Pattern(30 * toDeg);
            //antenna._Length = 0.3;

            //Console.WriteLine("Длина вибратора {0}", antenna.Length);
            //antenna.Length = 0.7;

            var antenna_array = new AntennaArray1D(16, 0.5, new Vibrator(0.5));

            PrintPattern(antenna_array, -Math.PI / 2, Math.PI / 2, 0.1 * toRad);
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

                Console.WriteLine("{0,4:f1}  |  {1,6:F3}  |  {2,6:F3}  |  {3,7:F3}  |  {4,8:F3}",
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
