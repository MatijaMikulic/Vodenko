namespace ModelProvider.Models
{
    public class MathematicalModel
    {
        public double[][] A { get; set; } = new double[][] {
            new double[] { -0.5747, 0.0, 0.0},
            new double[] { 0.0022, -0.0879, 0.0848},
            new double[] {0.0, 0.0848, -0.0958 }
        };
        public double[][] B { get; set; } = new double[][] {
            new double[] {1.8689},
            new double[] {0.0},
            new double[] {0.0}
        };
        public double[][] C { get; set; } = new double[][] {
            new double[] {0.0, 0.0 ,1.0}
        };
        public double[][] D { get; set; } = new double[][] {
            new double[] {0.0}
        };

        public MathematicalModel()
        {
        }
        public MathematicalModel(double[][] A, double[][] B)
        {
            ValidateMatrix(A, "A");
            ValidateMatrix(B, "B");
            this.A = A;
            this.B = B;
        }
        private void ValidateMatrix(double[][] matrix, string name)
        {
            if (matrix == null)
            {
                throw new ArgumentNullException(name, $"{name} cannot be null");
            }

            int rowCount = matrix.Length;
            if (rowCount == 0)
            {
                throw new ArgumentException($"{name} must have at least one row", name);
            }

            int colCount = matrix[0].Length;
            for (int i = 1; i < rowCount; i++)
            {
                if (matrix[i].Length != colCount)
                {
                    throw new ArgumentException($"{name} must have the same number of columns in every row", name);
                }
            }
        }

    }
}
