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
            const double dTheta = 0.5;

            Console.WriteLine("Расчёт ДН");
            Console.WriteLine("Theta |  Re(F)   |  Im(F)   | Abs(F) db |Phase(F) deg");

            var values = GetPattern(Theta0, X, lambda0, ThetaMin, ThetaMax, dTheta);

            WritePatternToFile(values, "pattern.txt");

            var max_angle = GetMainMaxAngle(values);

            var beam_width = GetPatternWidth(values, max_angle);

            Console.WriteLine("Положение главного максимума {0:f2} градусов", max_angle);
            Console.WriteLine("Ширина диаграммы направленности {0:f2} градусов", beam_width);

            var student1 = new Student();
            student1.LastName = "Иванов";
            student1.FirstName = "Иван";
            student1.Patronymic = "Иванович";
            student1.Birthday = new DateTime(2000, 1, 15);

            var student2 = new Student();
            student2.LastName = "Петров";
            student2.FirstName = "Пётр";
            student2.Patronymic = "Петрович";
            student2.Birthday = new DateTime(2002, 7, 10);

            student1.PrintToConsole();
            Console.WriteLine();

            student2.PrintToConsole();

            var antenna1 = new Dipole();
            antenna1.Length = lambda0 / 2;

            var antenna2 = new Dipole();
            antenna2.Length = lambda0;
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

    class Student
    {
        public string FirstName;
        public string LastName;
        public string Patronymic;

        //public int Age;
        public DateTime Birthday;

        public void PrintToConsole()
        {
            Console.WriteLine("Меня зовут {0} {1} {2}", LastName, FirstName, Patronymic);
            Console.WriteLine("Мне {0} лет", Math.Ceiling((DateTime.Now - Birthday).Days / 365.0));
        }
    }


    class Antenna
    {
        public virtual Complex Pattern(double Theta, double Lambda)
        {
            return 1;
        }
    }

    class Dipole : Antenna
    {
        public double Length;

        public override Complex Pattern(double Theta, double Lambda)
        {
            var k = 2 * Math.PI / Lambda;
            var kL = k * Length;

            return (Math.Cos(kL * Math.Sin(Theta)) - Math.Cos(kL)) / (Math.Cos(Theta) * (1 - Math.Cos(kL)));
        }
    }

    class AntennaArray : Antenna
    {

    }
}
