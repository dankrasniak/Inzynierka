using System.Windows.Navigation;
using Inzynierka.Model.ControlAlgorithm;
using Inzynierka.Model.ControlAlgorithm.PredictionControl;
using Inzynierka.Model.Model;
using Inzynierka.Model.Model.Pendulum;
using Inzynierka.Model.Model.PoliReactor;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Inzynierka.View;

namespace Inzynierka.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        // TODO TMP
        private PredictionControl _algorithm;
        private PoliReactor _model;

        #region Properties

        /** Instructions
         * Input window variables here.
         */
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

        #region Value
        public Double Value { get; set; }
        #endregion Value
        
        #endregion Properties

        #region Buttons
        //public ICommand StepButton { get; set; } // TODO DELETE
        #endregion Buttons

        public MainWindowViewModel()
        {
            /*
            {
                // TODO TMP
                _model = new PoliReactor(new PoliReactorFactory().Properties);
                _algorithm = new PredictionControl(_model, new PredictionControlFactory().Properties, new PredictionControlFactory().LoggedValues);
            }
             */

#region Algorithm & Model Factories Initialisaton

            Algorithms = new List<AAlgorithmFactory>()
            {
                new PredictionControlFactory()
            };
            Models = new List<AModelFactory>
            {
                new PoliReactorFactory(), 
                new PendulumFactory()
            };

#endregion Algorithm & Model Factories Initialisaton

            /*
#region Button Initialisation

            StepButton = new ButtonCommand(
                () =>
                {
                    //step
                    this.Value = (Double)_algorithm.GetValueTMP();
                    OnPropertyChanged("Value");
                },
                () => true
            );

#endregion Button Initialisation
             */
        }
    }
}