using System;
using System.Collections.Generic;
using System.Text;

namespace AntennaArray
{
    class MathAnalysis
    {
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
    }
}
