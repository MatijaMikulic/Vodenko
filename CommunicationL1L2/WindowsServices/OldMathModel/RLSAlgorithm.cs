using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

/// <summary>
/// Class implementing the Recursive Least Squares (RLS) algorithm for parameter estimation.
/// </summary>
public class RLSAlgorithm
{
    private Matrix<double> P;
    public Vector<double> theta;
    private double lambda;
    private double alpha;
    private double alphaWeight;

    /// <summary>
    /// Initializes a new instance of the RLSAlgorithm class.
    /// </summary>
    /// <param name="initialTheta">Initial parameter estimates.</param>
    /// <param name="initialPValue">Initial value for the covariance matrix.</param>
    /// <param name="lambda">Forgetting factor for the RLS algorithm.</param>
    /// <param name="alpha">Parameter influencing the regression matrix formation.</param>
    /// <param name="alphaWeight">Weight for combining regression matrices from model and real values.</param>
    public RLSAlgorithm(double[] initialTheta, double initialPValue, double lambda, double alpha, double alphaWeight)
    {
        this.theta = Vector<double>.Build.DenseOfArray(initialTheta);
        this.P = Matrix<double>.Build.DenseIdentity(initialTheta.Length) * initialPValue;
        this.lambda = lambda;
        this.alpha = alpha;
        this.alphaWeight = alphaWeight;
    }

    /// <summary>
    /// Sets new parameter estimates.
    /// </summary>
    /// <param name="newTheta">New parameter estimates.</param>
    /// <exception cref="ArgumentException">Thrown when the length of newTheta does not match the current parameter vector length.</exception>
    public void SetTheta(double[] newTheta)
    {
        if (newTheta.Length != theta.Count)
        {
            throw new ArgumentException($"Theta should have {theta.Count} elements.");
        }
        this.theta = Vector<double>.Build.DenseOfArray(newTheta);
    }

    /// <summary>
    /// Updates the parameter estimates based on the current observation and input.
    /// </summary>
    /// <param name="yK">Current output vector.</param>
    /// <param name="yHatKPrev">Previous predicted output vector.</param>
    /// <param name="u">Current input value.</param>
    /// <param name="yPrev">Previous real output vector.</param>
    /// <returns>Updated predicted output vector.</returns>
    public Vector<double> Update(Vector<double> yK, Vector<double> yHatKPrev, double u, Vector<double> yPrev)
    {
        // Form regression matrix \(\Phi(k)\) using previous estimated values
        var phiKModel = Matrix<double>.Build.DenseOfArray(new double[,] {
            { yHatKPrev[0], 0, 0, 0, 0, 0, u },
            { 0, yHatKPrev[0], yHatKPrev[1], yHatKPrev[2], 0, 0, 0 },
            { 0, 0, 0, 0, yHatKPrev[1], yHatKPrev[2], 0 }
        });

        // Form regression matrix \(\Phi(k)\) using previous real values
        var phiKReal = Matrix<double>.Build.DenseOfArray(new double[,] {
            { yPrev[0], 0, 0, 0, 0, 0, u },
            { 0, yPrev[0], yPrev[1], yPrev[2], 0, 0, 0 },
            { 0, 0, 0, 0, yPrev[1], yPrev[2], 0 }
        });

        // Combine regression matrices
        var phiK = alphaWeight * phiKModel + (1 - alphaWeight) * phiKReal;

        // Prediction
        var yHatK = theta * phiK.Transpose(); // yHatK will be of dimension 3x1

        // Error vector
        var eK = yK - yHatK; // eK will be of dimension 3x1

        // Update covariance matrix
        var Kk = (P * phiK.Transpose()) * (lambda * Matrix<double>.Build.DenseIdentity(phiK.RowCount) + phiK * P * phiK.Transpose()).Inverse();
        // Kk will be of dimension 7x3

        // Update parameters
        theta = theta + eK * Kk.Transpose(); // theta will be of dimension 7x1

        // Update covariance matrix
        P = (P - Kk * phiK * P) / lambda; // P will remain of dimension 7x7

        return yHatK; // yHatK will be of dimension 3x1
    }

    /// <summary>
    /// Gets the estimated system matrices A and B.
    /// </summary>
    /// <returns>Tuple containing the estimated matrices A_est and B_est.</returns>
    public (Matrix<double> A_est, Matrix<double> B_est) GetEstimatedParameters()
    {
        var A_est = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { theta[0], 0, 0 },
            { theta[1], theta[2], theta[3] },
            { 0, theta[4], theta[5] }
        });

        var B_est = Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { theta[6] },
            { 0 },
            { 0 }
        });

        return (A_est, B_est);
    }
}
