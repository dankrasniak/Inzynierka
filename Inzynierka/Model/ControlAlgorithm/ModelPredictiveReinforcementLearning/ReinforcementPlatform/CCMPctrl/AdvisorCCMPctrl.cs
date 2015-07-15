using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Computing;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Neural;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.Probability;
using Inzynierka.Model.Logger;
using Inzynierka.Model.Model;
using System;
using System.Collections;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning.ReinforcementPlatform.CCMPctrl
{
    public class Advisor : IAlgorithm //: Reinforcement.CCAdvisor
    {
        protected MLPerceptron2 V;    // aproksymator funkcji V
        protected Vector Vval;        // wektor zwracany przez ten aproksymator 
        protected Vector Vgrad;       // gradient na wyjściu aproksymatora V
        protected double betaV;       // parametr kroku dla tego aproksymatora 
        protected Vector VparamGrad; 

        #region parametry 
        protected readonly int HORIZON_SIZE;    // długość horyzontu
        protected readonly int Vsize;           // wielkość sieci neuronowej V
        protected double Gamma;        // dyskonto 
        protected Vector MinAction;    // min sterowania 
        protected Vector MaxAction;    // max sterowania 
        protected const double H_STEP_SIZE = 0.001;
        #endregion 

        protected int TimeIndex;       // indeks czasowy bieżącego zdarzenia 
        protected ArrayList AllVisits; // wszystkie dotychczasowe zdarzenia 
        protected AVisit Visit;        // zdarzenie wylosowane

		protected readonly ASampler Sampler;      // służy do losowania 

        protected Vector State;        // bieżący stan 
        protected Vector[] Actions;    // sterowania na horyzoncie 
        protected Vector[] NextStates; // następne stany na horyzoncie 
        protected double Vest;         // bieżąca ocena sterowań Actions 

        #region Ad. Biblioteka Programowa

        protected readonly IModel _model;
        protected LogIt _logger;
        #endregion

	    public Advisor(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
	    {
            _model = model;

            HORIZON_SIZE = (int) Convert.ToDouble(properties.Find(p => p.Name.Equals("Horizon")).Value);

            //_logger = new LogIt("", loggedValues);

            TimeIndex = 0;
            AllVisits = new ArrayList();
            Sampler = new ASampler();

            StartInState(_model.GetInitialState().ToArray());
            MinAction = new Vector(1, 0.0); // TODO Assess the values
            MaxAction = new Vector(1, 0.1); // TODO Assess the values

	        double[] stateAverage = {0.0, 0.0, 0.0, 0.0};
	        double[] stateStandardDeviation = {1.0, 1.0, 1.0, 1.0};
	        const double discount = 0.8;
	        Vsize = 5;

            Init(stateAverage, stateStandardDeviation, MinAction.Table, Vsize, discount);

	    }

        /// <returns>The value returned by the model.</returns>
        public Double GetValueTMP() 
        {
            var currentAction = _model.GenerateControlVariables().ToArray();
            AdviseAction(ref currentAction);
            ThisHappened(RungeKuttha(State.Table.ToList(), currentAction.ToList()).ToArray());

            return _model.GetValue(State.Table.ToList());
        }

        /// <summary>
        /// Prepares the model for the next epizode. The Approximator stays unchanged.
        /// </summary>
        public void NextEpisode()
        {
            
        }

        // state_av - average value?
        // state_stddev - standard deviation?
        public void Init(double[] state_av, double[] state_stddev,
            double[] action_min, int vsize, double discount)
        {
            int StateDim = state_av.GetLength(0);
            int ActionDim = action_min.GetLength(0);

            #region budowa sieci neuronowej

            V = new MLPerceptron2();
            V.Build(StateDim, CellType.Arcustangent, new int[] { vsize, 1 });
            V.SetInputDescription(new Vector(state_av), new Vector(state_stddev));
            V.InitWeights(1.0/Math.Sqrt(vsize+1));
            Vval = new Vector(1);
            Vgrad = new Vector(1);

            #endregion koniec budowy sieci neuronowej

            Gamma = discount;

            Actions = new Vector[HORIZON_SIZE];
            NextStates = new Vector[HORIZON_SIZE];
            for (int i = 0; i < HORIZON_SIZE; i++)
            {
                Actions[i] = new Vector(ActionDim, 0.0);
                NextStates[i] = new Vector(StateDim, 0.0); 
            }
        }

        public void SetParameters(double v_stepsize)
        {
            betaV = v_stepsize; 
        }

		// początek epizodu sterowania 
		public void StartInState(double[] state) 
		{
            State = new Vector(state); 
		}

		//  wiemy w jakim system jest stanie 
        // i produkujemy sterowanie dla niego 
		public void AdviseAction(ref double[] action)
		{
            Vest = CalculateStateValue(State, Actions, ref NextStates); 
            for (int i = 0; i < 10; i++) // kilka poprawek
                AdjustActions(State, 1, ref Actions, ref NextStates, ref Vest);

            AllVisits.Add(new AVisit(TimeIndex++, State, Actions, NextStates)); 
            action = Actions[0].Table; 
		}

		//  sterowanie zostało wykonane, 
        // system przeszedł do następnego stanu (należy zignorować reward) 
		public void ThisHappened(double[] nextState)
		{
            if (ModelIsStateAcceptable(new Vector(nextState)))
            {
                State = new Vector(nextState);
                for (int i = 0; i < HORIZON_SIZE - 1; i++)
                {
                    Actions[i] = Actions[i + 1];
                    NextStates[i] = NextStates[i + 1];
                }
            }

            // dokonujemy poprawek historycznych sterowań i funkcji V
            for (int k = 0; k < 10; k++) // kilka kolejnych stanów początkowych do poprawienia 
            {
                // Get random state & calculate its value
                Visit = (AVisit)AllVisits[Sampler.Next(AllVisits.Count - 1)]; 
                Vest = CalculateStateValue(Visit.State, Visit.Actions, ref Visit.NextStates); 

                // Try to Adjust its actions
                for (int i = 0; i < 10; i++) // kilka poprawek
                    AdjustActions(Visit.State, 1, ref Visit.Actions, ref Visit.NextStates, ref Vest);

                // TODO EXPLAIN
                // Get the state value from the approximator & add calculated discrepancy to its weights(NN).
                V.Approximate(Visit.State, ref Vval);
                Vgrad[0] = Vval[0] - Vest;
                V.BackPropagateGradient(Vgrad, ref VparamGrad);
                
                V.AddToWeights(VparamGrad, -betaV);
            }
        }

        #region Model
        protected bool ModelIsStateAcceptable(Vector state)
        {
            return _model.IsStateAcceptable(state.Table.ToList());
        }

        protected double ModelReward(Vector next_state)
        {
            if (ModelIsStateAcceptable(next_state))
                return (-1) * Math.Pow(_model.GetDiscrepancy(next_state.Table.ToList()), 2);
            return 0; 
        }

        protected Vector ModelNextState(Vector state, Vector action)
        {
            //throw new NotImplementedException("ModelNextState");
            return new Vector(RungeKuttha(state.Table.ToList(), action.Table.ToList()).ToArray());
        }

        #region Predictive Control

        private List<Double> RungeKuttha(List<Double> stateVariables, List<Double> controlVariables)
        {
            var k1 = _model.StateFunction(stateVariables, controlVariables);
            var k2 = _model.StateFunction(Add(stateVariables, Multiply(0.5 * H_STEP_SIZE, k1)), controlVariables);
            var k3 = _model.StateFunction(Add(stateVariables, Multiply(0.5 * H_STEP_SIZE, k2)), controlVariables);
            var k4 = _model.StateFunction(Add(stateVariables, Multiply(H_STEP_SIZE, k3)), controlVariables);

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
        #endregion
        #endregion 

        protected double CalculateStateValue(Vector state, Vector[] actions, ref Vector[] next_states)
        {
            // TODO h? Actions.length? HorizonSize?
            int h = actions.Length; 
            next_states = new Vector[h];
            var stateTmp = state;
            double value = 0;
            double gammai = 1; 

            // Get the last state with the current actions
            for (int i = 0; i < h; i++)
            {
                next_states[i] = stateTmp = ModelNextState(stateTmp, actions[i]);
                if (!ModelIsStateAcceptable(stateTmp))
                    return value; 
                value += gammai * ModelReward(stateTmp);
                gammai *= Gamma; 
            }

            V.Approximate(stateTmp, ref Vval);
            value += gammai * Vval[0];
            
            return value; 
        }

        /// <summary>Iteracja algorytmu (1+1) </summary>
        /// <returns>Zwraca, czy nastąpiła poprawa </returns>
        protected bool AdjustActions(Vector state, double sigma, ref Vector[] currentActions, ref Vector[] currentNextStates, ref double stateValue)
        {
            var modifiedActions = new Vector[HORIZON_SIZE];
            var newNextStates = new Vector[HORIZON_SIZE];

            #region Modify the Action vector

            for (int i = 0; i < HORIZON_SIZE; i++)
            {
                modifiedActions[i] = currentActions[i].Clone();
                // < MinAction ; _action + Rand ; MaxAction >
                for (int j = 0; j < modifiedActions[i].Dimension; j++) // MinAction and MaxAction never set
                {
                    modifiedActions[i][j] = Math.Min(
                        Math.Max(MinAction[j], modifiedActions[i][j] + 0.1*Sampler.SampleFromNormal(0, sigma)/4.0),
                        MaxAction[j]);
                }
            }
            #endregion

            // If the value of the state with new action vector is bigger, replace the previous action vector
            #region if bigger, replace the previous action vector

            double stateNewValue = CalculateStateValue(state, modifiedActions, ref newNextStates);
            if (stateNewValue > stateValue)
            {
                stateValue = stateNewValue; 
                currentActions = modifiedActions;
                currentNextStates = newNextStates;
                return true;
            }

            #endregion

            return false; 
        }
    }
}

