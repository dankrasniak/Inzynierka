using System;
using System.Collections.Generic;

namespace Inzynierka.Model.Model.PoliReactor
{
    public class PoliReactor : IModel
    {
        private readonly List<Double> _initialState = new List<Double>() {0.01, 0.01, 0.01, 0.01}; //{ 5.506774, 0.132906, 0.0019752, 49.38182 };
        private readonly Double _setpoint = 25000.5;
        private readonly Double _commandingValue = 0.0;

        public PoliReactor(List<Property> properties)
        {
            _setpoint = Convert.ToDouble(properties.Find(p => p.Name.Equals("Setpoint")).Value);
            _initialState = new List<double>()
            {
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V1")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V2")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V3")).Value),
                Convert.ToDouble(properties.Find(p => p.Name.Equals("S0V4")).Value)
            };
            _commandingValue = Convert.ToDouble(properties.Find(p => p.Name.Equals("CommandingValue")).Value);
        }

        public List<Double> StateFunction2(List<Double> stateVariables, List<Double> controlVariables)
        {
            var result = new List<Double>(stateVariables);

            var Cm = stateVariables[0];
            var C1 = stateVariables[1];
            var D0 = stateVariables[2];
            var D1 = stateVariables[3];

            var P0 = Math.Sqrt((2 * 0.58 * 0.10225 * C1) / (1.093 * Math.Pow(10, 11) + 1.3281 * Math.Pow(10, 10)));


            /* New Cm */result[0] = (-(2.4952 * Math.Pow(10, 6) + 2.4522 * Math.Pow(10, 3)) * Cm * P0 + 1.0 * (6.0 - Cm) / 0.1);

            /* New C1 */result[1] = (-0.10225 * C1 + (controlVariables[0] * 8.0 - 1.0 * C1) / 0.1);

            /* New D0 */result[2] = ((0.5 * 1.3281 * Math.Pow(10, 10) + 1.093 * Math.Pow(10, 11)) * Math.Pow(P0, 2) + 2.4522 * Math.Pow(10, 3) * Cm * P0 - (1.0 * D0) / 0.1);

            /* New D1 */result[3] = (100.12 * (2.4952 * Math.Pow(10, 6) + 2.4522 * Math.Pow(10, 3)) * Cm * P0 - (1.0 * D1) / 0.1);

            return result;
        }

        public List<Double> StateFunction(List<Double> stateVariables, List<Double> controlVariables)
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

            /* New C1 */ result[1] = (-kI * C1 + (controlVariables[0] * Clin - F * C1) / V); // (controlVariables[0] * 80.0  -10.10225 * C1);

            /* New D0 */ result[2] = ((0.5*kTc + kTd)*P0*P0 + kfm*Cm*P0 - F*D0/V);

            /* New D1 */ result[3] = (Mm*(kp+kfm)*Cm*P0 - F*D1/V);

            return result;
        }

        public Double GetValue(List<Double> stateVariables)
        {
            return stateVariables[3]/stateVariables[2];
        }

        public Double GetDiscrepancy(List<Double> stateVariables)
        {
            return Math.Abs(_setpoint - GetValue(stateVariables)); // TODO Czy musi być Abs
        }

        public Boolean IsFirstBetter(List<Double> state1, List<Double> state2) // TODO Delete
        {
            return (Math.Abs(_setpoint - GetValue(state1)) <
                Math.Abs(_setpoint - GetValue(state2))) ;
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
            return true; //GetValue(state) >= 0; TODO
        }
    }
}