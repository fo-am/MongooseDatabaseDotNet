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

            pg.AddWeights(weights);
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

            AddUltrasoundData(ultrasoundData, pg);
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

        public void ProcessLifeHistories(IEnumerable<NewLifeHistory> lifeHistories)
        {
            Logger.Info("Starting to add life history data.");
            lifeHistories = lifeHistories as IList<NewLifeHistory> ?? lifeHistories.ToList();
            var pg = new PostgresRepository();

            foreach (var lifeHistory in lifeHistories)
            {
                var duplicateCount = 0;
                if (lifeHistory.Litter == null && lifeHistory.Pack == null && lifeHistory.Indiv == null)
                {
                    Logger.Warn("No valid litter, pack or individual for life history event.");
                    continue;
                }
                if (LifeHistoryIsLitterEvent(lifeHistory))
                {
                    // these are ignored as they are probably bad data.
                    Logger.Info("Litter Event");
                    duplicateCount++;
                    if (lifeHistory.Litter == "ESG0903")
                    {
                        Logger.Warn("No pack id for litter name ESG0903");
                        return;
                    }
                    // Add pack info
                    pg.InsertSinglePack(lifeHistory.Pack);
                    var packId = pg.GetPackId(lifeHistory.Pack);
                    // add litter info
                    pg.InsertSingleLitter(lifeHistory.Litter, packId);
                    int litterId = pg.GetLitterId(lifeHistory.Litter);

                    // add litter event
                    pg.AddLitterEventCode(lifeHistory.Code);
                    int litterCodeId = pg.GetLitterEventCodeId(lifeHistory.Code);
                    // link litter event to litter.
                    pg.LinkLitterEvent(litterId, litterCodeId, lifeHistory);
                }
                if (LifeHistoryIsPackEvent(lifeHistory))
                {
                    Logger.Info("Pack Event");
                    duplicateCount++;

                    pg.InsertSinglePack(lifeHistory.Pack);
                    var packId = pg.GetPackId(lifeHistory.Pack);

                    pg.AddPackEventCode(lifeHistory.Code);

                    // get pack codes and ids
                    var packEventCodeId = pg.GetPackEventCodeId(lifeHistory.Code);

                    // link packs to codes.
                    if (lifeHistory.Code == "IGI")
                    {
                        Logger.Info("event is IGI");
                        // we have an intergroup interaction! CODE RED!
                        pg.AddIGI(lifeHistory);
                        continue;
                    }

                    pg.LinkPackEvents(packId,
                        packEventCodeId,
                        lifeHistory.Status, lifeHistory.Date,
                        lifeHistory.Exact, lifeHistory.Cause,
                        lifeHistory.Comment, lifeHistory.Latitude, lifeHistory.Longitude);

                    // record pack event
                }
                if (LifeHistoryIsIndividualEvent(lifeHistory))
                {
                    Logger.Info("Individual Event");
                    duplicateCount++;

                    pg.InsertIndividual(new Individual {Name = lifeHistory.Indiv, Sex = lifeHistory.Sex});

                    pg.InsertSinglePack(lifeHistory.Pack);
                    //add individual event code
                    // get individual code

                    var packId = pg.GetPackId(lifeHistory.Pack);

                    var individualId = pg.GetIndividualId(lifeHistory.Indiv);

                    InsertpackHistory(packId, individualId, lifeHistory.Date, pg);

                    pg.AddLitter(new LifeHistoryDto
                    {
                        pgPackId = packId,
                        Litter = lifeHistory.Litter,
                        pgIndividualId = individualId
                    });

                    // get ids
                    // record lifehistory
                    var pack_history_id = pg.GetPackHistoryId(lifeHistory.Pack, lifeHistory.Indiv);
                    var individual_event_code_id = pg.GetIndiviudalEventCodeId(lifeHistory.Code);

                    pg.LinkIndividualEvents(pack_history_id,
                        individual_event_code_id,
                        lifeHistory.Latitude, lifeHistory.Longitude, lifeHistory.Status,
                        lifeHistory.Date, lifeHistory.Exact, lifeHistory.Cause, lifeHistory.Comment);
                }

                if (duplicateCount == 0)
                {
                    Logger.Error($"LifeHistory type not determined:{lifeHistory}");
                }
                if (duplicateCount > 1)
                {
                    Logger.Error($"LifeEvent was of multiple types:{lifeHistory}");
                    Console.WriteLine("Too many types");
                }
            }

            //pg.AddPacks(lifeHistories.Select(s => s.Pack).Distinct());
            //pg.AddIndividuals(lifeHistories.GroupBy(lh => new {lh.Indiv, lh.Sex})
            //    .Select(s => new Individual {Name = s.Key.Indiv, Sex = s.Key.Sex}));

            //var pgPacks = pg.GetAllPacks();
            //var pgIndividuals = pg.GetAllIndividuals();

            //AddPackHistories(
            //    lifeHistories.Select(lh => new PackHistoryDto(lh.Indiv, lh.Pack, lh.Date)), pgPacks,
            //    pgIndividuals, pg);

            //AddLitterInfo(lifeHistories.GroupBy(l => new {l.Pack, l.Indiv, l.Litter}).Select(
            //            l => new LifeHistoryDto {Pack = l.Key.Pack, Individual = l.Key.Indiv, Litter = l.Key.Litter})
            //        .ToList(),
            //    pg);

            //AddLitterEvents(lifeHistories.Where(l => string.IsNullOrEmpty(l.Pack) && string.IsNullOrEmpty(l.Indiv) &&
            //                                         !string.IsNullOrEmpty(l.Code)));
            //AddPackEvents( // note this contains IGI between packs... need to pull them out seperatly or something.
            //    lifeHistories.Where(l => !string.IsNullOrEmpty(l.Pack) && string.IsNullOrEmpty(l.Indiv) &&
            //                             !string.IsNullOrEmpty(l.Code)), pg);
            //AddIndividualEvents(lifeHistories.Where(l => !string.IsNullOrEmpty(l.Indiv) &&
            //                                             !string.IsNullOrEmpty(l.Code) &&
            //                                             !string.IsNullOrEmpty(l.Pack)), pg);

            Logger.Info("Done adding life history data.");
        }

        private static bool LifeHistoryIsIndividualEvent(NewLifeHistory lifeHistory)
        {
            return !string.IsNullOrEmpty(lifeHistory.Indiv) &&
                   !string.IsNullOrEmpty(lifeHistory.Code) &&
                   !string.IsNullOrEmpty(lifeHistory.Pack);
        }

        private static bool LifeHistoryIsPackEvent(NewLifeHistory lifeHistory)
        {
            return !string.IsNullOrEmpty(lifeHistory.Pack) && string.IsNullOrEmpty(lifeHistory.Indiv) &&
                   !string.IsNullOrEmpty(lifeHistory.Code);
        }

        private static bool LifeHistoryIsLitterEvent(NewLifeHistory lifeHistory)
        {
            // if pack and indiv are null but litter and code are filled
            // also if indiv matches litter and has a value and code is filled.

            return (string.IsNullOrEmpty(lifeHistory.Pack) && string.IsNullOrEmpty(lifeHistory.Indiv)) ||
                   (lifeHistory.Indiv == lifeHistory.Litter && !string.IsNullOrEmpty(lifeHistory.Indiv)) &&
                   !string.IsNullOrEmpty(lifeHistory.Code);
        }

        private void AddIndividualEvents(IEnumerable<NewLifeHistory> individualEvents, PostgresRepository pg)
        {
            // add individual event codes
            pg.AddIndividualEventCodes(individualEvents.Select(e => e.Code).Distinct());

            //get all the codes
            var pgIndividualCodes = pg.GetIndividualCodes();
            // get individuals
            var pgIndividuals = pg.GetAllIndividuals();

            // add events with individual ids added
            foreach (var individualEvent in individualEvents)
            {
                var pack_history_id = pg.GetPackHistoryId(individualEvent.Pack, individualEvent.Indiv);

                pg.LinkIndividualEvents(pack_history_id,
                    pgIndividualCodes.Single(ic => ic.Code == individualEvent.Code).IndividualEventCodeId,
                    individualEvent.Latitude, individualEvent.Longitude, individualEvent.Status,
                    individualEvent.Date, individualEvent.Exact, individualEvent.Cause, individualEvent.Comment);
            }
        }

        private void AddPackEvents(IEnumerable<NewLifeHistory> packEvents, PostgresRepository pg)
        {
            pg.AddPackEventCodes(packEvents.Select(e => e.Code).Distinct());
            // get pack codes and ids
            var pgPackCodes = pg.GetPackEventCodes();
            // get packs and ids
            var pgPacks = pg.GetAllPacks();
            // link packs to codes.
            foreach (var packEvent in packEvents)
            {
                pg.LinkPackEvents(pgPacks.Single(p => p.Name == packEvent.Pack).PackId,
                    pgPackCodes.Single(p => p.Code == packEvent.Code).PackEventCodeId, packEvent.Status, packEvent.Date,
                    packEvent.Exact, packEvent.Cause,
                    packEvent.Comment, packEvent.Latitude, packEvent.Longitude);
            }
        }

        private void AddLitterEvents(IEnumerable<NewLifeHistory> litterEvents)
        {
            // do this some other time, there are 3 in the database and look like bad data.
        }

        private void AddLitterInfo(IEnumerable<LifeHistoryDto> litters, PostgresRepository pg)
        {
            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();

            foreach (var litter in litters)
            {
                if (string.IsNullOrEmpty(litter.Pack) || string.IsNullOrEmpty(litter.Individual) ||
                    string.IsNullOrEmpty(litter.Litter))
                {
                    Logger.Warn(
                        $"Something was null for this litter. pack:{litter.Pack} Individual:{litter.Individual} Litter {litter.Litter}");
                    continue;
                }
                litter.pgIndividualId = pgIndividuals.Single(i => i.Name == litter.Individual).IndividualId;
                litter.pgPackId = pgPacks.Single(p => p.Name == litter.Pack).PackId;

                pg.AddLitter(litter);
            }
        }

        private void AddRadioCollarData(IEnumerable<RadioCollar> radioCollarData, List<Individual> pgIndividuals,
            PostgresRepository pg)
        {
            pg.RemoveRadioCollarData();

            foreach (var radioCollar in radioCollarData)
            {
                if (string.IsNullOrEmpty(radioCollar.INDIVIDUAL))
                {
                    Logger.Warn("individual name null");
                    continue;
                }
                var pack_history_id =
                    pg.GetPackHistoryId(radioCollar.PACK,
                        radioCollar
                            .INDIVIDUAL); //pgIndividuals.Single(i => i.Name == radioCollar.INDIVIDUAL).IndividualId;

                pg.AddRadioCollar(pack_history_id, radioCollar.FITTED, radioCollar.TURNED_ON, radioCollar.REMOVED,
                    radioCollar.FREQUENCY,
                    radioCollar.WEIGHT, radioCollar.DATE_ENTERED, radioCollar.COMMENT);
            }
        }

        private static DateTime GetMinimumDateFromRadioCollar(RadioCollar ph)
        {
            return new List<DateTime?> {ph.DATE_ENTERED, ph.FITTED, ph.REMOVED, ph.TURNED_ON}.Min()
                .GetValueOrDefault();
        }

        private void AddUltrasoundData(IEnumerable<Ultrasound> ultrasoundData, PostgresRepository pg)
        {
            pg.RemoveUltrasoundData();
            foreach (var ultrasound in ultrasoundData)
            {
                var pack_history_id = pg.GetPackHistoryId(ultrasound.PACK, ultrasound.INDIV);

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
                            pg.AddFoetus(pack_history_id, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
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
                            pg.AddFoetus(pack_history_id, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
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
                            pg.AddFoetus(pack_history_id, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
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
                            pg.AddFoetus(pack_history_id, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
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
                            pg.AddFoetus(pack_history_id, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
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
                            pg.AddFoetus(pack_history_id, i, ultrasound.DATE, ultrasound.FOETUS_SIZE,
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
                var packId = pgPacks.Single(p => p.Name == membership.PackName).PackId;
                var individualId = pgIndividuals.Single(i => i.Name == membership.IndividualName).IndividualId;

                InsertpackHistory(packId, individualId, membership.DateJoined, pg);
            }
        }

        private void InsertpackHistory(int packId, int? individualId, DateTime? date, PostgresRepository pg)
        {
            var databasePackHistory = pg.GetPackHistory(packId, individualId);

            if (databasePackHistory != null && date.HasValue)
            {
                if (databasePackHistory.DateJoined > date)
                {
                    pg.UpdatePackHistoryDate(date.Value, databasePackHistory);
                }
            }
            else
            {
                // if not insert new info
                pg.InsertPackHistory(packId, individualId, date);
            }
        }

        public void ProcessOestrusData(IEnumerable<Oestrus> oestruses)
        {
            Logger.Info("Starting to add life history data.");
            oestruses = oestruses as IList<Oestrus> ?? oestruses.ToList();

            var pg = new PostgresRepository();
            foreach (var oestrus in oestruses)
            {
                if (string.IsNullOrEmpty(oestrus.FEMALE_ID))
                {
                    Logger.Error("null femail id.");
                    continue;
                }
                //Insert main female
                pg.InsertIndividual(new Individual {Name = oestrus.FEMALE_ID, Sex = "F"});
                var individualId = pg.GetIndividualId(oestrus.FEMALE_ID);

                // add individual to group
                pg.InsertSinglePack(oestrus.GROUP);
                var packId = pg.GetPackId(oestrus.GROUP);

                InsertpackHistory(packId, individualId, oestrus.DATE, pg);

                // add indiv from guard
                pg.InsertIndividual(new Individual {Name = oestrus.GUARD_ID});

                // add individuals from pesterer 1-4
                pg.InsertIndividual(new Individual {Name = oestrus.PESTERER_ID});
                pg.InsertIndividual(new Individual {Name = oestrus.PESTERER_ID_2});
                pg.InsertIndividual(new Individual {Name = oestrus.PESTERER_ID_3});
                pg.InsertIndividual(new Individual {Name = oestrus.PESTERER_ID_4});

                // add individual from copulation
                pg.InsertIndividual(new Individual {Name = oestrus.COPULATION});

                pg.AddOestrusEvent(oestrus);
                // add oestrus record, add pesterers (many-many)
            }
        }

        public void ProcessCaptures(List<CapturesNew2013> captures)
        {
            var pg = new PostgresRepository();
            foreach (var capture in captures)
            {
                if (string.IsNullOrEmpty(capture.PACK) && string.IsNullOrEmpty(capture.INDIV))
                {
                    Logger.Info("Capture with no pack or individual.");
                   continue; 
                }
                // do individual
                pg.InsertIndividual(new Individual {Name = capture.INDIV, Sex = capture.SEX});
                var individualId = pg.GetIndividualId(capture.INDIV);
                // do pack
                if (string.IsNullOrEmpty(capture.PACK))
                {
                    capture.PACK = "Unknown";
                }
                else
                {
                    pg.InsertSinglePack(capture.PACK);
                }

                var packid = pg.GetPackId(capture.PACK);

                // Link Pack and Individual
                InsertpackHistory(packid, individualId, capture.Capture_DATE, pg);

                var packHistoryId = pg.GetPackHistoryId(capture.PACK, capture.INDIV);
                // do transponder

                pg.AddTransponder(capture.TRANSPONDER, individualId);

                if (capture.Capture_DATE != null)
                {
                    pg.AddCaptureData(capture, packHistoryId);
                }
            }
        }

        public void AddStaticData()
        {
            var pg = new PostgresRepository();
            pg.InsertSinglePack("Unknown");
            // add unknown pack
            // add unknown (other things)
        }
    }
}