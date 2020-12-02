using System;
using System.Collections.Generic;
using System.Text;

namespace AntennaArray
{
    struct Vector2D
    {
        private double _X;
        private double _Y;

        public double X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        public double Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        //public double Radius
        //{
        //    get
        //    {
        //        return Math.Sqrt(_X * _X + _Y * _Y);
        //    }
        //}

        //public double Radius
        //{
        //    get => Math.Sqrt(_X * _X + _Y * _Y);
        //}

        public double Radius => Math.Sqrt(_X * _X + _Y * _Y);

        public double Angle => Math.Atan2(_Y, _X); //Math.Atan(_Y / _X);

        public Vector2D(double X, double Y)
        {
            _X = X;
            _Y = Y;
        }

        //public static Vector2D operator +(Vector2D A, Vector2D B)
        //{
        //    return new Vector2D(A.X + B.X, A.Y + B.Y);
        //}

        public static Vector2D operator +(Vector2D A, Vector2D B) => new Vector2D(A.X + B.X, A.Y + B.Y);

        public static Vector2D operator -(Vector2D A, Vector2D B) => new Vector2D(A.X - B.X, A.Y - B.Y);

        public static Vector2D operator +(Vector2D A, double k) => new Vector2D(A.X + k, A.Y + k);
        public static Vector2D operator +(double k, Vector2D A) => new Vector2D(A.X + k, A.Y + k);

        public static Vector2D operator -(Vector2D A, double k) => new Vector2D(A.X - k, A.Y - k);
        public static Vector2D operator -(double k, Vector2D A) => new Vector2D(k - A.X, k - A.Y);

        public static Vector2D operator *(Vector2D A, double k) => new Vector2D(A.X * k, A.Y * k);
        public static Vector2D operator *(double k, Vector2D A) => new Vector2D(A.X * k, A.Y * k);

        public static Vector2D operator /(Vector2D A, double k) => new Vector2D(A.X / k, A.Y / k);
        public static Vector2D operator /(double k, Vector2D A) => new Vector2D(k / A.X, k / A.Y);

        public static double operator *(Vector2D A, Vector2D B) => A.X * B.X + A.Y * B.Y;
    }
}
