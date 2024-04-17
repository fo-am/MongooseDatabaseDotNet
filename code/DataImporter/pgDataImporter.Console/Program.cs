
using System.Diagnostics;
using NLog;
using pgDataImporter.Core;
using pgDataImporter.Data;

namespace pgDataImporter.Console
{
    internal class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            var accessData = new AccessRepository();
            var postgresData = new PostgresCore();

            postgresData.AddStaticData();

            var lifeHistories = accessData.GetLifeHistorys();
            postgresData.ProcessLifeHistories(lifeHistories);

            var weights = accessData.GetWeights();
            postgresData.ProcessWeights(weights);

            var conditionFemales = accessData.GetMaternalConditionFemales();
            postgresData.ProcessConditionFemales(conditionFemales);

            var conditionProvisioning = accessData.GetMaternalConditionProvisioning();
            postgresData.ProcessConditionProvisioning(conditionProvisioning);

            var bloodData = accessData.GetBloodData();
            postgresData.ProcessBloodData(bloodData);

            var hpaSamples = accessData.GetHpaSamples();
            postgresData.ProcessHpaSamples(hpaSamples);

            var dnaSamples = accessData.GetDnaSamples();
            postgresData.ProcessDnaSamples(dnaSamples);

            var antiParasite = accessData.GetAntiParasite();
            postgresData.ProcessAntiParasite(antiParasite);

            var oxFeeding = accessData.GetOxFeeding();
            postgresData.ProcessOxFeeding(oxFeeding);

            var oxMales = accessData.GetOxMale();
            postgresData.ProcessOxMale(oxMales);

            var oxFemales = accessData.GetOxFemale();
            postgresData.ProcessOxFemale(oxFemales);

            var oestruses = accessData.GetOestruses();
            postgresData.ProcessOestrusData(oestruses);

            var ultrasoundData = accessData.GetUltrasounds();
            postgresData.ProcessUltrasoundData(ultrasoundData);

            var radioCollarData = accessData.GetRadioCollars();
            postgresData.ProcessRadioCollarData(radioCollarData);

            var pupAssocs = accessData.GetPupAssocs();
            postgresData.ProcessPupAssocs(pupAssocs);

            var babysittingRecords = accessData.GetBabysittingRecords();
            postgresData.ProcessBabysittingRecords(babysittingRecords);

            var groupCompositions = accessData.GetGroupCompositions();
            postgresData.ProcessGroupCompositions(groupCompositions);

            var pooSamples = accessData.GetPoo();
            postgresData.ProcessPoo(pooSamples);

            var weatherData = accessData.GetWeather();
            postgresData.ProcessWeather(weatherData);

            var conditionLitter = accessData.GetMaternalConditionLitters();
            postgresData.ProcessConditionLitters(conditionLitter);

            var captures = accessData.GetCaptures();
            postgresData.ProcessCaptures(captures);

            stopwatch.Stop();

            Logger.Info($"done. Time taken {stopwatch.Elapsed}.");
            System.Console.ReadLine();
        }
    }
}
