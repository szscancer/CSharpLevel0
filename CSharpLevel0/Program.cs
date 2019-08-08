using CSharpLevel0.Async;
using CSharpLevel0.Json;
using CSharpLevel0.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpLevel0
{
    class Program
    {
        static void Main(string[] args)
        {
            //Log();
            //Json();
            Task();
            Console.Read();
        }

        private static void Log()
        {
            LogManager.Logger.Info("123");
        }

        private static void Json()
        {
            JsonManager.Manage();
        }

        private static void Task()
        {
            TaskManager.Manage();
        }
    }
}
