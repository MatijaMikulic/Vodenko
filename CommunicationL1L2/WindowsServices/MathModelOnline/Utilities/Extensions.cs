using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace MathModelOnline.Utilities
{
    namespace MatrixExponential
    {
        public static class Extensions
        {
            /// <summary>
            /// Computes the matrix exponential of a square matrix.
            /// </summary>
            /// <param name="m">The input square matrix.</param>
            /// <returns>The matrix exponential of the input matrix.</returns>
            public static Matrix<double> Exponential(this Matrix<double> m)
            {
                if (m.RowCount != m.ColumnCount)
                    throw new ArgumentException("Matrix should be square");

                Matrix<double> exp_m = null;

                // If m is diagonal, then the matrix exponential is the pointwise exponential
                if (m.IsDiagonal())
                    exp_m = DenseMatrix.OfDiagonalVector(m.Diagonal().PointwiseExp());
                else
                {
                    // If m is not diagonal, attempt to diagonalize it
                    bool diagonalization_failed = !m.IsSymmetric();
                    if (!diagonalization_failed)
                    {
                        try
                        {
                            var evd = m.Evd();
                            Matrix expD = DenseMatrix.OfDiagonalVector(evd.D.Diagonal().PointwiseExp());
                            exp_m = evd.EigenVectors * expD * evd.EigenVectors.Inverse();
                        }
                        catch
                        {
                            diagonalization_failed = true;
                        }
                    }

                    if (diagonalization_failed)
                    {
                        // Use Padé approximation method as a last resort
                        int p = 5; // Order of Padé approximation

                        // Normalize the matrix to reduce round-off errors
                        double k = 0;
                        double mNorm = m.L1Norm();
                        if (mNorm > 0.5)
                        {
                            k = Math.Ceiling(Math.Log(mNorm) / Math.Log(2.0));
                            m = m / Math.Pow(2.0, k);
                        }

                        Matrix<double> N = DenseMatrix.CreateIdentity(m.RowCount);
                        Matrix<double> D = DenseMatrix.CreateIdentity(m.RowCount);
                        Matrix<double> m_pow_j = m;

                        int q = p; // Symmetric approximation
                        for (int j = 1; j <= Math.Max(p, q); j++)
                        {
                            if (j > 1)
                                m_pow_j = m_pow_j * m;
                            if (j <= p)
                                N = N + SpecialFunctions.Factorial(p + q - j) * SpecialFunctions.Factorial(p) / SpecialFunctions.Factorial(p + q) / SpecialFunctions.Factorial(j) / SpecialFunctions.Factorial(p - j) * m_pow_j;
                            if (j <= q)
                                D = D + Math.Pow(-1.0, j) * SpecialFunctions.Factorial(p + q - j) * SpecialFunctions.Factorial(q) / SpecialFunctions.Factorial(p + q) / SpecialFunctions.Factorial(j) / SpecialFunctions.Factorial(q - j) * m_pow_j;
                        }

                        // Calculate inv(D)*N with LU decomposition
                        exp_m = D.LU().Solve(N);

                        // Denormalize if necessary
                        if (k > 0)
                        {
                            for (int i = 0; i < k; i++)
                            {
                                exp_m = exp_m * exp_m;
                            }
                        }
                    }
                }
                return exp_m;
            }

            /// <summary>
            /// Checks if a matrix is diagonal.
            /// </summary>
            /// <param name="m">The input square matrix.</param>
            /// <returns>True if the matrix is diagonal, otherwise false.</returns>
            public static bool IsDiagonal(this Matrix<double> m)
            {
                if (m.RowCount != m.ColumnCount)
                    throw new ArgumentException("Matrix should be square");

                for (int i = 0; i < m.ColumnCount; i++)
                    for (int j = i + 1; j < m.ColumnCount; j++)
                    {
                        if (m[i, j] != 0.0) return false;
                        if (m[j, i] != 0.0) return false;
                    }

                return true;
            }

            /// <summary>
            /// Computes the matrix logarithm of a square matrix.
            /// </summary>
            /// <param name="m">The input square matrix.</param>
            /// <returns>The matrix logarithm of the input matrix.</returns>
            public static Matrix<double> Logarithm(this Matrix<double> m)
            {
                if (m.RowCount != m.ColumnCount)
                    throw new ArgumentException("Matrix should be square");

                for (int i = 0; i < m.RowCount; i++)
                {
                    for (int j = 0; j < m.ColumnCount; j++)
                    {
                        if (double.IsNaN(m[i, j]))
                        {
                            return null;
                        }
                    }
                }

                var evd = m.Evd();
                var D = evd.D;
                var V = evd.EigenVectors;
                var logD = DenseMatrix.OfMatrix(D);

                for (int i = 0; i < logD.RowCount; i++)
                {
                    logD[i, i] = Math.Log(logD[i, i]);
                }

                return V * logD * V.Inverse();
            }

            public static bool IsApproximatelyEqual(this Matrix<double> matrix1, Matrix<double> matrix2, double tolerance)
            {
                if (matrix1.RowCount != matrix2.RowCount || matrix1.ColumnCount != matrix2.ColumnCount)
                {
                    return false;
                }

                for (int i = 0; i < matrix1.RowCount; i++)
                {
                    for (int j = 0; j < matrix1.ColumnCount; j++)
                    {
                        if (Math.Abs(matrix1[i, j] - matrix2[i, j]) > tolerance)
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
        }

    }
}
