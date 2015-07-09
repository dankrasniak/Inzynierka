using System;
using System.Collections;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcentLearning.Computing;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcentLearning.Neural;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcentLearning.Probability;

namespace Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcentLearning.ReinforcementPlatform.CCMPctrl
{
	/// <summary>
	/// Summary description for Advisor.
	/// </summary>
    public class Advisor //: Reinforcement.CCAdvisor
    {
        protected MLPerceptron2 V;    // aproksymator funkcji V
        protected Vector Vval;        // wektor zwracany przez ten aproksymator 
        protected Vector Vgrad;       // gradient na wyjściu aproksymatora V
        protected double betaV;       // parametr kroku dla tego aproksymatora 

        #region parametry 
        protected int H;               // długość horyzontu
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

        public Advisor() : base()
		{
			Sampler = new ASampler(); 
			TimeIndex = 0;
			AllVisits = new ArrayList(); 
		}

        new public void Init(double[] state_av, double[] state_stddev,
            double[] action_min, double[] action_max,
            int thetasize, int vsize, double discount, Reinforcement.ActualAction act_action)
            // inicjalizacja 
        {
            int StateDim = state_av.GetLength(0);
            int ActionDim = action_min.GetLength(0);

            // budowa sieci neuronowej 
            V = new MLPerceptron2();
            V.Build(StateDim, CellType.Arcustangent, new int[] { vsize, 1 });
            V.SetInputDescription(new Vector(state_av), new Vector(state_stddev));
            V.InitWeights(1.0/Math.Sqrt(vsize+1));
            Vval = new Vector(1);
            Vgrad = new Vector(1); 
            // koniec budowy sieci neuronowej 

            Gamma = discount;

            H = 10;  // powinno przyjść z zewnątrz 
            Actions = new Vector[H];
            NextStates = new Vector[H];
            for (int i = 0; i < H; i++)
            {
                Actions[i] = new Vector(ActionDim, 0.0);
                NextStates[i] = new Vector(StateDim, 0.0); 
            }

            // reszta parametrów niepotrzebna 
        }

        public void SetParameters(
            double a_stepsize, double v_stepsize,
            double a_radius, double r_scalator,
            double blur_sigma, int noise_inertia, double actor_penalty,
            double lambda,
            double alph_gam, double squash_param,
            double max_int_intensity,
            int int_steps_no, int window_width
            )
            // ustawienie parametrów 
            // (ten ich zestaw jest od czapy)
        {
            betaV = v_stepsize; 
        }

        //	Name
		public string CCName // nazwa 
		{
			get { return "MPctrl"; }
		}

		//  początek epizodu sterowania 
		public void CCStartInState(double[] state) 
		{
            State = new Vector(state); 
		}

		//  wiemy w jakim system jest stanie 
        // i produkujemy sterowanie dla niego 
		public void CCAdviseAction(ref double[] action)
		{
            Vest = ObliczV(State, Actions, ref NextStates); 
            for (int i = 0; i < 10; i++) // kilka poprawek
                Popraw(State, 1, ref Actions, ref NextStates, ref Vest);

            AllVisits.Add(new AVisit(TimeIndex++, State, Actions, NextStates)); 
            action = Actions[0].Table; 
		}

		//  sterowanie zostało wykonane, 
        // system przeszedł do następnego stanu (należy zignorować reward) 
		public void CCThisHappened(double reward, double[] next_state)
		{
            if (ModelCzyStanDopuszczalny(new Vector(next_state)))
            {
                State = new Vector(next_state);
                for (int i = 0; i < H - 1; i++)
                {
                    Actions[i] = Actions[i + 1];
                    NextStates[i] = NextStates[i + 1];
                }
            }

            // dokonujemy poprawek historycznych sterowań i funkcji V
            for (int k = 0; k < 10; k++) // kilka kolejnych stanów początkowych do poprawienia 
            {
                Visit = (AVisit)AllVisits[Sampler.Next(AllVisits.Count - 1)]; 
                Vest = ObliczV(Visit.State, Visit.Actions, ref Visit.NextStates); 
                for (int i = 0; i < 10; i++) // kilka poprawek
                    Popraw(Visit.State, 1, ref Visit.Actions, ref Visit.NextStates, ref Vest);

                V.Approximate(Visit.State, ref Vval);
                Vgrad[0] = Vval[0] - Vest;
                V.AddToWeights(Vgrad, -betaV); 
            }
        }

        #region Model - na razie zaślepki, wszystko uzależnione od modelu obiektu 
        protected bool ModelCzyStanDopuszczalny(Vector state)
        {
            return true; 
        }

        protected double ModelNagroda(Vector action, Vector next_state)
        {
            if (ModelCzyStanDopuszczalny(next_state))
                return 1;
            else
                return 0; 
        }

        protected Vector ModelNastepnyStan(Vector state, Vector action)
        {
            return new Vector(state.Dimension, 0.0); 
        }
        #endregion 

        protected double ObliczV(Vector state, Vector[] actions, ref Vector[] next_states)
            // oblicz 
        {
            int h=actions.Length; 
            next_states = new Vector[h];
            Vector s = state;
            double val = 0;
            double gammai = 1; 
            for (int i = 0; i < h; i++)
            {
                next_states[i] = s = ModelNastepnyStan(s, Actions[i]);
                if (!ModelCzyStanDopuszczalny(s))
                    return val; 
                val += gammai * ModelNagroda(actions[i], s);
                gammai *= Gamma; 
            }
            V.Approximate(s, ref Vval);
            val += gammai * Vval[0];
            return val; 
        }

        protected bool Popraw(Vector state, double sigma, ref Vector[] actions, ref Vector[] next_states, ref double val)
            // iteracja algorytmu (1+1)
            // zwraca, czy nastąpiła poprawa 
        {
            Vector[] _actions = new Vector[H];
            Vector[] _next_states = new Vector[H];
            for (int i = 0; i < H; i++)
            {
                _actions[i] = actions[i].Clone();
                for (int j = 0; j < _actions[i].Dimension; j++)
                    _actions[i][j] = Math.Min(Math.Max(
                        MinAction[j], _actions[i][j] + Sampler.SampleFromNormal(0, sigma)), 
                        MaxAction[j]); 
            }
            double _val = ObliczV(state, _actions, ref _next_states);
            if (_val > val)
            {
                val = _val; 
                actions = _actions;
                next_states = _next_states;
                return true; 
            }
            return false; 
        }
    }
}

