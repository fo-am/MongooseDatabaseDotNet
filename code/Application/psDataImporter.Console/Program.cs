using NLog;
using pgDataImporter.Core;
using psDataImporter.Data;

namespace psDataImporter.Console
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var accessdata = new AccessRepository();
            var postgresData = new PostgresCore();

        //    var weights = accessdata.GetWeights();
        //    postgresData.ProcessWeights(weights);

            var ultrasoundData = accessdata.GetUltrasounds();
            postgresData.ProccessUltrasoundData(ultrasoundData);

            Logger.Info("done");
            System.Console.ReadLine();
        }
    }
}


// use raw sql import
// log errors not success
// create lib of useful db calls.