using System.Collections.Generic;
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
        protected int HORIZON_SIZE;    // długość horyzontu
        protected int Vsize;           // wielkość sieci neuronowej V
        protected double Gamma;        // dyskonto 
        protected Vector MinAction;    // min sterowania 
        protected Vector MaxAction;    // max sterowania 
        #endregion 

        protected int TimeIndex;       // indeks czasowy bieżącego zdarzenia 
        protected ArrayList AllVisits; // wszystkie dotychczasowe zdarzenia 
        protected AVisit Visit;        // zdarzenie wylosowane

		protected ASampler Sampler;      // służy do losowania 

        protected Vector State;        // bieżący stan 
        protected Vector[] Actions;    // sterowania na horyzoncie 
        protected Vector[] NextStates; // następne stany na horyzoncie 
        protected double Vest;         // bieżąca ocena sterowań Actions 

        #region Ad. Biblioteka Programowa

        private IModel _model;
        private LogIt _logger;
        #endregion

        public Advisor()
		{
			TimeIndex = 0;
			AllVisits = new ArrayList(); 
			Sampler = new ASampler(); 

            throw new NotImplementedException();
		}

	    public Advisor(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
	    {
            _model = model;

            HORIZON_SIZE = (int) Convert.ToDouble(properties.Find(p => p.Name.Equals("Horizon")).Value);

            TimeIndex = 0;
            AllVisits = new ArrayList();
            Sampler = new ASampler();

            State = new Vector(_model.GetInitialState().ToArray());
            MinAction = new Vector(1, 0.0); // TODO Assess the values
            MaxAction = new Vector(1, 1000); // TODO Assess the values

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

//            HORIZON_SIZE = 10;  // TODO powinno przyjść z zewnątrz 
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

		public string CCName
		{
			get { return "MPctrl"; }
		}

		// początek epizodu sterowania 
		public void CCStartInState(double[] state) 
		{
            State = new Vector(state); 
		}

		//  wiemy w jakim system jest stanie 
        // i produkujemy sterowanie dla niego 
		public void CCAdviseAction(ref double[] action)
		{
            Vest = CalculateStateValue(State, Actions, ref NextStates); 
            for (int i = 0; i < 10; i++) // kilka poprawek
                AdjustActions(State, 1, ref Actions, ref NextStates, ref Vest);

            AllVisits.Add(new AVisit(TimeIndex++, State, Actions, NextStates)); 
            action = Actions[0].Table; 
		}

		//  sterowanie zostało wykonane, 
        // system przeszedł do następnego stanu (należy zignorować reward) 
		public void CCThisHappened(double[] nextState)
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
                Vgrad[0] = Vval[0] - Vest; // TODO Czemu tylko [0]???
                V.BackPropagateGradient(Vgrad, ref VparamGrad);
                
                V.AddToWeights(VparamGrad, -betaV); // TODO zwróci błąd?
            }
        }

        #region Model - na razie zaślepki, wszystko uzależnione od modelu obiektu 
        protected bool ModelIsStateAcceptable(Vector state)
        {
            throw new NotImplementedException("ModelIsStateAcceptable");
            return true; 
        }

        protected double ModelReward(Vector action, Vector next_state)
        {
            if (ModelIsStateAcceptable(next_state))
                return 1;
            else
                return 0; 
        }

        protected Vector ModelNextState(Vector state, Vector action)
        {
            throw new NotImplementedException("ModelNextState");
            return new Vector(state.Dimension, 0.0); 
        }
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
                next_states[i] = stateTmp = ModelNextState(stateTmp, Actions[i]);
                if (!ModelIsStateAcceptable(stateTmp))
                    return value; 
                value += gammai * ModelReward(actions[i], stateTmp);
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

