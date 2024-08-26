using MathModelOnline.Model;
using MathModelOnline.Utilities;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathModelOnline.Algorithm
{
    public class DCModelCalculations
    {
        public Matrix<double> Ad { get; private set; }
        public Matrix<double> Bd { get; private set; }
        public double dt { get; private set; }

        public Vector<double> x_current { get; private set; }
        public Vector<double> u_current { get; private set; }

        private StateSpaceLinearModel stateSpaceModel;

        public DCModelCalculations(StateSpaceLinearModel model, double dt)
        {
            stateSpaceModel = model;
            this.dt = dt;
            InitializeFromModel();
        }

        public void Reinitialize(StateSpaceLinearModel model)
        {
            stateSpaceModel.A = model.A;
            stateSpaceModel.B = model.B;
            stateSpaceModel.C = model.C;
            stateSpaceModel.Type = model.Type;
            
        }

        public void SetInletFLowState(double flow)
        {
            x_current[0]=flow;
        }

        public void SetWaterLevelTank1(double waterLevel)
        {
            x_current[1]=waterLevel;
        }

        public void SetWaterLevelTank2(double waterLevel)
        {
            x_current[2]=waterLevel;
        }

        private void InitializeFromModel()
        {
            x_current = Vector<double>.Build.Dense(stateSpaceModel.InitialValues.Count, 0.0);
            u_current = Vector<double>.Build.Dense(stateSpaceModel.B.ColumnCount, 0.0);
        }

        /// <summary>
        /// Sets the sample time (dt) for the model.
        /// </summary>
        /// <param name="T">Sample time to be set.</param>
        public void SetSampleTime(double T)
        {
            dt = T;
        }

        /// <summary>
        /// Sets the sample time (dt) for the model.
        /// </summary>
        /// <param name="currentInput">Set the current input.</param>
        public void SetCurrentInput(Vector<double> currentInput)
        {
            u_current = currentInput;
        }


        /// <summary>
        /// Calculates the discrete form matrices (Ad, Bd) using Zero-Order Hold (ZOH) method.
        /// </summary>
        public void CalculateDiscreteFormZOH()
        {
            if (stateSpaceModel.Type == ModelType.CONTINUOUS)
            {
                var (A_d, B_d) = DiscreteContinuousConverter.CalculateDiscreteFormZOH(stateSpaceModel.A, stateSpaceModel.B, dt);
                Ad = A_d;
                Bd = B_d;
            }
            else if (stateSpaceModel.Type == ModelType.DISCRETE)
            {
                Ad = stateSpaceModel.A;
                Bd = stateSpaceModel.B;
            }
        }

        /// <summary>
        /// Calculates the time constants of the system given the continuous-time system matrix (Ac).
        /// </summary>
        /// <returns>An array of time constants.</returns>
        public double[] CalculateTimeConstants()
        {
            var Ac = stateSpaceModel.A;
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

        public Vector<double> CalculateNextState()
        {
            var x_next = Ad * x_current + Bd * u_current;

            //x_next[0] = Ad[0, 0] * x_current[0] + Ad[0, 1] * x_current[1] + Ad[0, 2] * x_current[2] + Bd[1,0] * u_current[0];
            //x_next[1] = Ad[1, 0] * x_current[0] + Ad[1, 1] * x_current[1] + Ad[1, 2] * x_current[2] + Bd[2,0] * 0;
            //x_next[2] = Ad[2, 0] * x_current[0] + Ad[2, 1] * x_current[1] + Ad[2, 2] * x_current[2] + Bd[3,0] * 0;

            // If inlet flow bigger than max value....
            if (x_next[0] > 239.171)
            {
                x_next[0] = 239.171;
            }

            // Water level tanks can't be smaller than 0 
            if (x_next[1] < 0)
            {
                x_next[1] = 0;
            }
            if(x_next[2] < 0)
            {
                x_next[2] = 0;   
            }

            x_current = x_next;

            return x_next;
        }
    }

}
