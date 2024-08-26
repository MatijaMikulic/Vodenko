using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathModelOnline.Utilities.MatrixExponential;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;

namespace MathModelOnline.Utilities
{
    public class StateSpaceModel
    {
        public bool IsInitialized { get; set; } 

        private const double K1 = 0.0582;
        private const double K2 = 0.1232;
        private const double K3 = 0.0702;
        private const double K4 = 0.0203 - 0.007;
        private const  double Tv = 1.74;
        private const  double Kv = 3.2519 + 0.0;
        private const  double Km = 6.25;
        private const  double A1 = 450;
        private const  double A2 = 450;
        private const  double g = 981;
        private double K1d;
        private double K2d;
        private double K12A;
        private double K12B;
        private double Ki;
        private const  double x = -12;

        public double[,] A {  get; private set; }
        public double[,] B { get; private set; }
        public double[,] C { get; private set; }
        public double qu0 { get; private set; }
        public double h10 { get; private set; }
        public double h20 { get; private set; }
        public double xv0 { get; private set; }



        public StateSpaceModel()
        {

            K1d = K4 * A1 / Math.Sqrt(2 * g);
            K2d = K4 * A1 / Math.Sqrt(2 * g);
            K12A = K3 * A1 / Math.Sqrt(2 * g);
            K12B = K2 * A1 / Math.Sqrt(2 * g);
            Ki = K1 * A1 / Math.Sqrt(2 * g);
        }

        public void InitializeStateSpaceModel(double h20,double h10, double qu0,double xv0)
        {
            IsInitialized = true;

            this.h20 = h20;
            this.h10 = h10;
            this.qu0 = qu0;
            this.xv0 = xv0;


            float result = (float)Math.Sqrt(2 * g * (h10 - h20));
            double C1 = (K12A + K12B) * g / result;
            double C2 = K1d * g / (float)Math.Sqrt(2 * g * h10);
            double C3 = (K2d) * g / ((float)Math.Sqrt(2 * g * h20))  + Ki *g / ((float)Math.Sqrt(2 * g * (h20-x)));


            double[,] A_ = {
            { -1.0 / Tv, 0, 0 },
            { 1.0 / A1, -(C1 + C2) / A1, C1 / A1 },
            { 0, C1 / A1, -(C1 + C3) / A1 }
            };
            A = A_;

            double[,] B_ = {
            { Kv/ Tv },
            { 0 },
            { 0 }
            };
            B = B_;

            double[,] C_ = {
            { 0,0,1},
            };
            C = C_;
        }

        public void UpdateStateSpaceModel(double[,] A_, double[,] B_)
        {
            A = A_;
            B = B_;
        }

        //public double[] CalculateTimeConstants()
        //{
        //    // Convert the 2D array to Math.NET Numerics matrix
        //    var matrixA = Matrix.Build.DenseOfArray(A);

        //    // Compute eigenvalues
        //    var evd = matrixA.Evd();
        //    var eigenvalues = evd.EigenValues;

        //    // Calculate time constants (inverse of the real part of eigenvalues)
        //    var timeConstants = new double[eigenvalues.Count];
        //    for (int i = 0; i < eigenvalues.Count; i++)
        //    {
        //        // Only consider real parts of the eigenvalues
        //        timeConstants[i] = -1.0 / eigenvalues[i].Real;
        //    }

        //    return timeConstants;
        //}


    }
}
