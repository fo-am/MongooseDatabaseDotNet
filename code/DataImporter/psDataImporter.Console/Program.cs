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
            postgresData.ProccessUltrasoundData(ultrasoundData);

            var radioCollarData = accessdata.GetRadioCollars();
            postgresData.ProccessRadioCollarData(radioCollarData);

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

            stopwatch.Stop();

            Logger.Info($"done. Time taken {stopwatch.Elapsed}.");
            System.Console.ReadLine();
        }
    }
}


// use raw sql import
// log errors not success
// create lib of useful db calls.