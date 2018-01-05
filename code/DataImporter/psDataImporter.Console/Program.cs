using System.Collections.Generic;
using System.Diagnostics;

using NLog;
using pgDataImporter.Core;

using psDataImporter.Contracts.Access;
using psDataImporter.Data;

namespace psDataImporter.Console
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();
            
            var accessdata = new AccessRepository();
            var postgresData = new PostgresCore();

            postgresData.AddStaticData();

            var ultrasoundData = accessdata.GetUltrasounds();
            postgresData.ProcessUltrasoundData(ultrasoundData);

            var radioCollarData = accessdata.GetRadioCollars();
            postgresData.ProcessRadioCollarData(radioCollarData);

            var lifeHistories = accessdata.GetLifeHistorys();
            postgresData.ProcessLifeHistories(lifeHistories);

            var weights = accessdata.GetWeights();
            postgresData.ProcessWeights(weights);

            var oestruses = accessdata.GetOestruses();
            postgresData.ProcessOestrusData(oestruses);

            var captures = accessdata.GetCaptures();
            postgresData.ProcessCaptures(captures);

            var pups = accessdata.GetPups();
            postgresData.ProcessPups(pups);

            var babysittingRecords = accessdata.GetBabysittingRecords();
            postgresData.ProcessBabysittingRecords(babysittingRecords);

            var groupCompositions = accessdata.GetGroupCompositions();
            postgresData.ProcessGroupCompositions(groupCompositions);

            var pooSamples = accessdata.GetPoo();
            postgresData.ProcessPoo(pooSamples);

            var weatherData = accessdata.GetWeather();
            postgresData.ProcessWeather(weatherData);

            List<Maternal_Condition_Experiment_Litters> conditionLitter = accessdata.GetMaternalConditionLitters();
            postgresData.ProcessConditionLitters(conditionLitter);

            var conditionFemales = accessdata.GetMaternalConditionFemales();
            postgresData.ProcessConditionFemales(conditionFemales);

            var bloodData = accessdata.GetBloodData();
            postgresData.ProcessBloodData(bloodData);

            var hpaSamples = accessdata.GetHpaSamples();
            postgresData.ProcessHpaSamples(hpaSamples);

            List<DNA_SAMPLES> dnaSamples = accessdata.GetDnaSamples();
            postgresData.ProcessDnaSamples(dnaSamples);

            stopwatch.Stop();

            Logger.Info($"done. Time taken {stopwatch.Elapsed}.");
            System.Console.ReadLine();
        }
    }
}


// use raw sql import
// log errors not success
// create lib of useful db calls.