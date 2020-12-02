using System;
using System.Collections.Generic;
using System.Text;

namespace AntennaArray
{
    class Matrix
    {
        /// <summary>Элементы матрицы</summary>
        private double[,] _Items;

        public int M => _Items.GetLength(0);

        public int N => _Items.GetLength(1);

        public double this[int i, int j]
        {
            get => _Items[i, j];
            set => _Items[i, j] = value;
        }

        /// <summary>Инициализация новой матрицы</summary>
        /// <param name="M">Число строк</param>
        /// <param name="N">Число столбцов</param>
        public Matrix(int M, int N)
        {
            _Items = new double[M, N];
        }

        public static Matrix operator +(Matrix M, double x)
        {
            var result = new Matrix(M.M, M.N);
            for(var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = M[i, j] + x;

            return result;
        }

        public static Matrix operator -(Matrix M, double x)
        {
            var result = new Matrix(M.M, M.N);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = M[i, j] - x;

            return result;
        }

        public static Matrix operator +(double x, Matrix M)
        {
            var result = new Matrix(M.M, M.N);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = M[i, j] + x;

            return result;
        }

        public static Matrix operator -(double x, Matrix M)
        {
            var result = new Matrix(M.M, M.N);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = x - M[i, j];

            return result;
        }

        public static Matrix operator *(Matrix M, double x)
        {
            var result = new Matrix(M.M, M.N);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = M[i, j] * x;

            return result;
        }

        public static Matrix operator /(Matrix M, double x)
        {
            var result = new Matrix(M.M, M.N);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = M[i, j] / x;

            return result;
        }

        public static Matrix operator *(double x, Matrix M)
        {
            var result = new Matrix(M.M, M.N);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = M[i, j] * x;

            return result;
        }

        public static Matrix operator +(Matrix A, Matrix B)
        {
            if (A.M != B.M)
                throw new InvalidOperationException("Не совпадает число строк матриц");
            if (A.N != B.N)
                throw new InvalidOperationException("Не совпадает число столбцов матриц");

            var result = new Matrix(A.M, A.N);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = A[i, j] + B[i, j];

            return result;
        }

        public static Matrix operator -(Matrix A, Matrix B)
        {
            if (A.M != B.M)
                throw new InvalidOperationException("Не совпадает число строк матриц");
            if (A.N != B.N)
                throw new InvalidOperationException("Не совпадает число столбцов матриц");

            var result = new Matrix(A.M, A.N);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                    result[i, j] = A[i, j] - B[i, j];

            return result;
        }

        public static Matrix operator *(Matrix A, Matrix B)
        {
            if (A.N != B.M)
                throw new InvalidOperationException("Число столбцов первого множителя не равно числу строк второго");

            var result = new Matrix(A.N, B.M);
            for (var i = 0; i < result.M; i++)
                for (var j = 0; j < result.N; j++)
                {
                    double S = 0;
                    for (var k = 0; k < A.M; k++)
                        S += A[i, k] * B[k, j];

                    result[i, j] = S;
                }

            return result;
        }
    }
}
