using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Probability;

namespace Inzynierka.Model.Model.Pendulum
{
    public class Pendulum : IModel
    {
        public static string Name = "Pendulum";
        public Double _XMAX = 2.4;
        private List<Double> _initialState;
        private Double _setpoint;
        private Double _commandingValue;
        private Double _internalDiscretization;
        private Double _externalDiscretization;
        private double _time = 0.005;
        private double[] _minActionValues = new double[1];
        private double[] _maxActionValues = new double[1];

        public Pendulum(List<Property> properties)
        {
            _setpoint = Convert.ToDouble(properties.Find(p => p.Name.Equals("Setpoint")).Value);
            _initialState = new List<double>()
            {
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V1")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V2")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V3")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V4")).Value)
            };
            _commandingValue = Convert.ToDouble(properties.Find(p => p.Name.Equals("CommandingValue")).Value); // TODO
            _minActionValues[0] = Convert.ToDouble(properties.Find(p => p.Name.Equals("MinActionValue")).Value);
            _maxActionValues[0] = Convert.ToDouble(properties.Find(p => p.Name.Equals("MaxActionValues")).Value);
        }

        public List<Double> StateFunction2(List<Double> stateVariables, List<Double> controlVariables)
        {
            /*stateVariables[0] = // TODO Pochodne
                ((((-1) * (Double)controlVariables[0] - 0.1 * 0.5 * Math.Pow(O, 2) * Math.Sin(O) + 0.0005 * Math.Sign((Double)stateVariables[1])) / (1.1)) *
                 Math.Cos(O) + 9.81*Math.Sin(O) + (0.0005*O)/(0.1*0.5))
                /
                (0.5*(4/3 - (0.1*Math.Pow(Math.Cos(O), 2))/(1.1)));

            stateVariables[1] = (((Double) controlVariables[0] + 0.1*0.5*(O*O*Math.Sin(O) - O*Math.Cos(O)) -
                                 0.0005*Math.Sign((Double)stateVariables[1]))
                                /1.1);*/
            var O = (Double) stateVariables[0];
            var Op = (Double) stateVariables[2];
            var Opp = 0.0;
            var x = (Double) stateVariables[1];
            var xp = (Double) stateVariables[3];
            var F = (Double) controlVariables[0];
            var g = 9.81;
            var mc = 1;
            var mp = 0.1;
            var l = 0.5;
            var mic = 0.0005;
            var mip = 0.000002;

            stateVariables[0] = (((-F - mp*l*Math.Pow(Op, 2)*Math.Sin(O) + mic*Math.Sign(xp))/(mc + mp))*Math.Cos(O) +
                                 g*Math.Sin(O) + (mip*Op)/(mp*l))
                                / (l*(4/3 - (mp*Math.Pow(Math.Cos(O), 2)))/(mc + mp));

            stateVariables[1] = (F + mp*l*(Op*Op*Math.Sin(O) - Opp*Math.Cos(O)) - mic*Math.Sign(xp))/(mc + mp);

            throw new NotImplementedException(); // TODO
            return stateVariables;
        }

        public List<Double> StateFunction3(List<Double> stateVariables, List<Double> controlVariables)
        {
            const double h = 0.005;

            var OldX = stateVariables[0];
            var OldO = stateVariables[1];
            var OldXp = stateVariables[2];
            var OldOp = stateVariables[3];

            var F = controlVariables[0];
            var g = 9.81;
            var mc = 1;
            var mp = 0.1;
            var l = 0.5;
            var mic = 0.0005;
            var mip = 0.000002;

            var Opp = (Double)(((-F - mp * l * Math.Pow(OldOp, 2) * Math.Sin(OldO * Math.PI / 180.0) + mic * Math.Sign(OldXp)) / (mc + mp)) * Math.Cos(OldO * Math.PI / 180.0) +
                                 g * Math.Sin(OldO * Math.PI / 180.0) + (mip * OldOp) / (mp * l))
                                / (l * (4 / 3 - (mp * Math.Pow(Math.Cos(OldO * Math.PI / 180.0), 2)) / (mc + mp)));

            var Op = OldOp + h * Opp * 180 / Math.PI;
            var O = OldO + h * Op + 0.5 * h * h * Opp;

            var xpp = (Double)(F + mp * l * (OldOp * OldOp * Math.Sin(OldO * Math.PI / 180.0) - Opp * Math.Cos(OldO * Math.PI / 180)) - mic * Math.Sign(OldXp)) / (mc + mp); // TODO Old or New O?

            var xp = OldXp + h * xpp;
            var x = OldX + h * xp + 0.5 * h * h * xpp;


            stateVariables[0] = x;
            stateVariables[1] = O;
            stateVariables[2] = xp;
            stateVariables[3] = Op;

            return stateVariables;
        }
        public List<Double> StateFunctionPart(List<Double> stateVariables, List<Double> controlVariables)
        {
            double h = _time;

            var OldX = stateVariables[0];
            var OldO = stateVariables[1];
            var OldXp = stateVariables[2];
            var OldOp = stateVariables[3];

            var F = controlVariables[0];
            var g = 9.81;
            var mc = 1;
            var mp = 0.1;
            var l = 0.5;
            var mic = 0.0005;
            var mip = 0.000002;

            var Opp = (Double)(((-F - mp * l * Math.Pow(OldOp, 2) * Math.Sin(OldO) + mic * Math.Sign(OldXp)) / (mc + mp)) * Math.Cos(OldO) +
                                 g * Math.Sin(OldO) + (mip * OldOp) / (mp * l))
                                / (l * (4 / 3 - (mp * Math.Pow(Math.Cos(OldO), 2)) / (mc + mp)));

            var O = OldO + h * OldOp + 0.5 * h * h * Opp;
            var Op = OldOp + h*Opp;

            var xpp = (Double)(F + mp * l * (OldOp * OldOp * Math.Sin(OldO) - Opp * Math.Cos(OldO)) - mic * Math.Sign(OldXp)) / (mc + mp);

            var x = OldX + h * OldXp + 0.5 * h * h * xpp;
            var xp = OldXp + h * xpp;


            stateVariables[0] = x;
            stateVariables[1] = O;
            stateVariables[2] = xp;
            stateVariables[3] = Op;

            return stateVariables;
        }

        public List<Double> StateFunction(List<Double> stateVariables, List<Double> controlVariables)
        {
            for (double t = 0; t < _externalDiscretization; t += _internalDiscretization)
            {
                _time = Math.Min(_internalDiscretization, _externalDiscretization - t);
                stateVariables = StateFunctionPart(stateVariables, controlVariables);
            }
            return stateVariables;
        }

        public void SetDiscretizations(double externalDiscretization, double internalDiscretization)
        {
            _externalDiscretization = externalDiscretization;
            _internalDiscretization = internalDiscretization;
        }

        public List<Double> GetValue(List<Double> stateVariables)
        {
            return new List<double>() {stateVariables[0], stateVariables[1]};
        }

        public Double GetDiscrepancy(List<Double> stateVariables)
        {
            return Math.Abs(_setpoint - Math.PI - Math.Abs(Math.PI - GetValue(stateVariables)[1])); // TODO
        }

        public Boolean IsFirstBetter(List<Double> state1, List<Double> state2)
        {
            throw new NotImplementedException(); // TODO
            return true;
        }

        public List<Double> GenerateControlVariables()
        {
            return new List<Double>() { _commandingValue };
        }

        public List<Double> GetInitialState()
        {
            return _initialState;
        }

        public Boolean IsStateAcceptable(List<Double> state)
        {
            return (state[0] >= -_XMAX) && (state[0] <= _XMAX);
        }

        public Double GetReward(List<Double> state)
        {
            return Math.Cos(state[1]);// - Math.Abs(state[0]/15) - Math.Abs(state[2]/30) - Math.Abs(state[3]/30);
        }

        public List<Double> MeddleWithGoalAndStartingState()
        {
            var state = GetInitialState();
            state[1] = new ASampler().NextDouble() * 2 * Math.PI;
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
    }
}