namespace Inzynierka.Model.Logger
{
    public class LoggedValue
    {
        public string Origin { get; set; }
        public string Name { get; set; }
        public bool Logged { get; set; }

        public LoggedValue(string origin, string name, bool logged)
        {
            Origin = origin;
            Name = name;
            Logged = logged;
        }
        public LoggedValue(LoggedValue copy)
        {
            Origin = copy.Origin;
            Name = copy.Name;
            Logged = copy.Logged;
        }
    }
}