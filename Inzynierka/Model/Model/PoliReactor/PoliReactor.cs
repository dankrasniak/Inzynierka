using System;
using System.Collections.Generic;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Probability;

namespace Inzynierka.Model.Model.PoliReactor
{
    public class PoliReactor : IModel
    {
        private readonly List<Double> _initialState = new List<Double>() {0.01, 0.01, 0.01, 0.01}; //{ 5.506774, 0.132906, 0.0019752, 49.38182 };
        private Double _setpoint = 25000.5;
        private readonly Double _commandingValue;
        private double H_STEP_SIZE;
        private double _externalDiscretization = 0.001;
        private double _internalDiscretization = 0.0001;
        private double[] _minActionValues = new double[1];
        private double[] _maxActionValues = new double[1];

        public PoliReactor(List<Property> properties)
        {
            H_STEP_SIZE = Convert.ToDouble(properties.Find(p => p.Name.Equals("H_STEP")).Value);
            _setpoint = Convert.ToDouble(properties.Find(p => p.Name.Equals("Setpoint")).Value);
            _initialState = new List<double>()
            {
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V1")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V2")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V3")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V4")).Value)
            };
            _commandingValue = Convert.ToDouble(properties.Find(p => p.Name.Equals("CommandingValue")).Value);
            _minActionValues[0] = Convert.ToDouble(properties.Find(p => p.Name.Equals("MinActionValue")).Value);
            _maxActionValues[0] = Convert.ToDouble(properties.Find(p => p.Name.Equals("MaxActionValue")).Value);
        }

        private List<Double> InnerStateFunction(List<Double> stateVariables, List<Double> controlVariables)
        {
            var result = new List<Double>(stateVariables);

            var Cm = stateVariables[0];
            var C1 = stateVariables[1];
            var D0 = stateVariables[2];
            var D1 = stateVariables[3];

            #region Variables

            var kTc = 1.3281*Math.Pow(10, 10);
            var kTd = 1.093*Math.Pow(10, 11);
            var kI = 1.0225*Math.Pow(10, -1);
            var kp = 2.4952*Math.Pow(10, 6);
            var kfm = 2.4522*Math.Pow(10, 3);
            var f = 0.58;
            var F = 1.0;
            var V = 0.1;
            var Clin = 8.0;
            var Mm = 100.12;
            var Cmin = 6.0;

            #endregion Variables

            var P0 = Math.Sqrt((2 * f * kI * C1) / (kTd + kTc));

            /* New Cm */ result[0] = (-(kp + kfm)*Cm*P0 + F*(Cmin - Cm)/V);

            /* New C1 */ result[1] = (-kI * C1 + (controlVariables[0] * Clin - F * C1) / V);

            /* New D0 */ result[2] = ((0.5*kTc + kTd)*P0*P0 + kfm*Cm*P0 - F*D0/V);

            /* New D1 */ result[3] = (Mm*(kp+kfm)*Cm*P0 - F*D1/V);

            return result;
        }

        public List<Double> StateFunction(List<Double> stateVariables, List<Double> controlVariables)
        {
            for (double t = 0; t < _externalDiscretization; t += H_STEP_SIZE)
            {
                H_STEP_SIZE = Math.Min(_internalDiscretization, _externalDiscretization - t);
                stateVariables = RungeKuttha(stateVariables, controlVariables);
            }
            return stateVariables;
        } 

        private List<Double> RungeKuttha(List<Double> stateVariables, List<Double> controlVariables)
        {
            var k1 = InnerStateFunction(stateVariables, controlVariables);
            var k2 = InnerStateFunction(Add(stateVariables, Multiply(0.5 * H_STEP_SIZE, k1)), controlVariables);
            var k3 = InnerStateFunction(Add(stateVariables, Multiply(0.5 * H_STEP_SIZE, k2)), controlVariables);
            var k4 = InnerStateFunction(Add(stateVariables, Multiply(H_STEP_SIZE, k3)), controlVariables);

            return Add(stateVariables, Multiply(H_STEP_SIZE / 6.0, (Add(k1, Add(Multiply(2, k2), Add(Multiply(2, k3), k4))))));
        }

        private static List<Double> Multiply(Double variable, List<Double> vector)
        {
            var result = new List<Double>(vector);
            var ARRAY_SIZE = vector.Count;

            for (int i = 0; i < ARRAY_SIZE; ++i)
            {
                result[i] = result[i] * variable;
            }

            return result;
        }

        private static List<Double> Add(List<Double> a1, List<Double> a2)
        {
            var result = new List<Double>(a1);
            var ARRAY_SIZE = a1.Count;

            for (int i = 0; i < ARRAY_SIZE; ++i)
            {
                result[i] = result[i] + a2[i];
            }

            return result;
        }

        public void SetDiscretizations(double externalDiscretization, double internalDiscretization)
        {
            _internalDiscretization = internalDiscretization;
            _externalDiscretization = externalDiscretization;
        }

        public List<Double> GetValue(List<Double> stateVariables)
        {
            return new List<double> {stateVariables[3]/stateVariables[2]};
        }

        public Double GetDiscrepancy(List<Double> stateVariables)
        {
            return Math.Abs(_setpoint - GetValue(stateVariables)[0]);
        }

        public List<Double> GenerateControlVariables()
        {
            return new List<Double>() { _commandingValue }; // { 0.016783 };
        }

        public List<Double> GetInitialState()
        {
            return _initialState;
        }

        public Boolean IsStateAcceptable(List<Double> state)
        {
            return GetValue(state)[0] >= 0; //TODO
        }

        public Double GetReward(List<Double> state)
        {
            //return (-1) * Math.Pow(GetDiscrepancy(state) / 20000, 2); // TODO
            return (-1) * Math.Pow(GetDiscrepancy(state) / _setpoint, 2); // TODO
        }

        public List<Double> MeddleWithGoalAndStartingState()
        {
            var rand = new ASampler();
            _setpoint = rand.Next(28000 - 20000) + 20000.5;
            var state = GetInitialState();
            while (GetValue(state)[0] < 25000.5 || GetValue(state)[0] > 38000.0)
            {
                state[0] = rand.NextDouble() * 5.506774;
                state[1] = rand.NextDouble() * 0.132906;
                state[2] = rand.NextDouble() * 0.0019752;
                state[3] = rand.NextDouble() * 49.38182;
            }
            return state;
        }

        public double[] MinActionValues()
        {
            return _minActionValues;
        }

        public double[] MaxActionValues()
        {
            return _maxActionValues;
        }

        public double Penalty()
        {
            return -20;
        }

        public List<Double> TurnStateToNNAcceptable(List<Double> state)
        {
            return new List<double>(){
                state[0],
                state[1],
                state[2],
                state[3],
                Math.Abs(_setpoint - GetValue(state)[0]),
                _setpoint - GetValue(state)[0]
            };
        }

        public List<double> GetStateValuesStandardDeviationNN()
        {
            //return new List<double>() {1.0, 1.0, 1.0, 1.0, 1.0}; //5.506774, 0.132906, 0.0019752, 49.38182, 25000.5 };
            return new List<double>() { 0.4, 0.1, 0.001, 6, 3000, 3000 }; //5.506774, 0.132906, 0.0019752, 49.38182, 25000.5 };
        }

        public List<double> GetStateValuesAverageNN()
        {
            //return new List<double>() { 0.0, 0.0, 0.0, 0.0, 0.0 };
            return new List<double>() { 6, 0.2, 0.002, 48, 0, 0};
        }

        public List<Double> GetAdditionalValues()
        {
            return new List<double>() { _setpoint, 25000 };
        }
    }
}