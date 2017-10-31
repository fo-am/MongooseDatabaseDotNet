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


            //var ultrasoundData = accessdata.GetUltrasounds();
            //postgresData.ProccessUltrasoundData(ultrasoundData);

            //var radioCollarData = accessdata.GetRadioCollars();
            //postgresData.ProccessRadioCollarData(radioCollarData);

            var lifeHistories = accessdata.GetLifeHistorys();
            postgresData.ProcessLifeHistories(lifeHistories);

            //var weights = accessdata.GetWeights();
            //postgresData.ProcessWeights(weights);

            //var oestruses = accessdata.GetOestruses();
            //postgresData.ProcessOestrusData(oestruses);

            Logger.Info("done");
            System.Console.ReadLine();
        }
    }
}


// use raw sql import
// log errors not success
// create lib of useful db calls.