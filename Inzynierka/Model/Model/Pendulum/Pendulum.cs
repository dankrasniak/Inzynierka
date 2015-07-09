using System;
using System.Collections.Generic;
using System.Runtime.Remoting;

namespace Inzynierka.Model.Model.Pendulum
{
    public class Pendulum : IModel
    {
        public static string Name = "Pendulum";
        private List<Double> _initialState = new List<Double>() { 0.0, 0.0 };
        private Double _setpoint = 0.0;
        private Double _commandingValue = 0.0;

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
            _commandingValue = (double)properties.Find(p => p.Name.Equals("CommandingValue")).Value;
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

        public List<Double> StateFunction(List<Double> stateVariables, List<Double> controlVariables)
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

            var Opp = (Double)(((-F - mp * l * Math.Pow(OldOp, 2) * Math.Sin(OldO) + mic * Math.Sign(OldXp)) / (mc + mp)) * Math.Cos(OldO) +
                                 g * Math.Sin(OldO) + (mip * OldOp) / (mp * l))
                                / (l * (4 / 3 - (mp * Math.Pow(Math.Cos(OldO), 2))) / (mc + mp));

            var Op = OldOp + h * Opp;
            var O = OldO + h * Op + 0.5 * h * h * Opp;

            var xpp = (Double)(F + mp * l * (OldOp * OldOp * Math.Sin(OldO) - Opp * Math.Cos(OldO)) - mic * Math.Sign(OldXp)) / (mc + mp); // TODO Old or New O?

            var xp = OldXp + h * xpp;
            var x = OldX + h * xp + 0.5 * h * h * xpp;

            //throw new NotImplementedException(); // TODO
            return stateVariables;
        }

        public Double GetValue(List<Double> stateVariables)
        {
            return stateVariables[1];
        }

        public Double GetDiscrepancy(List<Double> stateVariables)
        {
            return Math.Abs(_setpoint - GetValue(stateVariables));
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
    }
}