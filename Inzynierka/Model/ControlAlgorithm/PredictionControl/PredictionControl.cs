using System.Data;
using System.Linq;
using Inzynierka.Model.Logger;
using Inzynierka.Model.Model;
using System;
using System.Collections.Generic;

namespace Inzynierka.Model.ControlAlgorithm.PredictionControl
{
    public class PredictionControl : IAlgorithm
    {
        private readonly LogIt _logger;

        private readonly IModel _model;
        private readonly List<Double> _state;
        private List<List<Double>> _horizon;
        private readonly int _horizonSize;
        private double H_STEP_SIZE = 0.001; // TODO Verify

        public PredictionControl(IModel model, List<Property> properties, List<LoggedValue> loggedValues) 
        {
            _logger = new LogIt("", loggedValues); // TODO Add Path

            _model = model;
            _state = _model.GetInitialState();
            GenerateHorizon();

            // Variables from properties list
            H_STEP_SIZE = Convert.ToDouble(properties.Find(p => p.Name.Equals("H_STEP")).Value);
            M = (int) Convert.ToDouble(properties.Find(p => p.Name.Equals("M")).Value);
            _startSigma = Convert.ToDouble(properties.Find(p => p.Name.Equals("StartSigma")).Value);
            _horizonSize = (int)Convert.ToDouble(properties.Find(p => p.Name.Equals("Horizon")).Value);
        }

        private void GenerateHorizon()
        {
            _horizon = new List<List<Double>>();

            for (int i = 0; i < _horizonSize; ++i)
            {
                _horizon.Add(_model.GenerateControlVariables());
            }
        }

        public Double GetValueTMP()
        {
            var controlVariables = GetControlVariables();
            _logger.Log("Wartość wejściowa", 
                controlVariables.Aggregate("", (current, t) => current + (FormatDouble(t) + " ")));

            var nextState = RungeKuttha(_state, controlVariables);
            var STATE_VAR_COUNT = nextState.Count;

            for (int i = 0; i < STATE_VAR_COUNT; ++i)
            {
                _state[i] = nextState[i];
            }

            _logger.Log("stateVariables", 
                _state.Aggregate("", (s, d) => s + FormatDouble(d) + " ")); // TODO

            var result = _model.GetValue(_state);
            _logger.Log("Wartość wyjściowa", result); // TODO

            return result;
        }

        private List<Double> GetControlVariables()
        {
            PrepareHorizont();

            _logger.Log("horizon", 
                _horizon.Aggregate("", (s, list) => s + FormatDouble(list[0]) + " "));

            return _horizon[0];
        }

        #region EvoAlg

        private readonly int M = 10;
        private const double C1 = 0.82;
        private const double C2 = 1.2;
        private const double SigmaMin = 0.0001;

        // Standard deviation
        private double _sigma;
        private readonly double _startSigma = 0.001; // TODO
        // Number of times the child specimen was chosen over the parent specimen in the last cicle of M iterations
        private int _phi;
        private int _iterationNum;

        private void PrepareHorizont()
        {
            UpdateHorizon();

            _phi = 0;
            _iterationNum = 0;
            _sigma = _startSigma;

            while (!IsFinished())
            {
                ++_iterationNum;
                NextIteration();
            }
        }

        private void NextIteration()
        {
            var modifiedHorizon = ModifyHorizon(_horizon);
            
            /*
            var currentHorizonState = GetFinalState(_state, _horizon);
            var newHorizonState = GetFinalState(_state, tmpHorizon);
            _logger.Log("Possible states", "Possible states: " + FormatDouble(currentHorizonValue) + " VS: " + FormatDouble(newHorizonValue));

            if (!_model.IsFirstBetter(currentHorizonState, newHorizonState))
                _horizon = tmpHorizon;
             */

            #region tmp

            var currentHorizonValue = GetHorizonValue(_horizon);
            var newHorizonValue = GetHorizonValue(modifiedHorizon);

            _logger.Log("Possible states", "Possible states: " + FormatDouble(currentHorizonValue) + " VS: " + FormatDouble(newHorizonValue)); // TODO

            if (currentHorizonValue <= newHorizonValue)
                _horizon = modifiedHorizon;

            #endregion tmp

            UpdateSigma();
        }

        private double GetHorizonValue(IEnumerable<List<double>> horizon)
        {
            var horizonValue = 0.0;
            var horizonStatesList = GetHorizonStatesList(_state, horizon);

            for (int i = 0; i < _horizonSize; ++i)
            {
                horizonValue -= i*Math.Pow(_model.GetDiscrepancy(horizonStatesList[i]), 2);
            }

            return horizonValue;
        }

        private List<List<Double>> ModifyHorizon(List<List<Double>> horizon)
        {
            var modifiedHorizon = new List<List<Double>>();
            int ControlVariablesNr = (horizon[0]).Count;

            for (int i = 0; i < _horizonSize; ++i)
            {
                modifiedHorizon.Add(new List<Double>(horizon[i]));

                for (int j = 0; j < ControlVariablesNr; ++j)
                {
                    modifiedHorizon[i][j] = Math.Abs(horizon[i][j] + _sigma * GetGaussian());
                }
            }

            return modifiedHorizon;
        }

        private bool IsFinished()
        {
            return _sigma < SigmaMin;
        }

        private void UpdateSigma()
        {
            if (_iterationNum % M != 0)
                return;

            if ((double)_phi / M < 1.5)
            {
                _phi = 0;
                _sigma *= C1;
                return;
            }
            if ((double)_phi / M > 1.5)
            {
                _phi = 0;
                _sigma *= C2;
                return;
            }
            _phi = 0; // == 1.5
        }

        private readonly Random _rand = new Random(); //reuse this if you are generating many

        private double GetGaussian() // TODO Verify
        {
            double u1 = _rand.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = _rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            //double randNormal =
            //             mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randStdNormal;
        }

        #endregion EvoAlg

        private List<Double> GetFinalState(List<double> initialState, IEnumerable<List<double>> horizon)
        {
            //var state = new List<Double>(initialState);
            //state = horizon.Aggregate(state, RungeKuttha);
            return horizon.Aggregate(initialState, RungeKuttha); ;
        }

        private List<List<Double>> GetHorizonStatesList(IEnumerable<double> initialState, IEnumerable<List<double>> horizon)
        {
            var state = new List<Double>(initialState);
            var stateList = new List<List<Double>>();
            foreach (var controlValues in horizon)
            {
                state = RungeKuttha(state, controlValues);

                stateList.Add(new List<Double>(state));
            }
            return stateList;
        }

        private void UpdateHorizon()
        {
            _horizon.RemoveAt(0);
            _horizon.Add(_horizon[_horizonSize - 2]); // (_model.GenerateControlVariables()); // TODO
        }

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
            
            for (int i = 0; i < ARRAY_SIZE ; ++i)
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

        private static string FormatDouble(Double value)
        {
            return value.ToString(String.Format("{0}{1}", "F", 4));
        }
    }
}