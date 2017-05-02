using NLog;
using psDataImporter.Data;

namespace psDataImporter.Console
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var accessdata = new AccessRepository();
            var postgresData = new PostgresRepository();

   //         var lifeHistories = accessdata.GetNewLifeHistories();
  //          postgresData.PushLifeHistorysToPostgres(lifeHistories);


            var weights = accessdata.GetWeights();
            
            Logger.Info("done");
            System.Console.ReadLine();
        }
    }
}


// use raw sql import
// log errors not success
// create lib of useful db calls.