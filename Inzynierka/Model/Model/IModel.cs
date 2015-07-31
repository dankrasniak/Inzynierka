using System;
using System.Collections.Generic;

namespace Inzynierka.Model.Model
{
    public interface IModel
    {
        List<Double> StateFunction(List<Double> stateVariables, List<Double> controlVariables);

        List<Double> GenerateControlVariables();

        List<Double> GetValue(List<Double> stateVariables);

        // By how much the current output value is different from the setpoint.
        Double GetDiscrepancy(List<Double> stateVariables);

        List<Double> GetInitialState();

        Boolean IsStateAcceptable(List<Double> state);

        Double GetReward(List<Double> state);

        void SetDiscretizations(double externalDiscretization, double internalDiscretization);

        List<Double> MeddleWithGoalAndStartingState();

        double[] MinActionValues();

        double[] MaxActionValues();

        double Penalty();

        List<Double> TurnStateToNNAcceptable(List<Double> state);
    }
}