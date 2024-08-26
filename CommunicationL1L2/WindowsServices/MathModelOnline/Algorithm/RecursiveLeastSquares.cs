using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathModelOnline.Algorithm
{
    public class RecursiveLeastSquares
    {
        private Matrix<double> P;
        public Vector<double> Theta { get; set; }
        private double lambda;
        private double alpha;
        private double alphaWeight;
        private Func<Vector<double>,Vector<double>, Matrix<double>> regressionMatrixFunc;
        private RegressionMatrixConfig config;

        public RecursiveLeastSquares(
            Vector<double> initialTheta,
            double initialPValue,
            double lambda,
            double alpha,
            double alphaWeight,
            Func<Vector<double>,Vector<double>, Matrix<double>> regressionMatrixFunc,
            RegressionMatrixConfig config)
        {
            this.Theta = initialTheta;
            this.P = Matrix<double>.Build.DenseIdentity(initialTheta.Count) * initialPValue;
            this.lambda = lambda;
            this.alpha = alpha;
            this.alphaWeight = alphaWeight;
            this.regressionMatrixFunc = regressionMatrixFunc;
            this.config = config;
        }

        public Vector<double> Update(Vector<double> xK, Vector<double> xHatKPrev, Vector<double> xPrev, Vector<double> uPrev)
        {
            // Form regression matrix using previous estimated values
            var phiKModel = regressionMatrixFunc(xHatKPrev, uPrev);

            // Form regression matrix using previous real values (from sensor)
            var phiKReal = regressionMatrixFunc(xPrev, uPrev);

            // Combine regression matrices
            var phiK = alphaWeight * phiKModel + (1 - alphaWeight) * phiKReal;

            // Prediction
            var xHatK = Theta * phiK.Transpose();

            // Error vector
            var eK = xK - xHatK;

            // Update covariance matrix
            var Kk = (P * phiK.Transpose()) * (lambda * Matrix<double>.Build.DenseIdentity(phiK.RowCount) + phiK * P * phiK.Transpose()).Inverse();

            // Update theta parameters
            Theta = Theta + eK * Kk.Transpose();

            // Update covariance matrix
            P = (P - Kk * phiK * P) / lambda;

            return xHatK;
        }

        public (Matrix<double> A_est, Matrix<double> B_est) GetEstimatedDiscreteSSModel()
        {
            int numRows = config.RegressionMatrix.Count;

            if (numRows == 0)
            {
                // Return empty matrices if there are no rows
                var emptyMatrix = Matrix<double>.Build.Dense(0, 0);
                return (emptyMatrix, emptyMatrix);
            }

            int numCols = config.RegressionMatrix[0].Count;

            var A_est = Matrix<double>.Build.Dense(numRows, numRows);

            // Determine the number of inputs (U terms)
            var uniqueInputs = config.RegressionMatrix
                .SelectMany(row => row)
                .Where(term => term.Term == TermType.U)
                .Select(term => term.Index)  
                .Distinct()
                .ToList();

            int numInputs = uniqueInputs.Count;

            var B_est = Matrix<double>.Build.Dense(numRows, numInputs);

            for (int i = 0; i < config.RegressionMatrix.Count; i++) // 0,1,2
            {
                for (int j = 0; j < config.RegressionMatrix[0].Count; j++) // 0,1,2,3,4,5,6
                {
                    var term = config.RegressionMatrix[i][j];
                    if(term.Term == TermType.X)
                    {
                        A_est[i, term.Index] = Theta[j];
                    }
                    else if(term.Term == TermType.U)
                    {
                        B_est[i, term.Index] = Theta[j]; 
                    }
                }
            }

            return (A_est,B_est);
        }
    }
}
