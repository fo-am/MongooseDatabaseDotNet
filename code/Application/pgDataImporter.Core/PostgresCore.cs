using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using psDataImporter.Contracts.Access;
using psDataImporter.Contracts.dtos;
using psDataImporter.Contracts.Postgres;
using psDataImporter.Data;

namespace pgDataImporter.Core
{
    public class PostgresCore
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public void ProcessWeights(IEnumerable<Weights> weights)
        {
            Logger.Info("Starting to add weights.");
            weights = weights as IList<Weights> ?? weights.ToList();
            var pg = new PostgresRepository();

            pg.AddPacks(weights.Select(s => s.Group).Distinct());
            pg.AddIndividuals(weights.GroupBy(s => new {Name = s.Indiv, s.Sex})
                .Select(i => new Individual {Name = i.Key.Name, Sex = i.Key.Sex}));

            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();
            AddPackHistories(
                weights.Select(weight => new PackHistoryDto(weight.Indiv, weight.Group, weight.TimeMeasured)), pgPacks,
                pgIndividuals, pg);

            pg.AddWeights(weights, pgIndividuals);
            Logger.Info("Done adding weights.");
        }

        public void ProccessUltrasoundData(IEnumerable<Ultrasound> ultrasoundData)
        {
            Logger.Info("Starting to add ultrasound data.");
            ultrasoundData = ultrasoundData as IList<Ultrasound> ?? ultrasoundData.ToList();
            var pg = new PostgresRepository();
            pg.AddPacks(ultrasoundData.Select(s => s.PACK).Distinct());
            pg.AddIndividuals(ultrasoundData.Select(s => new Individual {Name = s.INDIV}).Distinct());

            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();
            AddPackHistories(
                ultrasoundData.Select(
                    ultrasound => new PackHistoryDto(ultrasound.INDIV, ultrasound.PACK, ultrasound.DATE)), pgPacks,
                pgIndividuals, pg);

            AddUltrasoundData(ultrasoundData, pgIndividuals, pg);
            Logger.Info("Done adding ultrasound data.");
        }

        public void ProccessRadioCollarData(IEnumerable<RadioCollar> radioCollarData)
        {
            Logger.Info("Starting to add radio collar data.");
            radioCollarData = radioCollarData as IList<RadioCollar> ?? radioCollarData.ToList();
            var pg = new PostgresRepository();
            pg.AddPacks(radioCollarData.Select(s => s.PACK).Distinct());
            pg.AddIndividuals(radioCollarData.Select(s => new Individual {Name = s.INDIVIDUAL}).Distinct());

            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();

            AddPackHistories(
                radioCollarData.Select(collar => new PackHistoryDto(collar.INDIVIDUAL, collar.PACK,
                    GetMinimumDateFromRadioCollar(collar))), pgPacks,
                pgIndividuals, pg);

            AddRadioCollarData(radioCollarData, pgIndividuals, pg);

            Logger.Info("Done adding radio collar data.");
        }

        private void AddRadioCollarData(IEnumerable<RadioCollar> radioCollarData, List<Individual> pgIndividuals,
            PostgresRepository pg)
        {
            foreach (var radioCollar in radioCollarData)
            {
                if (string.IsNullOrEmpty(radioCollar.INDIVIDUAL))
                {
                    Logger.Warn("individual name null");
                    continue;
                }
                var individualId = pgIndividuals.Single(i => i.Name == radioCollar.INDIVIDUAL).IndividualId;

                pg.AddRadioCollar(individualId, radioCollar.FITTED, radioCollar.TURNED_ON, radioCollar.REMOVED,
                    radioCollar.FREQUENCY,
                    radioCollar.WEIGHT, radioCollar.DATE_ENTERED, radioCollar.COMMENT);
            }
        }

        private static DateTime GetMinimumDateFromRadioCollar(RadioCollar ph)
        {
            return new List<DateTime?> {ph.DATE_ENTERED, ph.FITTED, ph.REMOVED, ph.TURNED_ON}.Min()
                .GetValueOrDefault();
        }

