using Inzynierka.Model.ControlAlgorithm;
using Inzynierka.Model.ControlAlgorithm.ModelPredictiveReinforcementLearning;
using Inzynierka.Model.ControlAlgorithm.PredictionControl;
using Inzynierka.Model.Model;
using Inzynierka.Model.Model.Pendulum;
using Inzynierka.Model.Model.PoliReactor;
using System;
using System.Collections.Generic;

namespace Inzynierka.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Properties

        #region Algorithms

        public List<AAlgorithmFactory> Algorithms { get; set; }

        #region SelectedAlgorithm

        private AAlgorithmFactory _selectedAlgorithm;

        public AAlgorithmFactory SelectedAlgorithm
        {
            get { return _selectedAlgorithm; }
            set
            {
                if (_selectedAlgorithm != value || value == null || _selectedAlgorithm == null)
                {
                    _selectedAlgorithm = value;
                    AlgorithmDescription = _selectedAlgorithm != null ? _selectedAlgorithm.Description : null;
                    OnPropertyChanged("AlgorithmDescription");
                }
            }
        }

        #endregion SelectedAlgorithm

        public string AlgorithmDescription { get; set; }

        #endregion Algorithms


        #region Models
        public List<AModelFactory> Models { get; set; }

        #region SelectedModel
        private AModelFactory _selectedModel;
        public AModelFactory SelectedModel
        {
            get { return _selectedModel; }
            set
            {
                if (_selectedModel != value || value == null || _selectedModel == null)
                {
                    _selectedModel = value;
                    ModelDescription = _selectedModel != null ? _selectedModel.Description : null;
                    OnPropertyChanged("ModelDescription");
                }
            }
        }
        #endregion SelectedModel

        public string ModelDescription { get; set; }

        #endregion Models

        #endregion Properties


        public MainWindowViewModel()
        {

#region Algorithm & Model Factories Initialisation

            Algorithms = new List<AAlgorithmFactory>()
            {
                new PredictionControlFactory(),
                new ModelPredictiveReinforcementLearningFactory()
            };
            Models = new List<AModelFactory>
            {
                new PoliReactorFactory(), 
                new PendulumFactory()
            };

#endregion Algorithm & Model Factories Initialisation
        }
    }
}