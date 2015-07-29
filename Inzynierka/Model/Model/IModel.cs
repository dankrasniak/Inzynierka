using System;
using System.Collections.Generic;

namespace Inzynierka.Model.Model
{
    public interface IModel
    {
        List<Double> StateFunction(List<Double> stateVariables, List<Double> controlVariables);
        List<Double> GenerateControlVariables();

        // Return the output value.
        List<Double> GetValue(List<Double> stateVariables);

        // By how much the current output value is different from the setpoint.
        Double GetDiscrepancy(List<Double> stateVariables);

        List<Double> GetInitialState();

        Boolean IsStateAcceptable(List<Double> state);

        Boolean IsFirstBetter(List<Double> state1, List<Double> state2); // TODO Delete TMP

        Double GetReward(List<Double> state);

        void SetDiscretizations(double externalDiscretization, double internalDiscretization);
    }
}