using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using NCalc;

namespace MathModelOnline.Model
{
    public enum ModelType
    {
        DISCRETE,
        CONTINUOUS
    }
    public class StateSpaceLinearModel
    {
        private string[,] symbolicA;
        private string[,] symbolicB;
        private string[,] symbolicC;

        public Matrix<double> A { get;  set; }
        public Matrix<double> B { get;  set; }
        public Matrix<double> C { get;  set; }

        public Dictionary<string, double> Variables { get; private set; }
        public Dictionary<string, double> InitialValues { get; private set; }
        public Dictionary<string, double> InitialInputs { get; private set; }

        public ModelType Type { get;  set; }
        public bool IsInitialized { get; set; }

        public StateSpaceLinearModel(string[,] A_, string[,] B_,
                                    string[,] C_, Dictionary<string, double> variables,
                                    Dictionary<string, double> initialValues,
                                    Dictionary<string, double> inputs,
                                    ModelType type)
        {
            symbolicA = A_;
            symbolicB = B_;
            symbolicC = C_;

            Variables = variables;
            InitialValues = initialValues;
            InitialInputs = inputs;

            InitializeMatrices();
            Type = type;
            IsInitialized = true;
        }

        public void Reinitialize(Matrix<double> A_,
                            Matrix<double> B_)
        {
            this.A = A_;
            this.B = B_;
        }

        public StateSpaceLinearModel(Matrix<double> A_,
                                     Matrix<double> B_,
                                     ModelType type)
        {
            A = A_;
            B = B_;
            Type = type;

            // Set C to an identity matrix with the same number of rows as A has columns
            C = Matrix<double>.Build.DenseIdentity(A_.ColumnCount);

            // Initialize initial values and inputs to zero
            InitialValues = new Dictionary<string, double>();
            for (int i = 0; i < A_.ColumnCount; i++)
            {
                InitialValues[$"x{i}"] = 0.0;
            }

            InitialInputs = new Dictionary<string, double>
            {
                { "u0", 0.0 } // Assuming a single input
            };

            IsInitialized = true;
        }

        public StateSpaceLinearModel(Matrix<double> A_,
                                     Matrix<double> B_,
                                     Matrix<double> C_,
                                     Dictionary<string, double> initialValues,
                                     Dictionary<string, double> inputs,
                                     ModelType type)
        {
            A = A_;
            B = B_;
            C = C_;
            InitialValues = initialValues;
            InitialInputs = inputs;
            Type = type;
            IsInitialized = true;

        }

        public StateSpaceLinearModel(StateSpaceLinearModel other)
        {
            // Clone matrices
            this.A = other.A.Clone();
            this.B = other.B.Clone();
            this.C = other.C.Clone();

            // Clone dictionaries
            this.Variables = new Dictionary<string, double>(other.Variables);
            this.InitialValues = new Dictionary<string, double>(other.InitialValues);
            this.InitialInputs = new Dictionary<string, double>(other.InitialInputs);

            // Clone symbolic matrices
            this.symbolicA = (string[,])other.symbolicA.Clone();
            this.symbolicB = (string[,])other.symbolicB.Clone();
            this.symbolicC = (string[,])other.symbolicC.Clone();

            // Copy other properties
            this.Type = other.Type;
            this.IsInitialized = other.IsInitialized;
        }

        public void Reinitialize(Dictionary<string, double> variables,
                                 Dictionary<string, double> initialValues,
                                 Dictionary<string, double> inputs)
        {
            Variables = variables;
            InitialValues = initialValues;
            InitialInputs = inputs;
            InitializeMatrices();
            IsInitialized = true;
        }

        private void InitializeMatrices()
        {
            var combinedVariables = new Dictionary<string, double>(Variables);
            foreach (var kvp in InitialValues)
            {
                combinedVariables[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in InitialInputs)
            {
                combinedVariables[kvp.Key] = kvp.Value;
            }

            A = ParseMatrix(symbolicA, combinedVariables);
            B = ParseMatrix(symbolicB, combinedVariables);
            C = ParseMatrix(symbolicC, combinedVariables);
        }

        private Matrix<double> ParseMatrix(string[,] matrix, Dictionary<string, double> variables)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);

            var numericsMatrix = Matrix<double>.Build.Dense(rows, cols);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    string expression = matrix[i, j];
                    double value = EvaluateExpression(expression, variables);
                    numericsMatrix[i, j] = value;
                }
            }
            return numericsMatrix;
        }

        private double EvaluateExpression(string expression, Dictionary<string, double> variables)
        {
            var expr = new NCalc.Expression(expression);

            foreach (var variable in variables)
            {
                expr.Parameters[variable.Key] = variable.Value;
            }

            var result = expr.Evaluate();

            if (result is int intResult)
            {
                return (double)intResult;
            }
            else if (result is double doubleResult)
            {
                return doubleResult;
            }
            else
            {
                throw new InvalidCastException("The evaluated result is neither an int nor a double.");
            }
        }
    }
}
