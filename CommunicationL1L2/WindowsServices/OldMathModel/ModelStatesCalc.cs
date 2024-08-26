using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MathModelOnline.Utilities.MatrixExponential;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace MathModelOnline.Utilities
{
    public static class ModelStatesCalc
    {
        private static Matrix<double> Ad;
        private static Matrix<double> Bd;
        private static double dt;

        public static double[,] x_current = { { 0 }, { 0 }, { 0 } };  // Example current state
        public static double[,] u_current = { { 0 } };

        /// <summary>
        /// Sets the sample time (dt) for the model.
        /// </summary>
        /// <param name="T">Sample time to be set.</param>
        public static void SetSampleTime(double T)
        {
            dt = T;
        }

        /// <summary>
        /// Calculates the discrete form matrices (Ad, Bd) using Zero-Order Hold (ZOH) method.
        /// </summary>
        /// <param name="A">Continuous-time system matrix A.</param>
        /// <param name="B">Continuous-time input matrix B.</param>
        public static void CalculateDiscreteFormZOH(double[,] A, double[,] B)
        {
            var matrixA = DenseMatrix.OfArray(A);
            var matrixB = DenseMatrix.OfArray(B);

            // Compute matrix exponential e^(A*Ts)
            var expATs = matrixA.Multiply(0.5).Exponential();

            // Compute the integral of e^(A*tau) from 0 to Ts
            var I = DenseMatrix.CreateIdentity(matrixA.RowCount);
            var integralExpAt = expATs.Subtract(I).Multiply(matrixA.Inverse());

            // Compute discrete-time matrices
            Ad = expATs;
            Bd = integralExpAt.Multiply(matrixB);
        }

        /// <summary>
        /// Converts the discrete-time system matrices (Ad, Bd) back to continuous-time form (A_c, B_c).
        /// </summary>
        /// <param name="A_c">Output continuous-time system matrix A_c.</param>
        /// <param name="B_c">Output continuous-time input matrix B_c.</param>
        public static void CalculateContinuousFormFromDiscrete(out double[,] A_c, out double[,] B_c)
        {
            var logAd = Ad.Logarithm();
            var Ac = logAd.Divide(dt);

            // Step 2: Form the augmented matrix
            int n = Ad.RowCount;
            int m = Bd.ColumnCount;

            // Initialize the augmented matrix with appropriate size
            var augmentedMatrix = DenseMatrix.Create(n + m, n + m, 0.0);

            // Copy Ad to the top-left block
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmentedMatrix[i, j] = Ad[i, j];
                }
            }

            // Copy Bd to the top-right block
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    augmentedMatrix[i, n + j] = Bd[i, j];
                }
            }

            // Set the bottom-right block to identity matrix
            for (int i = 0; i < m; i++)
            {
                augmentedMatrix[n + i, n + i] = 1.0;
            }

            // Step 3: Compute the logarithm of the augmented matrix
            var logAugmentedMatrix = augmentedMatrix.Logarithm();

            // Step 4: Extract Ac and Bc from the logarithm manually
            var AcExtracted = DenseMatrix.Create(n, n, (i, j) => logAugmentedMatrix[i, j] / dt);
            var BcExtracted = DenseMatrix.Create(n, m, (i, j) => logAugmentedMatrix[i, j + n] / dt);

            // Convert the results to arrays
            A_c = AcExtracted.ToArray();
            B_c = BcExtracted.ToArray();
        }

        /// <summary>
        /// Calculates the next state of the system using the current state and input.
        /// </summary>
        /// <returns>The next state as a 2D array.</returns>
        public static double[,] CalculateNextState()
        {
            Matrix<double> matX_current = DenseMatrix.OfArray(x_current);
            Matrix<double> matU_current = DenseMatrix.OfArray(u_current);

            // Compute next state using backward Euler formula
            Matrix<double> matX_next = Ad * matX_current + Bd * matU_current;

            // Convert MathNet.Numerics matrix to double[,]
            double[,] x_next = matX_next.ToArray();

            return x_next;
        }

        /// <summary>
        /// Calculates the nonlinear model states using the Runge-Kutta 4th order method.
        /// </summary>
        /// <param name="xv">Input xv.</param>
        /// <param name="qu">State variable qu.</param>
        /// <param name="h1">State variable h1.</param>
        /// <param name="h2">State variable h2.</param>
        /// <returns>The updated states as a tuple (qu, h1, h2).</returns>
        public static (double, double, double) CalculateNonLinearModelStates(double xv, double qu, double h1, double h2)
        {
            if (dt == null)
            {
                throw new ArgumentNullException();
            }
            (double k1_h1, double k1_h2, double k1_qu) = Derivatives(h1, h2, qu, xv);
            (double k2_h1, double k2_h2, double k2_qu) = Derivatives(h1 + dt / 2 * k1_h1, h2 + dt / 2 * k1_h2, qu + dt / 2 * k1_qu, xv);
            (double k3_h1, double k3_h2, double k3_qu) = Derivatives(h1 + dt / 2 * k2_h1, h2 + dt / 2 * k2_h2, qu + dt / 2 * k2_qu, xv);
            (double k4_h1, double k4_h2, double k4_qu) = Derivatives(h1 + dt * k3_h1, h2 + dt * k3_h2, qu + dt * k3_qu, xv);

            h1 += dt / 6 * (k1_h1 + 2 * k2_h1 + 2 * k3_h1 + k4_h1);
            h2 += dt / 6 * (k1_h2 + 2 * k2_h2 + 2 * k3_h2 + k4_h2);
            qu += dt / 6 * (k1_qu + 2 * k2_qu + 2 * k3_qu + k4_qu);

            return (qu, h1, h2);
        }

        /// <summary>
        /// Calculates the derivatives for the nonlinear model states.
        /// </summary>
        /// <param name="h1">State variable h1.</param>
        /// <param name="h2">State variable h2.</param>
        /// <param name="qu">State variable qu.</param>
        /// <param name="xv">Input xv.</param>
        /// <returns>The derivatives as a tuple (dh1_dt, dh2_dt, dqu_dt).</returns>
        private static (double, double, double) Derivatives(double h1, double h2, double qu, double xv)
        {
            double K1 = 0.0582;
            double K2 = 0.1232;
            double K3 = 0.0702;
            double K4 = 0.0203 - 0.007;

            double Tv = 1.74;
            double Kv = 3.2519;
            double Km = 6.25;
            double A1 = 450;
            double A2 = 450;
            double g = 981;
            double K1d = K4 * A1 / Math.Sqrt(2 * g);
            double K2d = K4 * A1 / Math.Sqrt(2 * g);
            double K12A = K3 * A1 / Math.Sqrt(2 * g);
            double K12B = K2 * A1 / Math.Sqrt(2 * g);
            double Ki = K1 * A1 / Math.Sqrt(2 * g);
            double x = 2;

            // Ensure h1 and h2 are within valid range to avoid negative square root arguments
            if (h1 < 0)
            {
                h1 = 0;
            }
            if (h2 < x)
            {
                h2 = x;
            }
            if(h1 < h2)
            {
                h1 = h2 + 0.1;
            }

            double dh1_dt = (qu - (K12A + K12B) * Math.Sqrt(2 * g * Math.Abs(h1 - h2)) - K1d * Math.Sqrt(2 * g * h1)) / A1;
            double dh2_dt = ((K12A + K12B) * Math.Sqrt(2 * g * Math.Abs(h1 - h2)) - Ki * Math.Sqrt(2 * g * Math.Abs(h2 - x)) - K2d * Math.Sqrt(2 * g * h2)) / A1;
            double dqu_dt = (Kv * xv - qu) / Tv;
            return (dh1_dt, dh2_dt, dqu_dt);
        }

        /// <summary>
        /// Converts the discrete-time system matrices (Ad, Bd) to continuous-time form (A_c, B_c) using a different method signature.
        /// </summary>
        /// <param name="A_c">Output continuous-time system matrix A_c.</param>
        /// <param name="B_c">Output continuous-time input matrix B_c.</param>
        /// <param name="Ad">Discrete-time system matrix Ad.</param>
        /// <param name="Bd">Discrete-time input matrix Bd.</param>
        /// <param name="dt_">Time step.</param>
        public static void CalculateContinuousFormFromDiscrete(out double[,] A_c, out double[,] B_c, Matrix<double> Ad, Matrix<double> Bd, double dt_ = 0.5)
        {
            // Calculate the logarithm of Ad
            var logAd = Ad.Logarithm();

            // Divide the logAd matrix by the time step dt_
            var Ac = logAd.Divide(dt_);

            // Step 2: Form the augmented matrix
            int n = Ad.RowCount;
            int m = Bd.ColumnCount;

            // Initialize the augmented matrix with appropriate size
            var augmentedMatrix = DenseMatrix.Create(n + m, n + m, 0.0);

            // Copy Ad to the top-left block
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmentedMatrix[i, j] = Ad[i, j];
                }
            }

            // Copy Bd to the top-right block
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    augmentedMatrix[i, n + j] = Bd[i, j];
                }
            }

            // Set the bottom-right block to identity matrix
            for (int i = 0; i < m; i++)
            {
                augmentedMatrix[n + i, n + i] = 1.0;
            }

            // Step 3: Compute the logarithm of the augmented matrix
            var logAugmentedMatrix = augmentedMatrix.Logarithm();

            // Step 4: Extract Ac and Bc from the logarithm manually
            var AcExtracted = DenseMatrix.Create(n, n, (i, j) => logAugmentedMatrix[i, j] / dt_);
            var BcExtracted = DenseMatrix.Create(n, m, (i, j) => logAugmentedMatrix[i, j + n] / dt_);

            // Convert the results to arrays
            A_c = AcExtracted.ToArray();
            B_c = BcExtracted.ToArray();
        }

        /// <summary>
        /// Calculates the time constants of the system given the continuous-time system matrix (Ac).
        /// </summary>
        /// <param name="Ac">Continuous-time system matrix Ac.</param>
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