        private void AddUltrasoundData(IEnumerable<Ultrasound> ultrasoundData, List<Individual> pgIndividuals,
            PostgresRepository pg)
        {
            pg.RemoveUltrasoundData();
            foreach (var ultrasound in ultrasoundData)
            {
                var individualId = pgIndividuals.Single(i => i.Name == ultrasound.INDIV).IndividualId;

                for (var i = 1; i <= 6; i++)
                {
                    if (i == 1)
                    {
                        if (!(ultrasound.FOETUS_1_CROSS_VIEW_WIDTH == null &&
                              ultrasound.FOETUS_1_CROSS_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_1_LONG_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_1_LONG_VIEW_WIDTH == null))
                        {
                            Logger.Info($"Adding ultrasound data indiviudal:{ultrasound.INDIV} Foetus:{i}");
                            pg.AddFoetus(individualId, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
                                ultrasound.FOETUS_1_CROSS_VIEW_WIDTH,
                                ultrasound.FOETUS_1_CROSS_VIEW_LENGTH,
                                ultrasound.FOETUS_1_LONG_VIEW_LENGTH,
                                ultrasound.FOETUS_1_LONG_VIEW_WIDTH, ultrasound.COMMENT, ultrasound.OBSERVER);
                        }
                    }

                    if (i == 2)
                    {
                        if (!(ultrasound.FOETUS_2_CROSS_VIEW_WIDTH == null &&
                              ultrasound.FOETUS_2_CROSS_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_2_LONG_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_2_LONG_VIEW_WIDTH == null))
                        {
                            Logger.Info($"Adding ultrasound data indiviudal:{ultrasound.INDIV} Foetus:{i}");
                            pg.AddFoetus(individualId, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
                                ultrasound.FOETUS_2_CROSS_VIEW_WIDTH,
                                ultrasound.FOETUS_2_CROSS_VIEW_LENGTH,
                                ultrasound.FOETUS_2_LONG_VIEW_LENGTH,
                                ultrasound.FOETUS_2_LONG_VIEW_WIDTH, ultrasound.COMMENT, ultrasound.OBSERVER);
                        }
                    }

                    if (i == 3)
                    {
                        if (!(ultrasound.FOETUS_3_CROSS_VIEW_WIDTH == null &&
                              ultrasound.FOETUS_3_CROSS_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_3_LONG_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_3_LONG_VIEW_WIDTH == null))
                        {
                            Logger.Info($"Adding ultrasound data indiviudal:{ultrasound.INDIV} Foetus:{i}");
                            pg.AddFoetus(individualId, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
                                ultrasound.FOETUS_3_CROSS_VIEW_WIDTH,
                                ultrasound.FOETUS_3_CROSS_VIEW_LENGTH,
                                ultrasound.FOETUS_3_LONG_VIEW_LENGTH,
                                ultrasound.FOETUS_3_LONG_VIEW_WIDTH, ultrasound.COMMENT, ultrasound.OBSERVER);
                        }
                    }

                    if (i == 4)
                    {
                        if (!(ultrasound.FOETUS_4_CROSS_VIEW_WIDTH == null &&
                              ultrasound.FOETUS_4_CROSS_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_4_LONG_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_4_LONG_VIEW_WIDTH == null))
                        {
                            Logger.Info($"Adding ultrasound data indiviudal:{ultrasound.INDIV} Foetus:{i}");
                            pg.AddFoetus(individualId, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
                                ultrasound.FOETUS_4_CROSS_VIEW_WIDTH,
                                ultrasound.FOETUS_4_CROSS_VIEW_LENGTH,
                                ultrasound.FOETUS_4_LONG_VIEW_LENGTH,
                                ultrasound.FOETUS_4_LONG_VIEW_WIDTH, ultrasound.COMMENT, ultrasound.OBSERVER);
                        }
                    }

                    if (i == 5)
                    {
                        if (!(ultrasound.FOETUS_5_CROSS_VIEW_WIDTH == null &&
                              ultrasound.FOETUS_5_CROSS_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_5_LONG_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_5_LONG_VIEW_WIDTH == null))
                        {
                            Logger.Info($"Adding ultrasound data indiviudal:{ultrasound.INDIV} Foetus:{i}");
                            pg.AddFoetus(individualId, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
                                ultrasound.FOETUS_5_CROSS_VIEW_WIDTH,
                                ultrasound.FOETUS_5_CROSS_VIEW_LENGTH,
                                ultrasound.FOETUS_5_LONG_VIEW_LENGTH,
                                ultrasound.FOETUS_5_LONG_VIEW_WIDTH, ultrasound.COMMENT, ultrasound.OBSERVER);
                        }
                    }

                    if (i == 6)
                    {
                        if (!(ultrasound.FOETUS_6_CROSS_VIEW_WIDTH == null &&
                              ultrasound.FOETUS_6_CROSS_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_6_LONG_VIEW_LENGTH == null &&
                              ultrasound.FOETUS_6_LONG_VIEW_WIDTH == null))
                        {
                            Logger.Info($"Adding ultrasound data indiviudal:{ultrasound.INDIV} Foetus:{i}");
                            pg.AddFoetus(individualId, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
                                ultrasound.FOETUS_6_CROSS_VIEW_WIDTH,
                                ultrasound.FOETUS_6_CROSS_VIEW_LENGTH,
                                ultrasound.FOETUS_6_LONG_VIEW_LENGTH,
                                ultrasound.FOETUS_6_LONG_VIEW_WIDTH, ultrasound.COMMENT, ultrasound.OBSERVER);
                        }
                    }
                }
            }
        }

        private void AddPackHistories(IEnumerable<PackHistoryDto> packHistorys, IEnumerable<Pack> pgPacks,
            IEnumerable<Individual> pgIndividuals, PostgresRepository pg)
        {
            //select and see if we have an entry

            foreach (var membership in packHistorys.OrderByDescending(ph => ph.DateJoined))
            {
                if (string.IsNullOrEmpty(membership.IndividualName) || string.IsNullOrEmpty(membership.PackName))
                {
                    Logger.Warn(
                        $"Null found entering pack history. pack name:{membership.PackName} individual name {membership.IndividualName}");
                    continue;
                }
                var packId = pgPacks.Single(p => p.Name == membership.PackName).PackId;
                var individualId = pgIndividuals.Single(i => i.Name == membership.IndividualName).IndividualId;

                var databasePackHistory = pg.GetPackHistory(packId, individualId);

                if (databasePackHistory != null)
                {
                    if (databasePackHistory.DateJoined > membership.DateJoined)
                    {
                        pg.UpdatePackHistory(membership, databasePackHistory);
                    }
                }
                else
                {
                    // if not insert new info
                    pg.InsertPackHistory(packId, individualId, membership);
                }
            }
        }
    }
}