using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
        protected readonly int PREDICTION_HORIZON_SIZE;    // długość horyzontu predykcji // TODO
        protected readonly int HORIZON_SIZE;    // długość horyzontu
        protected readonly int Vsize;           // wielkość sieci neuronowej V
        protected double Gamma;                 // dyskonto 
        protected readonly Vector MinAction;    // min sterowania 
        protected readonly Vector MaxAction;    // max sterowania
        protected readonly int TimesToAdjust;   // ile razy próbować poprawić horyzont
        protected readonly int TimesToTeach;    // ile razy uczyć sieć co iterację
        protected readonly int IterationsLimit;    // max czasu na jeden epizod
        protected int TimesToAdjustPastActions; // max itracji na jedną próbe optymalizacji przyszłego sterowania
        #endregion 

        protected int TimeIndex;                // indeks czasowy bieżącego zdarzenia 
        private int _episodeNr;
        protected readonly ArrayList AllVisits; // wszystkie dotychczasowe zdarzenia 
        protected AVisit Visit;                 // zdarzenie wylosowane

		protected readonly ASampler Sampler;      // służy do losowania 

        protected Vector State;        // bieżący stan 
        protected Vector[] Actions;    // sterowania na horyzoncie 
        protected Vector[] NextStates; // następne stany na horyzoncie 
        protected double Vest;         // bieżąca ocena sterowań Actions 
        protected double VestSum;      // suma ocen sterowań na przestrzeni jednego epizodu
        protected double VestAv;       // średnia ocen sterowań na przestrzeni 10 ostatich epizodów
        protected List<double> VestList; // Wartości ocen sterowań na przestrzeni ostatnich 10 epizodów 

        #region Ad. Biblioteka Programowa

        protected readonly IModel _model;
        protected LogIt _logger;
        protected double StartingActionsValue;
        protected readonly double Sigma;
        protected double SigmaMin;
        #endregion

	    public Advisor(IModel model, List<Property> properties, List<LoggedValue> loggedValues)
	    {
            _model = model;

            HORIZON_SIZE = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("Horizon")).Value);
            PREDICTION_HORIZON_SIZE = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("PredictionHorizon")).Value); // TODO
            Vsize = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("Neurons Number")).Value);
            BetaV = Convert.ToDouble(properties.Find(p => p.Name.Equals("BetaV")).Value);
            Gamma = Convert.ToDouble(properties.Find(p => p.Name.Equals("Discount")).Value);
            StartingActionsValue = Convert.ToDouble(properties.Find(p => p.Name.Equals("CommandingValue")).Value);
            Sigma = Convert.ToDouble(properties.Find(p => p.Name.Equals("Sigma")).Value);
            SigmaMin = Convert.ToDouble(properties.Find(p => p.Name.Equals("SigmaMin")).Value);
            TimesToAdjust = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("TimesToAdjust")).Value);
            var externalDiscretization =
                Convert.ToDouble(properties.Find(p => p.Name.Equals("ExternalDiscretization")).Value);
            var internalDiscretization =
                Convert.ToDouble(properties.Find(p => p.Name.Equals("InternalDiscretization")).Value);
	        var TimeLimit = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("TimeLimit")).Value);
            TimesToTeach = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("TimesToTeach")).Value);
            TimesToAdjustPastActions = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("TimesToAdjustPastActions")).Value);

            _logger = new LogIt("", loggedValues);
            _model.SetDiscretizations(externalDiscretization, internalDiscretization);

            TimeIndex = 0;
	        _episodeNr = 0;
            AllVisits = new ArrayList();
            Sampler = new ASampler();
            IterationsLimit = (int) (TimeLimit / externalDiscretization);
	        VestSum = 0;
            VestList = new List<double>(10);

            StartInState(_model.GetInitialState().ToArray());
            MinAction = new Vector(_model.MinActionValues());
            MaxAction = new Vector(_model.MaxActionValues());

            double[] stateAverage = { 0.0, 0.0, 0.0, 0.0, 0.0 };// TODO Assess the values
            double[] stateStandardDeviation = { 2.0, 0.8, 0.8, 3.0, 4.0 };// TODO Assess the values

            Init(stateAverage, stateStandardDeviation, MinAction.Table, Vsize);
	    }

        public void Init(double[] stateAv, double[] stateStddev, double[] actionMin, int vsize)
        {
            int stateDim = stateAv.GetLength(0);
            int actionDim = actionMin.GetLength(0);


            #region budowa sieci neuronowej

            V = new MLPerceptron2();
            V.Build(stateDim, CellType.Arcustangent, new int[] { vsize, 1 });
            V.SetInputDescription(new Vector(stateAv), new Vector(stateStddev));
            V.InitWeights(1.0 / Math.Sqrt(vsize + 1));
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

        /// <returns>The value returned by the model.</returns>
        public Data GetValueTMP() 
        {
            if (TimeIndex > IterationsLimit)
                NextEpisode();
            var currentAction = _model.GenerateControlVariables().ToArray();

            AdviseAction(ref currentAction);
            ThisHappened(_model.StateFunction(State.Table.ToList(), currentAction.ToList()).ToArray());

            return new Data() { Values = _model.GetValue(State.Table.ToList()), IterationNumber = TimeIndex, EpisodeNumber = _episodeNr }; // TODO
        }

        /// <summary>Prepares the model for the next epizode. The Approximator stays unchanged.</summary>
        public void NextEpisode()
        {
            VestSum /= TimeIndex;
            UpdateVestList(VestSum);
            TimeIndex = 0;
            ++_episodeNr;

            StartInState(_model.MeddleWithGoalAndStartingState().ToArray());

            Actions = new Vector[HORIZON_SIZE];
            NextStates = new Vector[HORIZON_SIZE];
            for (int i = 0; i < HORIZON_SIZE; i++)
            {
                Actions[i] = new Vector(1, StartingActionsValue); // TODO
                NextStates[i] = new Vector(4, 0.0); // TODO
            }
        }

        protected void UpdateVestList(double VestSum)
        {
            var listLength = VestList.Count;
            if (listLength < 10)
            {
                VestAv = (VestAv*listLength + VestSum)/(listLength + 1);
                VestList.Add(VestSum);
            }
            else
            {
                VestAv = VestAv - VestList[0]/10 + VestSum/10;
                VestList.RemoveAt(0);
                VestList.Add(VestSum);
            }
            _logger.Log("Średnie Wartości nagród z 10 epizodów", VestAv);
        }

		public void StartInState(double[] state) 
		{
            State = new Vector(state); 
		}

		//  wiemy w jakim system jest stanie 
        // i produkujemy sterowanie dla niego 
		public void AdviseAction(ref double[] action)
		{
            Vest = CalculateStateValue(State, Actions, ref NextStates);
            //for (int i = 0; i < TimesToAdjust; i++)
            //    AdjustActions(State, Sigma, ref Actions, ref NextStates, ref Vest);
            PrepareHorizon(TimesToAdjust, State, ref Actions, ref NextStates, ref Vest);
		    VestSum += Vest;

            AllVisits.Add(new AVisit(TimeIndex++, State, Actions, NextStates)); 
            action = Actions[0].Table; 
		}

		//  sterowanie zostało wykonane, 
        // system przeszedł do następnego stanu (należy zignorować reward) 
		public void ThisHappened(double[] nextState)
		{
            Thread.Sleep(100);
		    var reset = false;
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
                reset = true;
            }

            // dokonujemy poprawek historycznych sterowań i funkcji V
            for (int k = 0; k < TimesToTeach; k++)
            {
                // Get random state & calculate its value
                Visit = (AVisit)AllVisits[Sampler.Next(AllVisits.Count - 1)]; 
                Vest = CalculateStateValue(Visit.State, Visit.Actions, ref Visit.NextStates); 

                // Try to Adjust its actions
                //for (int i = 0; i < TimesToAdjust; i++)
                //    AdjustActions(Visit.State, Sigma, ref Visit.Actions, ref Visit.NextStates, ref Vest);
                PrepareHorizon(TimesToAdjustPastActions, Visit.State, ref Visit.Actions, ref Visit.NextStates, ref Vest);

                // Get the state value from the approximator & add calculated discrepancy to its weights(NN).
                V.Approximate(new Vector(_model.TurnStateToNNAcceptable(Visit.State.Table.ToList()).ToArray()), ref Vval);
                Vgrad[0] = Vval[0] - Vest;
                V.BackPropagateGradient(Vgrad, ref VparamGrad);
                
                V.AddToWeights(VparamGrad, -BetaV);
            }
            if (reset) NextEpisode(); // TODO
		}

        #region Model
        protected bool ModelIsStateAcceptable(Vector state)
        {
            return _model.IsStateAcceptable(state.Table.ToList());
        }

        protected double ModelReward(Vector nextState)
        {
            return ModelIsStateAcceptable(nextState) ? _model.GetReward(nextState.Table.ToList()) : _model.Penalty();
        }

        protected Vector ModelNextState(Vector state, Vector action)
        {
            return new Vector(_model.StateFunction(state.Table.ToList(), action.Table.ToList()).ToArray());
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
                next_states[i] = stateTmp = ModelNextState(stateTmp, actions[i]);
                value += gammai * ModelReward(stateTmp);
                if (!ModelIsStateAcceptable(stateTmp))
                {
                    for (int j = i+1; j < h; ++j)
                    {
                        next_states[j] = new Vector(4, 0.0); // TODO
                    }
                    return value; // TODO
                }
                
                gammai *= Gamma; 
            }

            V.Approximate(new Vector(_model.TurnStateToNNAcceptable(stateTmp.Table.ToList()).ToArray()), ref Vval);
            value += gammai * Vval[0];
            
            return value; 
        }

        protected int Phi;
        protected int IterationsNrOpti;
        private readonly int M = 10;
        private const double C1 = 0.82;
        private const double C2 = 1.2;

        protected void PrepareHorizon(int iterationLimit, Vector state, ref Vector[] currentActions, ref Vector[] currentNextStates, ref double stateValue)
        {
            Phi = 0;
            IterationsNrOpti = 0;
            var sigma = Sigma;

            while (IterationsNrOpti < iterationLimit || sigma < SigmaMin)
            {
                ++IterationsNrOpti;
                AdjustActions(state, ref sigma, ref currentActions, ref currentNextStates, ref stateValue);
            }
        }

        protected void UpdateSigma(ref double sigma)
        {
            if (IterationsNrOpti % M != 0)
                return;

            if ((double)Phi / M < 0.2)
            {
                sigma *= C1;
            }
            else if ((double)Phi / M > 0.2)
            {
                sigma *= C2;
            }
            Phi = 0; // == 0.2
        }

        /// <summary>Iteracja algorytmu (1+1) </summary>
        /// <returns>Zwraca, czy nastąpiła poprawa </returns>
        protected void AdjustActions(Vector state, ref double sigma, ref Vector[] currentActions, ref Vector[] currentNextStates, ref double stateValue)
        {
            var modifiedActions = new Vector[HORIZON_SIZE];
            var newNextStates = new Vector[HORIZON_SIZE];
            double sigmaDiscount;

            #region Modify the Action vector

            for (int i = 0; i < HORIZON_SIZE; i++)
            {
                modifiedActions[i] = currentActions[i].Clone();
                sigmaDiscount = (double)(i + 1)/HORIZON_SIZE;
                // < MinAction ; _action + Rand ; MaxAction >
                for (int j = 0; j < modifiedActions[i].Dimension; j++)
                {
                    modifiedActions[i][j] = Math.Min(
                        Math.Max(MinAction[j], modifiedActions[i][j] + Sampler.SampleFromNormal(0, sigma) * sigmaDiscount),
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
                ++Phi;
            }
            #endregion

            UpdateSigma(ref sigma);
        }
    }
}

