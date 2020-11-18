using System;

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
            const double Th07 = 0.5 * toRad; // Половина ширины луча

            // 2Th07 = 51 * lambda0/L
            const double L1 = 51 * lambda0 / (Th07 * toDeg); // Размер апертуры в см

            const double Th0max = 45 * toRad; // Сектор сканирования (максимальное отклонение луча)

            /* --------------------------------------------------------- */

            Console.WriteLine("Расчёт ДН антенной решётки");
            Console.WriteLine("Частота {0} ГГц, длина волны {1} см", f0, lambda0);

            Console.WriteLine("Требуется обеспечить ширину луча {0} градусов", 2 * Th07 * toDeg);

            Console.WriteLine("Размер апертуры решётки {0} см ({1}м)", L1, L1 / 100);

            var dx = lambda0 / (1 + Math.Abs(Math.Sin(Th0max)));

            Console.WriteLine("Шаг между элементами решётки {0:f2} см", dx);

            var N = Math.Ceiling(L1 / dx); // Число элементов в решётке

            Console.WriteLine("Число элементов в решётке {0}", N);

            var L = N * dx; // Физический размер апертуры
        }
    }
}
