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
        protected double BetaV;       // parametr kroku dla tego aproksymatora 
        protected Vector VparamGrad; 

        #region parametry 
        protected readonly int HORIZON_SIZE;    // długość horyzontu
        protected readonly int Vsize;           // wielkość sieci neuronowej V
        protected double Gamma;        // dyskonto 
        protected readonly Vector MinAction;    // min sterowania 
        protected readonly Vector MaxAction;    // max sterowania 
        protected const double H_STEP_SIZE = 0.001;
        #endregion 

        protected int TimeIndex;       // indeks czasowy bieżącego zdarzenia 
        protected readonly ArrayList AllVisits; // wszystkie dotychczasowe zdarzenia 
        protected AVisit Visit;        // zdarzenie wylosowane

		protected readonly ASampler Sampler;      // służy do losowania 

        protected Vector State;        // bieżący stan 
        protected Vector[] Actions;    // sterowania na horyzoncie 
        protected Vector[] NextStates; // następne stany na horyzoncie 
        protected double Vest;         // bieżąca ocena sterowań Actions 

        #region Ad. Biblioteka Programowa

        protected readonly IModel _model;
        protected LogIt _logger;
        protected double StartingActionsValue;
        protected readonly double Sigma;
        #endregion

	    public Advisor(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
	    {
            _model = model;

            HORIZON_SIZE = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("Horizon")).Value);
            Vsize = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("Neural Network Layers Number")).Value);
            BetaV = Convert.ToDouble(properties.Find(p => p.Name.Equals("Scalar")).Value);
            Gamma = Convert.ToDouble(properties.Find(p => p.Name.Equals("Discount")).Value);
            StartingActionsValue = Convert.ToDouble(properties.Find(p => p.Name.Equals("CommandingValue")).Value);
	        Sigma = Convert.ToDouble(properties.Find(p => p.Name.Equals("Sigma")).Value);

            _logger = new LogIt("", loggedValues);

            TimeIndex = 0;
            AllVisits = new ArrayList();
            Sampler = new ASampler();

            StartInState(_model.GetInitialState().ToArray());
            MinAction = new Vector(1, -10); // TODO Assess the values
            MaxAction = new Vector(1, 10); // TODO Assess the values

            double[] stateAverage = { 0.0, 0.0, 0.0, 0.0 };// TODO Assess the values
            double[] stateStandardDeviation = { 1.0, 1.0, 1.0, 1.0 };// TODO Assess the values

            Init(stateAverage, stateStandardDeviation, MinAction.Table, Vsize);
	    }

        /// <returns>The value returned by the model.</returns>
        public List<Double> GetValueTMP() 
        {
            var currentAction = _model.GenerateControlVariables().ToArray();

            AdviseAction(ref currentAction);
            //ThisHappened(RungeKuttha(State.Table.ToList(), currentAction.ToList()).ToArray());
            ThisHappened(_model.StateFunction(State.Table.ToList(), currentAction.ToList()).ToArray());

            return _model.GetValue(State.Table.ToList());
        }

        /// <summary>Prepares the model for the next epizode. The Approximator stays unchanged.</summary>
        public void NextEpisode()
        {
            TimeIndex = 0;
            StartInState(_model.GetInitialState().ToArray());
            Actions = new Vector[HORIZON_SIZE];
            NextStates = new Vector[HORIZON_SIZE];
            for (int i = 0; i < HORIZON_SIZE; i++)
            {
                Actions[i] = new Vector(1, StartingActionsValue); // TODO
                NextStates[i] = new Vector(4, 0.0); // TODO
            }
        }

        public void Init(double[] stateAv, double[] stateStddev, double[] actionMin, int vsize)
        {
            int stateDim = stateAv.GetLength(0);
            int actionDim = actionMin.GetLength(0);


            #region budowa sieci neuronowej

            V = new MLPerceptron2();
            V.Build(stateDim, CellType.Arcustangent, new int[] { vsize, 1 });
            V.SetInputDescription(new Vector(stateAv), new Vector(stateStddev));
            V.InitWeights(1.0/Math.Sqrt(vsize+1));
            Vval = new Vector(1);
            Vgrad = new Vector(1);

            #endregion koniec budowy sieci neuronowej


            Actions = new Vector[HORIZON_SIZE];
            NextStates = new Vector[HORIZON_SIZE];
            for (int i = 0; i < HORIZON_SIZE; i++)
            {
                Actions[i] = new Vector(actionDim, StartingActionsValue);
                NextStates[i] = new Vector(stateDim, 0.0); 
            }
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
                AdjustActions(State, Sigma / (Sigma + i), ref Actions, ref NextStates, ref Vest);// / (Sigma + i)

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
            else
            {
                NextEpisode();
            }

            // dokonujemy poprawek historycznych sterowań i funkcji V
            for (int k = 0; k < 10; k++) // kilka kolejnych stanów początkowych do poprawienia 
            {
                // Get random state & calculate its value
                Visit = (AVisit)AllVisits[Sampler.Next(AllVisits.Count - 1)]; 
                Vest = CalculateStateValue(Visit.State, Visit.Actions, ref Visit.NextStates); 

                // Try to Adjust its actions
                for (int i = 0; i < 10; i++) // kilka poprawek
                    AdjustActions(Visit.State, Sigma / (Sigma + i), ref Visit.Actions, ref Visit.NextStates, ref Vest);//  / (Sigma + i)

                // Get the state value from the approximator & add calculated discrepancy to its weights(NN).
                V.Approximate(Visit.State, ref Vval);
                Vgrad[0] = Vval[0] - Vest;
                V.BackPropagateGradient(Vgrad, ref VparamGrad);
                
                V.AddToWeights(VparamGrad, -BetaV);
            }
        }

        #region Model
        protected bool ModelIsStateAcceptable(Vector state)
        {
            return _model.IsStateAcceptable(state.Table.ToList());
        }

        protected double ModelReward(Vector nextState)
        {
            if (ModelIsStateAcceptable(nextState))
                return (-1) * Math.Pow(_model.GetDiscrepancy(nextState.Table.ToList()), 2);
            return 0; 
        }

        protected Vector ModelNextState(Vector state, Vector action)
        {
            //return new Vector(RungeKuttha(state.Table.ToList(), action.Table.ToList()).ToArray());
            return  new Vector(_model.StateFunction(state.Table.ToList(), action.Table.ToList()).ToArray());
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
                {
                    //NextEpisode();
                    return value - 1000000; // TODO
                }
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
                for (int j = 0; j < modifiedActions[i].Dimension; j++)
                {
                    //modifiedActions[i][j] = Math.Min(
                    //    Math.Max(MinAction[j], modifiedActions[i][j] + 0.1*Sampler.SampleFromNormal(0, sigma)/4.0),
                    //    MaxAction[j]);
                    modifiedActions[i][j] = Math.Min(
                        Math.Max(MinAction[j], modifiedActions[i][j] + Sampler.SampleFromNormal(0, sigma)),
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

