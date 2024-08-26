using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra.Double;
using MathModelOnline.Utilities.MatrixExponential;

namespace MathModelOnline.Utilities
{
    public static class DiscreteContinuousConverter
    {
        public static (Matrix<double> Ad, Matrix<double> Bd) CalculateDiscreteFormZOH(Matrix<double> A, Matrix<double> B, double dt)
        {
            // Compute matrix exponential e^(A*Ts)
            var expATs = A.Multiply(dt).Exponential();

            // Compute the integral of e^(A*tau) from 0 to Ts
            var I = DenseMatrix.CreateIdentity(A.RowCount);
            var integralExpAt = expATs.Subtract(I).Multiply(A.Inverse());

            // Compute discrete-time matrices
            var Ad = expATs;
            var Bd = integralExpAt.Multiply(B);

            return (Ad, Bd);
        }

        public static (Matrix<double> A_c, Matrix<double> B_c) CalculateContinuousFormFromDiscrete(Matrix<double> Ad, Matrix<double> Bd, double dt)
        {
            var logAd = Ad.Logarithm();
            var Ac = logAd.Divide(dt);

            // Step 2: Form the augmented matrix
            int n = Ad.RowCount;
            int m = Bd.ColumnCount;

            var augmentedMatrix = DenseMatrix.Create(n + m, n + m, 0.0);

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmentedMatrix[i, j] = Ad[i, j];
                }
            }

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    augmentedMatrix[i, n + j] = Bd[i, j];
                }
            }

            for (int i = 0; i < m; i++)
            {
                augmentedMatrix[n + i, n + i] = 1.0;
            }

            var logAugmentedMatrix = augmentedMatrix.Logarithm();

            var AcExtracted = DenseMatrix.Create(n, n, (i, j) => logAugmentedMatrix[i, j] / dt);
            var BcExtracted = DenseMatrix.Create(n, m, (i, j) => logAugmentedMatrix[i, j + n] / dt);

            return (AcExtracted, BcExtracted);
        }

        /// <summary>
        /// Calculates the time constants of the system given the continuous-time system matrix (Ac).
        /// </summary>
        /// <returns>An array of time constants.</returns>
        public static double[] CalculateTimeConstants(Matrix<double> Ac)
        {
            for (int i = 0; i < Ac.RowCount; i++)
            {
                for (int j = 0; j < Ac.ColumnCount; j++)
                {
                    if (double.IsNaN(Ac[i, j]))
                    {
                        // Return an empty array to indicate error
                        return new double[0];
                    }
                }
            }
            // Compute eigenvalues
            var evd = Ac.Evd();
            var eigenvalues = evd.EigenValues;

            // Calculate time constants (inverse of the real part of eigenvalues)
            var timeConstants = new double[eigenvalues.Count];
            for (int i = 0; i < eigenvalues.Count; i++)
            {
                // Only consider real parts of the eigenvalues
                timeConstants[i] = -1.0 / eigenvalues[i].Real;
            }

            return timeConstants;
        }
    }
}
