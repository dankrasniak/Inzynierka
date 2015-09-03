using Inzynierka.Model;
using Inzynierka.Model.ControlAlgorithm;
using Inzynierka.Model.Logger;
using Inzynierka.Model.Model;
using System.Collections.Generic;
using System.Globalization;

namespace Inzynierka.ViewModel
{
    public class SettingWindowViewModel : ViewModelBase
    {
        #region Algorithm


        private AAlgorithmFactory _algorithmFactory;
        public AAlgorithmFactory AlgorithmFactory
        {
            get { return _algorithmFactory; }
            set
            {
                _algorithmFactory = value;
                AlgorithmFactoryName = _algorithmFactory.Name;
                OnPropertyChanged("AlgorithmFactoryName");
                PropertiesA = new List<Property>();
                foreach (Property e in _algorithmFactory.Properties)
                {
                    PropertiesA.Add(new Property(e));
                }
                OnPropertyChanged("PropertiesA");

                foreach (var e in _algorithmFactory.LoggedValues)
                {
                    Log.Add(e);
                }
                OnPropertyChanged("Log");
            }
        }

        #region SelectedPropertyA

        private Property _selectedPropertyA;

        public Property SelectedPropertyA
        {
            get { return _selectedPropertyA; }
            set 
            {
                if (_selectedPropertyA == null || value == null || _selectedPropertyA != value)
                {
                    _selectedPropertyA = value;
                    OnPropertyChanged("SelectedPropertyA");
                    
                    if (value != null)
                    {
                        SelectedPropertyADescription = value.Description;
                        OnPropertyChanged("SelectedPropertyADescription");
                        SelectedPropertyAMin = value.Min.ToString(CultureInfo.InvariantCulture);
                        OnPropertyChanged("SelectedPropertyAMin");
                        SelectedPropertyAMax = value.Max.ToString(CultureInfo.InvariantCulture);
                        OnPropertyChanged("SelectedPropertyAMax");
                    }
                }
            }
        }
        #endregion SelectedPropertyA

        public string SelectedPropertyADescription { get; set; }
        public string SelectedPropertyAMin { get; set; }
        public string SelectedPropertyAMax { get; set; }

        public List<Property> PropertiesA { get; set; } 

        public string AlgorithmFactoryName { get; set; }

        #endregion Algorithm


        #region Model

        private AModelFactory _modelFactory;

        public AModelFactory ModelFactory
        {
            get { return _modelFactory; }
            set
            {
                _modelFactory = value;
                ModelFactoryName = _modelFactory.Name;
                OnPropertyChanged("ModelFactoryName");
                PropertiesM = new List<Property>();
                foreach (Property e in _modelFactory.Properties)
                {
                    PropertiesM.Add(new Property(e));
                }
                OnPropertyChanged("PropertiesM");

                // Odkomentować w przypadku chęci zaimplementowania wartości logowalnych w kodzie obietków dynamicznych.
                //foreach (var e in _modelFactory.LoggedValues)
                //{
                //    Log.Add(e);
                //}
                //OnPropertyChanged("Log");
            }
        }

        #region SelectedPropertyM

        private Property _selectedPropertyM;

        public Property SelectedPropertyM
        {
            get { return _selectedPropertyM; }
            set
            {
                if (_selectedPropertyM == null || value == null || _selectedPropertyM != value)
                {
                    _selectedPropertyM = value;
                    OnPropertyChanged("SelectedPropertyM");

                    if (value != null)
                    {
                        SelectedPropertyMDescription = value.Description;
                        OnPropertyChanged("SelectedPropertyMDescription");
                        SelectedPropertyMMin = value.Min.ToString(CultureInfo.InvariantCulture);
                        OnPropertyChanged("SelectedPropertyMMin");
                        SelectedPropertyMMax = value.Max.ToString(CultureInfo.InvariantCulture);
                        OnPropertyChanged("SelectedPropertyMMax");
                    }
                }
            }
        }
        #endregion SelectedPropertyM

        public string SelectedPropertyMDescription { get; set; }
        public string SelectedPropertyMMin { get; set; }
        public string SelectedPropertyMMax { get; set; }

        public List<Property> PropertiesM { get; set; }

        public string ModelFactoryName { get; set; }

        #endregion Model

        #region Log
        public List<LoggedValue> Log { get; set; }
        #endregion Log

        #region SimulationSettings
        #endregion SimulationSettings

        public SettingWindowViewModel()
        {
            Log = new List<LoggedValue>();
        }
    }
}