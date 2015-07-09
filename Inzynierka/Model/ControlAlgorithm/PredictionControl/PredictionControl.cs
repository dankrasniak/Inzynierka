using Inzynierka.Model.Logger;
using Inzynierka.Model.Model;
using System;
using System.Collections.Generic;

namespace Inzynierka.Model.ControlAlgorithm.PredictionControl
{
    public class PredictionControl : IAlgorithm
    {
        private readonly LogIt logger;

        private readonly IModel _model;
        private readonly List<Double> _state;
        private List<List<Double>> _horizon;
        private readonly int _horizonSize = 20;
        private double H_STEP_SIZE = 0.001; // TODO Verify

        public PredictionControl(IModel model, List<Property> properties, List<LoggedValue> loggedValues) 
        {
            logger = new LogIt("", loggedValues); // TODO Add Path

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
            logger.Log("Wartość wejściowa", controlVariables[0]);

            var s = RungeKuttha(_state, controlVariables);
            var STATE_VAR_COUNT = s.Count;

            for (int i = 0; i < STATE_VAR_COUNT; ++i)
            {
                _state[i] = s[i];
            }

            #region LogStateVariables

            var stateVariables = "";
            for (int i = 0; i < STATE_VAR_COUNT; ++i)
            {
                stateVariables += FormatDouble(_state[i]) + " ";
            }
            logger.Log("stateVariables", stateVariables); // TODO

            #endregion LogStateVariables

            var result = _model.GetValue(_state);
            logger.Log("Wartość wyjściowa", result); // TODO

            return result;
        }

        private List<Double> GetControlVariables()
        {
            PrepareHorizont();

            #region Log Horizon Variables

            var horizonLog = "";
            for (var i = 0; i < _horizonSize; ++i)
            {
                horizonLog += FormatDouble(_horizon[i][0]) + " ";
            }
            logger.Log("horizon", horizonLog);

            #endregion Log Horizon Variables

            return _horizon[0];
        }

        #region EvoAlg

        private int M = 10;
        private double C1 = 0.82;
        private double C2 = 1.2;
        private double SigmaMin = 0.0001;

        // Standard deviation
        private double _sigma;
        private double _startSigma = 0.001; // TODO
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
            var tmpHorizon = ModifyHorizon(_horizon);
            
            //var currentHorizonState = GetFinalState(_state, _horizon);
            //var newHorizonState = GetFinalState(_state, tmpHorizon);
            //logTest.Info("Possible states: " + FormatDouble(_model.GetValue(currentHorizonState)) + " VS: " + FormatDouble(_model.GetValue(newHorizonState)));

            //if (!_model.IsFirstBetter(currentHorizonState, newHorizonState))
            //    _horizon = tmpHorizon;

            #region tmp

            var currentHorizonStateTmp = GetHorizonStatesList(_state, _horizon);
            var newHorizonStateTmp = GetHorizonStatesList(_state, tmpHorizon);

            var currentHorizonValue = 0.0;
            var newHorizonValue = 0.0;
            for (var i = 0; i < _horizonSize; ++i)
            {
                currentHorizonValue -= i*Math.Pow(_model.GetDiscrepancy(currentHorizonStateTmp[i]), 2);
            }
            for (var i = 0; i < _horizonSize; ++i)
            {
                newHorizonValue -= i*Math.Pow(_model.GetDiscrepancy(newHorizonStateTmp[i]), 2);
            }

            logger.Log("Possible states", "Possible states: " + FormatDouble(currentHorizonValue) + " VS: " + FormatDouble(newHorizonValue));

            if (currentHorizonValue <= newHorizonValue)
                _horizon = tmpHorizon;
            #endregion tmp

            UpdateSigma();
        }

        private List<List<Double>> ModifyHorizon(List<List<Double>> horizon)
        {
            var result = new List<List<Double>>();
            int SIZE = horizon.Count;
            int SIZE2 = (horizon[0]).Count;

            for (int i = 0; i < SIZE; ++i)
            {
                result.Add(new List<Double>(horizon[i]));
            }

            for (int i = 0; i < SIZE; ++i)
            {
                for (int j = 0; j < SIZE2; ++j)
                {
                    result[i][j] = Math.Abs(result[i][j] + _sigma * GetGaussian());
                }
            }

            return result;
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
            _phi = 0;
            
            return; // == 1.5
        }

        private Random _rand = new Random(); //reuse this if you are generating many

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

        private List<Double> GetFinalState(IEnumerable<double> initialState, IEnumerable<List<double>> horizon)
        {
            var state = new List<Double>(initialState);
            foreach (var c in horizon)
            {
                state = RungeKuttha(state, c);
            }
            return state;
        }

        private List<List<Double>> GetHorizonStatesList(IEnumerable<double> initialState, IEnumerable<List<double>> horizon)
        {
            var state = new List<Double>(initialState);
            var stateList = new List<List<Double>>();
            foreach (var c in horizon)
            {
                state = RungeKuttha(state, c);

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