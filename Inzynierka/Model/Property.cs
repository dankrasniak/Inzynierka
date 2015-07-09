using System;

namespace Inzynierka.Model
{
    //public delegate string Myfunc(object);

    public class Property
    {
        private string _name;
        private object _value;
        private string _description;
        private double _min;
        private double _max;
        //private Myfunc _verify;

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public object Value
        {
            get { return _value; }
            set
            { // TODO EHHHHH....
                _value = value;
                if (_value.GetType() == "".GetType())
                    _value = ((string)_value).Replace(".",",");
            }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public double Min
        {
            get { return _min; }
        }

        public double Max
        {
            get { return _max; }
        }

        #endregion Properties

        public Property(string name, object value, string description, double min, double max) //, Myfunc verify
        {
            this._name = name;
            this._value = value;
            this._description = description;
            this._min = min;
            this._max = max;
            //this._verify = verify;
        }

        public Property(Property copy)
        {
            _name = copy.Name;
            _value = copy.Value;
            _description = copy.Description;
            _min = copy.Min;
            _max = copy.Max;
            // _verify = copy.Verify; // sth
        }

        //public string Verify()
        //{
        //    return _verify(_value);
        //}
        public string Verify()
        {
            if (_value.GetType() == (1.0).GetType()) ; // TODO
            //throw new NotImplementedException();
            return "";
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public void Set(string value)
        {
            throw new NotImplementedException();
        }
    }
}