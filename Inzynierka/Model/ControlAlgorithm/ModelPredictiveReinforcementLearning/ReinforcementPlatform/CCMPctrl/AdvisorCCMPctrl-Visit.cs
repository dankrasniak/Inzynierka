using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Computing;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.ReinforcementPlatform.CCMPctrl
{
    #region Visit
    public class AVisit
    {
        //	phase 1: only the state is known
        public int TimeIndex;
        public Vector State;

        //	phase 2: the action is known
        public Vector[] Actions;
        public Vector[] NextStates;

        public AVisit(int time_index, Vector state, Vector[] actions, Vector[] next_states)
        {
            TimeIndex = time_index;
            State = state.Clone();

            Actions = new Vector[actions.Length];
            for (int i = 0; i < actions.Length; i++)
                Actions[i] = actions[i].Clone();

            NextStates = new Vector[next_states.Length];
            for (int i = 0; i < next_states.Length; i++)
                NextStates[i] = next_states[i].Clone();
        }
    }
    #endregion
}
