using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MathModelOnline.Algorithm
{
    public class RegressionMatrixBuilder
    {
        private readonly RegressionMatrixConfig _config;

        public RegressionMatrixBuilder(RegressionMatrixConfig config)
        {
            _config = config;
        }

        public Matrix<double> BuildRegressionMatrix(Vector<double> x, Vector<double> u)
        {
            Matrix<double> result = Matrix<double>.Build.Dense(_config.RegressionMatrix.Count, _config.RegressionMatrix[0].Count);
            // Print the dimensions of the vectors x and u
            //Console.WriteLine($"Dimensions of vector x: {x.Count}");
            //Console.WriteLine($"Dimensions of vector u: {u.Count}");
            for (int i = 0; i < _config.RegressionMatrix.Count; i++)
            {
                for(int j = 0; j < _config.RegressionMatrix[0].Count; j++)
                {
                    var term = _config.RegressionMatrix[i][j];
                    //Console.WriteLine($"{term.Term},{term.Index}");
                    result[i,j] = EvaluateTerm(x,u,term);
                }
            }

            return result;
        }

        private double EvaluateTerm(Vector<double> x, Vector<double> u, (TermType Term, int Index) term)
        {
            // Print the term type and index being evaluated
            //Console.WriteLine($"Evaluating term: {term.Term}, Index: {term.Index}");

            if (term.Term == TermType.X)
            {
                if (term.Index >= x.Count)
                {
                    throw new ArgumentOutOfRangeException($"Index {term.Index} is out of range for vector x of size {x.Count}");
                }
                return x[term.Index];
            }
            else if (term.Term == TermType.U)
            {
                if (term.Index >= u.Count)
                {
                    throw new ArgumentOutOfRangeException($"Index {term.Index} is out of range for vector u of size {u.Count}");
                }
                return u[term.Index];
            }
            else if (term.Term == TermType.Zero)
            {
                return 0;
            }
            throw new ArgumentException($"Invalid term: {term}");
        }
    }
}
