using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathModelOnline.Model
{
    public class NonLinearModel
    {
        public double PreviousQu { get; private set; }
        public double PreviousH1 { get; private set; }
        public double PreviousH2 { get; private set; }
        public double PreviousInput { get; private set; }

        public double dt { get; private set; }

        public NonLinearModel()
        {
            PreviousQu = 0;
            PreviousH1 = 0;
            PreviousH2 = 0;
            PreviousInput = 0;
            dt = 0.1;
        }

        public NonLinearModel(double previousQu, double previousH1, double previousH2, double previousInput, double dt)
        {
            PreviousQu = previousQu;
            PreviousH1 = previousH1;
            PreviousH2 = previousH2;
            PreviousInput = previousInput;
            this.dt = dt;
        }

        public void Reinitialize(double previousQu, double previousH1, double previousH2, double previousInput)
        {
            PreviousQu = previousQu;
            PreviousH1 = previousH1;
            PreviousH2 = previousH2;
            PreviousInput = previousInput;
        }

        public void SetInput(double previousInput)
        {
            PreviousInput = previousInput;
        }

        public void SetSampleTime(double T)
        {
            dt = T;
        }

        /// <summary>
        /// Calculates the nonlinear model states using the Runge-Kutta 4th order method.
        /// </summary>
        /// <returns>The updated states as a tuple (qu, h1, h2).</returns>
        public (double, double, double) CalculateNonLinearModelStates()
        {
            double xv = PreviousInput;
            double qu = PreviousQu;
            double h1 = PreviousH1;
            double h2 = PreviousH2;

            (double k1_h1, double k1_h2, double k1_qu) = Derivatives(h1, h2, qu, xv);
            (double k2_h1, double k2_h2, double k2_qu) = Derivatives(h1 + dt / 2 * k1_h1, h2 + dt / 2 * k1_h2, qu + dt / 2 * k1_qu, xv);
            (double k3_h1, double k3_h2, double k3_qu) = Derivatives(h1 + dt / 2 * k2_h1, h2 + dt / 2 * k2_h2, qu + dt / 2 * k2_qu, xv);
            (double k4_h1, double k4_h2, double k4_qu) = Derivatives(h1 + dt * k3_h1, h2 + dt * k3_h2, qu + dt * k3_qu, xv);

            h1 += dt / 6 * (k1_h1 + 2 * k2_h1 + 2 * k3_h1 + k4_h1);
            h2 += dt / 6 * (k1_h2 + 2 * k2_h2 + 2 * k3_h2 + k4_h2);
            qu += dt / 6 * (k1_qu + 2 * k2_qu + 2 * k3_qu + k4_qu);


            PreviousInput = xv;
            PreviousQu = qu;
            PreviousH1 = h1;
            PreviousH2 = h2;
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
        private (double, double, double) Derivatives(double h1, double h2, double qu, double xv)
        {
            double K1 = 0.0582;
            double K2 = 0.1232;
            double K3 = 0.0702;
            double K4 = 0.0203 - 0.006;

            double Tv = 1.74;
            double Kv = 3.1519 + 0.04;
            double Km = 6.25;
            double A1 = 450;
            double A2 = 450;
            double g = 981;
            double K1d = K4 * A1 / Math.Sqrt(2 * g);
            double K2d = K4 * A1 / Math.Sqrt(2 * g);
            double K12A = K3 * A1 / Math.Sqrt(2 * g);
            double K12B = K2 * A1 / Math.Sqrt(2 * g);
            double Ki = K1 * A1 / Math.Sqrt(2 * g);
            double x = 2.2;

            // Ensure h1 and h2 are within valid range to avoid negative square root arguments
            if (h1 < 0)
            {
                h1 = 0;
            }
            if (h2 < x)
            {
                h2 = x;
            }
            if (h1 <= h2)
            {
                h1 = h2 + 0.01;
            }

            double dh1_dt = (qu - (K12A + K12B) * Math.Sqrt(2 * g * Math.Abs(h1 - h2)) - K1d * Math.Sqrt(2 * g * h1)) / A1;
            double dh2_dt = ((K12A + K12B) * Math.Sqrt(2 * g * Math.Abs(h1 - h2)) - Ki * Math.Sqrt(2 * g * Math.Abs(h2 - x)) - K2d * Math.Sqrt(2 * g * h2)) / A1;
            double dqu_dt = (Kv * xv - qu) / Tv;
            return (dh1_dt, dh2_dt, dqu_dt);
        }
    }
}
