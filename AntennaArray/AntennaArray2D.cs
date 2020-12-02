using System;
using System.Collections.Generic;
using System.Text;

namespace AntennaArray
{
    class AntennaArray2D
    {
        private AntennaArray2DItem[] _Items;

        public AntennaArray2D(int M, int N, double dx, double dy, Antenna ElementaryAntenna)
        {
            var items = new List<AntennaArray2DItem>(N * M);
            for(var n = 0; n < N; n++)
                for (var m = 0; m < M; m++)
                {
                    items.Add(new AntennaArray2DItem(new Vector2D(m * dx, n * dy), ElementaryAntenna));
                }

            _Items = items.ToArray();
        }
    }

    class AntennaArray2DItem
    {
        private Vector2D _R;
        private Antenna _Element;

        public AntennaArray2DItem(Vector2D R, Antenna ElementaryAntenna)
        {
            _R = R;
            _Element = ElementaryAntenna;
        }


    }
}
