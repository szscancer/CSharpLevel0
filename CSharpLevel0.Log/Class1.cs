using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]
namespace CSharpLevel0.Log
{
    public class LogManager
    {
        //public static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("BaseLogger");//NLog
        //public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(typeof(LogManager));//log4net
        public static readonly log4net.ILog Logger = log4net.LogManager.GetLogger("BaseLogger");//log4net
        static LogManager()
        {
            XmlConfigurator.Configure();
        }
    }
}
