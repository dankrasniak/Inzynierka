using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Inzynierka.Model.Logger
{
    public class LogIt
    {
        private readonly IDictionary _loggers = new Dictionary<string, log4net.ILog>();
        private readonly String _date = DateTime.Now.ToShortDateString();
        private readonly String _time = DateTime.Now.TimeOfDay.ToString();
        private readonly String _path;
        private readonly List<LoggedValue> _loggedValues;


        public LogIt(String path, List<LoggedValue> loggedValues)
        {
            _path = path;
            _loggedValues = new List<LoggedValue>();
            foreach (var loggedValue in loggedValues)
            {
                _loggedValues.Add(new LoggedValue(loggedValue));
            }
        }

        private void CreateLogger(String name, String conversionPattern)
        {
            // Create Appender
            var nameWDate = name + "_" + _time;
            var path = (_path.Equals(""))
                ? @"Logs\" + _date + @"\" + _time.Replace(':', '_') + @"\" + name + ".log"
                : _path + _date + @"\" + _time.Replace(':', '_') + @"\" + name + ".log";
            var appender = new RollingFileAppender()
            {
                Name = nameWDate,
                File = path,
                AppendToFile = true
            };

            // Create Layout
            var layout = new PatternLayout() {ConversionPattern = conversionPattern};
            layout.ActivateOptions();

            // Apply Layout to Appender
            appender.Layout = layout;
            appender.ActivateOptions();

            // Create Logger
            var logger = LogManager.GetLogger(nameWDate);

            // Apply Appender to Logger
            ((log4net.Repository.Hierarchy.Logger) logger.Logger).AddAppender(appender);

            // Add Logger to Dictionary
            _loggers.Add(name, logger);

            XmlConfigurator.Configure();
        }

        public void Log(String name, Double value)
        {
            // Does this variable exist and does the user want to log it?
            if (!_loggedValues.Exists(v => v.Name.Equals(name) && v.Logged))
                return;

            if (!_loggers.Contains(name))
                CreateLogger(name, "%m%n");//%d - %m%n

            (_loggers[name] as ILog).Info(FormatDouble(value));
        }

        public void Log(String name, String value)
        {
            // Does this variable exist and does the user want to log it?
            if (!_loggedValues.Exists(v => v.Name.Equals(name) && v.Logged))
                return;

            if (!_loggers.Contains(name))
                CreateLogger(name, "%m%n");//%d - %m%n

            (_loggers[name] as ILog).Info(value);
        }

    private static string FormatDouble(Double value)
        {
            return value.ToString(String.Format("{0}{1}", "F", 4));
        }
    }
} // "%d [%t] %-5p %c [%x] - %m%n"