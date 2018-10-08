
using System.Diagnostics;
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
            var stopwatch = Stopwatch.StartNew();

            var accessdata = new AccessRepository();
            var postgresData = new PostgresCore();

            postgresData.AddStaticData();

            var lifeHistories = accessdata.GetLifeHistorys();
            postgresData.ProcessLifeHistories(lifeHistories);

            var weights = accessdata.GetWeights();
            postgresData.ProcessWeights(weights);

            var conditionFemales = accessdata.GetMaternalConditionFemales();
            postgresData.ProcessConditionFemales(conditionFemales);

            var conditionProvisioning = accessdata.GetMaternalConditionProvisioning();
            postgresData.ProcessConditionProvisioning(conditionProvisioning);

            var bloodData = accessdata.GetBloodData();
            postgresData.ProcessBloodData(bloodData);

            var hpaSamples = accessdata.GetHpaSamples();
            postgresData.ProcessHpaSamples(hpaSamples);

            var dnaSamples = accessdata.GetDnaSamples();
            postgresData.ProcessDnaSamples(dnaSamples);

            var antiParasite = accessdata.GetAntiParasite();
            postgresData.ProcessAntiParasite(antiParasite);

            var oxFeeding = accessdata.GetOxFeeding();
            postgresData.ProcessOxFeeding(oxFeeding);

            var oxMales = accessdata.GetOxMale();
            postgresData.ProcessOxMale(oxMales);

            var oxFemales = accessdata.GetOxFemale();
            postgresData.ProcessOxFemale(oxFemales);

            var oestruses = accessdata.GetOestruses();
            postgresData.ProcessOestrusData(oestruses);

            var ultrasoundData = accessdata.GetUltrasounds();
            postgresData.ProcessUltrasoundData(ultrasoundData);

            var radioCollarData = accessdata.GetRadioCollars();
            postgresData.ProcessRadioCollarData(radioCollarData);

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

            var conditionLitter = accessdata.GetMaternalConditionLitters();
            postgresData.ProcessConditionLitters(conditionLitter);

            var captures = accessdata.GetCaptures();
            postgresData.ProcessCaptures(captures);

            stopwatch.Stop();

            Logger.Info($"done. Time taken {stopwatch.Elapsed}.");
            System.Console.ReadLine();
        }
    }
}
