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
            pg.AddIndividuals(weights.GroupBy(s => new { Name = s.Indiv, s.Sex })
                .Select(i => new Individual { Name = i.Key.Name, Sex = i.Key.Sex }));

            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();
            AddPackHistories(
                weights.Select(weight => new PackHistoryDto(weight.Indiv, weight.Group, weight.TimeMeasured)), pgPacks,
                pgIndividuals, pg);

            pg.AddWeights(weights);
            Logger.Info("Done adding weights.");
        }

        public void ProcessUltrasoundData(IEnumerable<Ultrasound> ultrasoundData)
        {
            Logger.Info("Starting to add ultrasound data.");
            ultrasoundData = ultrasoundData as IList<Ultrasound> ?? ultrasoundData.ToList();
            var pg = new PostgresRepository();
            pg.AddPacks(ultrasoundData.Select(s => s.PACK).Distinct());
            pg.AddIndividuals(ultrasoundData.Select(s => new Individual { Name = s.INDIV }).Distinct());

            var pgPacks = pg.GetAllPacks();
            var pgIndividuals = pg.GetAllIndividuals();
            AddPackHistories(
                ultrasoundData.Select(
                    ultrasound => new PackHistoryDto(ultrasound.INDIV, ultrasound.PACK, ultrasound.DATE)), pgPacks,
                pgIndividuals, pg);

            AddUltrasoundData(ultrasoundData, pg);
            Logger.Info("Done adding ultrasound data.");
        }

        public void ProcessRadioCollarData(IEnumerable<RadioCollar> radioCollarData)
        {
            Logger.Info("Starting to add radio collar data.");
            var pg = new PostgresRepository();

            foreach (var radioCollar in radioCollarData)
            {
                // do individual
                if (string.IsNullOrEmpty(radioCollar.INDIVIDUAL))
                {
                    radioCollar.INDIVIDUAL = "Unknown";
                }

                pg.InsertIndividual(new Individual { Name = radioCollar.INDIVIDUAL });
                var individualId = pg.GetIndividualId(radioCollar.INDIVIDUAL);

                // do pack
                if (string.IsNullOrEmpty(radioCollar.PACK))
                {
                    radioCollar.PACK = "Unknown";
                }

                pg.InsertSinglePack(radioCollar.PACK);

                var packid = pg.GetPackId(radioCollar.PACK);

                // Link Pack and Individual
                InsertpackHistory(packid, individualId, radioCollar.DATE_ENTERED, pg);

                var packHistoryId = pg.GetPackHistoryId(radioCollar.PACK, radioCollar.INDIVIDUAL);

                // add the data!
                pg.AddRadioCollar(packHistoryId, radioCollar.FITTED, radioCollar.TURNED_ON, radioCollar.REMOVED,
                    radioCollar.FREQUENCY,
                    radioCollar.WEIGHT, radioCollar.DATE_ENTERED, radioCollar.COMMENT);
            }

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
                    var litterId = pg.GetLitterId(lifeHistory.Litter).Value;

                    // add litter event
                    pg.AddLitterEventCode(lifeHistory.Code);
                    var litterCodeId = pg.GetLitterEventCodeId(lifeHistory.Code);
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

                    pg.InsertIndividual(new Individual { Name = lifeHistory.Indiv, Sex = lifeHistory.Sex });

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
            return !string.IsNullOrEmpty(lifeHistory.Pack) &&
                   string.IsNullOrEmpty(lifeHistory.Indiv) &&
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

        private static DateTime GetMinimumDateFromRadioCollar(RadioCollar ph)
        {
            return new List<DateTime?> { ph.DATE_ENTERED, ph.FITTED, ph.REMOVED, ph.TURNED_ON }.Min()
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
                pg.InsertIndividual(new Individual { Name = oestrus.FEMALE_ID, Sex = "F" });
                var individualId = pg.GetIndividualId(oestrus.FEMALE_ID);

                // add individual to group
                pg.InsertSinglePack(oestrus.GROUP);
                var packId = pg.GetPackId(oestrus.GROUP);

                InsertpackHistory(packId, individualId, oestrus.DATE, pg);

                // add indiv from guard
                pg.InsertIndividual(new Individual { Name = oestrus.GUARD_ID });

                // add individuals from pesterer 1-4
                pg.InsertIndividual(new Individual { Name = oestrus.PESTERER_ID });
                pg.InsertIndividual(new Individual { Name = oestrus.PESTERER_ID_2 });
                pg.InsertIndividual(new Individual { Name = oestrus.PESTERER_ID_3 });
                pg.InsertIndividual(new Individual { Name = oestrus.PESTERER_ID_4 });

                // add individual from copulation
                pg.InsertIndividual(new Individual { Name = oestrus.COPULATION });

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
                pg.InsertIndividual(new Individual { Name = capture.INDIV, Sex = capture.SEX });
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

        public void ProcessPups(List<PupAssociation> pups)
        {
            var pg = new PostgresRepository();
            foreach (var pupAssociation in pups)
            {
                var packId = pg.TryGetPackId(pupAssociation.GROUP);

                if (packId == null)
                {
                    pg.InsertSinglePack(pupAssociation.GROUP);
                    packId = pg.GetPackId(pupAssociation.GROUP);
                }

                var pupLitter = pg.GetLitterId(pupAssociation.LITTER);
                if (pupLitter == null && !string.IsNullOrEmpty(pupAssociation.LITTER))
                {
                    pg.InsertSingleLitter(pupAssociation.LITTER, packId.Value);
                    pupLitter = pg.GetLitterId(pupAssociation.LITTER);
                }

                pg.InsertIndividual(new Individual
                {
                    Name = pupAssociation.PUP,
                    Sex = pupAssociation.PUP_SEX,
                    LitterId = pupLitter
                });

                var pupId = pg.GetIndividualId(pupAssociation.PUP);
                // get pack record
                pg.InsertPackHistory(packId.Value, pupId, pupAssociation.DATE);
                var packHistoryId = pg.GetPackHistoryId(pupAssociation.GROUP, pupAssociation.PUP);

                // get escort id

                var escortId = GetEscortId(pupAssociation, pg);

                // put everything in the pups table.
                pg.InsertPupAssociation(packHistoryId, escortId, pupAssociation);
            }
        }

        private int? GetEscortId(PupAssociation pupAssociation, PostgresRepository pg)
        {
            if (pupAssociation.ESCORT == "NA" || pupAssociation.ESCORT == "REMOVED" ||
                string.IsNullOrEmpty(pupAssociation.ESCORT))
            {
                return null;
            }

            pg.InsertIndividual(new Individual
            {
                Name = pupAssociation.ESCORT,
                Sex = pupAssociation.ESC_SEX
            });
            var escortId = pg.GetIndividualId(pupAssociation.ESCORT);

            return escortId;
        }

        public void ProcessBabysittingRecords(List<BABYSITTING_RECORDS> babysittingRecords)
        {
            // add pack that is being watched
            // add litter being watched
            // add individual being watched. (with pack and sex... NOT litter)
            // if no individual then add null, then add unk, most, all to somewhere else??
            // process times (if -1 null, if number then hours. if split on . then hours mins) 830, 836 and 1505 are special numbers.

            var pg = new PostgresRepository();
            foreach (var babysitting in babysittingRecords)
            {
                var packId = pg.TryGetPackId(babysitting.GROUP);

                if (packId == null)
                {
                    pg.InsertSinglePack(babysitting.GROUP);
                    packId = pg.GetPackId(babysitting.GROUP);
                }

                var watchedLitter = pg.GetLitterId(babysitting.LITTER_CODE);
                if (watchedLitter == null && !string.IsNullOrEmpty(babysitting.LITTER_CODE))
                {
                    pg.InsertSingleLitter(babysitting.LITTER_CODE, packId.Value);
                    watchedLitter = pg.GetLitterId(babysitting.LITTER_CODE);
                }

                pg.InsertIndividual(new Individual
                {
                    Name = babysitting.BS,
                    Sex = babysitting.SEX
                });

                var babysitterId = pg.GetIndividualId(babysitting.BS);

                pg.InsertPackHistory(packId.Value, babysitterId, babysitting.DATE);
                var packHistoryId = pg.GetPackHistoryId(babysitting.GROUP, babysitting.BS);

                var startTime = GetTimeFromString(babysitting.TIME_START);
                var endTime = GetTimeFromString(babysitting.TIME_END);

                var denDistance = GetDenDistance(babysitting.DEN_DIST);

                pg.InsertBabysittingRecord(packHistoryId, watchedLitter, startTime, endTime, denDistance, babysitting);
            }
        }

        private int? GetDenDistance(string babysittingDenDist)
        {
            if (string.IsNullOrEmpty(babysittingDenDist) || babysittingDenDist == "-1" || babysittingDenDist == "1-" ||
                babysittingDenDist == "'" || babysittingDenDist == "UNK")
            {
                return null;
            }

            if (babysittingDenDist == ">10")
            {
                return 10;
            }

            if (babysittingDenDist == "400M")
            {
                return 400;
            }

            return int.Parse(babysittingDenDist);
        }

        private static DateTime? GetTimeFromString(string timeString)
        {
            //Times are strings that need to be turned into datetime times.
            // Example times are 
            // 12  => 12:00
            // 13.4 => 13:40
            // 3.32 => 3:32
            // there are some special numbers that are picked out individualy.
            // -1 and empty strings are nulls in the database.

            DateTime? timestart = DateTime.MinValue;

            if (timeString == "-1" || string.IsNullOrEmpty(timeString))
            {
                timestart = null;
            }
            else
            {
                var tims = Array.ConvertAll(timeString.Split('.'), s => int.Parse(s));
                if (tims.Length == 2)
                {
                    timestart = timestart.Value.AddHours(tims[0]);
                    var minutes = tims[1];
                    if (minutes < 10)
                    {
                        minutes = minutes * 10;
                    }

                    timestart = timestart.Value.AddMinutes(minutes);
                }

                if (tims.Length == 1)
                {
                    timestart = timestart.Value.AddHours(tims[0]);
                }
            }

            //830, 836 and 1505 
            if (timeString == "830")
            {
                timestart = new DateTime().AddHours(8).AddMinutes(30);
            }

            if (timeString == "836")
            {
                timestart = new DateTime().AddHours(8).AddMinutes(36);
            }

            if (timeString == "1705")
            {
                timestart = new DateTime().AddHours(17).AddMinutes(5);
            }

            return timestart;
        }

        public void ProcessGroupCompositions(IEnumerable<DiaryAndGrpComposition> groupCompositions)
        {
            var pg = new PostgresRepository();
            foreach (var groupComposition in groupCompositions)
            {
                if (string.IsNullOrEmpty(groupComposition.Pack))
                {
                    continue;
                }

                var packId = pg.TryGetPackId(groupComposition.Pack);

                if (packId == null)
                {
                    pg.InsertSinglePack(groupComposition.Pack);
                    packId = pg.GetPackId(groupComposition.Pack);
                }

                int? males_over_one_year = GetNumber(groupComposition.Males_one_yr);
                int? females_over_one_year = GetNumber(groupComposition.Females_one_yr);
                int? males_over_three_months = GetNumber(groupComposition.Males_three_months);
                int? females_over_three_months = GetNumber(groupComposition.Females_three_months);
                // int? male_pups = GetNumber(groupComposition.Male_em_pups);
                // int? female_pups = GetNumber(groupComposition.Female_em_pups);

                pg.InsertGroupComposition(packId, males_over_one_year, females_over_one_year, males_over_three_months,
                    females_over_three_months, groupComposition);
            }
        }

        private int? GetNumber(string numberString)
        {
            if (string.IsNullOrEmpty(numberString) || numberString == "UNK" || numberString == "-1")
            {
                return null;
            }

            if (int.TryParse(numberString, out int number))
            {
                return number;
            }

            return null;
        }

        public void ProcessPoo(IEnumerable<POO_DATABASE> pooSamples)
        {
            var pg = new PostgresRepository();
            foreach (var pooSample in pooSamples)
            {
                if (string.IsNullOrEmpty(pooSample.Pack) && string.IsNullOrEmpty(pooSample.Individual))
                {
                    Logger.Info("Capture with no pack or individual.");
                    continue;
                }

                // do individual
                if (string.IsNullOrEmpty(pooSample.Individual))
                {
                    pooSample.Individual = "Unknown";
                }

                pg.InsertIndividual(new Individual { Name = pooSample.Individual });
                var individualId = pg.GetIndividualId(pooSample.Individual);

                // do pack
                if (string.IsNullOrEmpty(pooSample.Pack))
                {
                    pooSample.Pack = "Unknown";
                }

                pg.InsertSinglePack(pooSample.Pack);

                var packid = pg.GetPackId(pooSample.Pack);

                // Link Pack and Individual
                InsertpackHistory(packid, individualId, pooSample.Date, pg);

                var packHistoryId = pg.GetPackHistoryId(pooSample.Pack, pooSample.Individual);

                var emergenceTime = GetDateTime(pooSample.Emergence_Time);
                var timeOfCollection = GetDateTime(pooSample.Time_of_Collection);

                pg.InsertPooRecord(packHistoryId, emergenceTime, timeOfCollection, pooSample);
            }
        }

        private DateTime? GetDateTime(string time)
        {
            if (string.IsNullOrEmpty(time) || time == "UNK" || time == "PM" || time == "-1")
            {
                return null;
            }

            time = time.Replace(';', ':');

            if (DateTime.TryParse(time, out var date))
            {
                return date;
            }

            if (time == "1500")
            {
                return new DateTime().AddHours(15).AddMinutes(00);
            }

            return null;
        }

        public void ProcessWeather(List<METEROLOGICAL_DATA> weatherData)
        {
            var pg = new PostgresRepository();
            foreach (var meterologicalData in weatherData)
            {
                pg.InsertWeather(meterologicalData);
            }
        }

        public void ProcessConditionLitters(List<Maternal_Condition_Experiment_Litters> conditionLitters)
        {
            var pg = new PostgresRepository();
            foreach (var litter in conditionLitters)
            {
                if (string.IsNullOrEmpty(litter.Pack) && string.IsNullOrEmpty(litter.Litter))
                {
                    Logger.Error("No pack or Litter");
                    continue;
                }

                // do pack
                if (string.IsNullOrEmpty(litter.Pack))
                {
                    litter.Pack = "Unknown";
                }

                pg.InsertSinglePack(litter.Pack);
                var packId = pg.GetPackId(litter.Pack);

                // Create litter
                pg.InsertSingleLitter(litter.Litter, packId);

                var litterId = pg.GetLitterId(litter.Litter);

                pg.AddConditioningLitter(litterId, litter);
            }
        }

        public void ProcessConditionFemales(List<Maternal_Condition_Experiment_Females> conditionFemales)
        {
         var pg = new PostgresRepository();
            foreach (var female in conditionFemales)
            {
                if (string.IsNullOrEmpty(female.Pack) && string.IsNullOrEmpty(female.Female_ID))
                {
                    Logger.Info("experiment with no pack or individual.");
                    continue;
                }

                // do individual
                if (string.IsNullOrEmpty(female.Female_ID))
                {
                    female.Female_ID = "Unknown";
                }

                pg.InsertIndividual(new Individual { Name =female.Female_ID });
                var individualId = pg.GetIndividualId(female.Female_ID);

                // do pack
                if (string.IsNullOrEmpty(female.Pack))
                {
                    female.Pack = "Unknown";
                }

                pg.InsertSinglePack(female.Pack);

                var packId = pg.GetPackId(female.Pack);

                // Link Pack and Individual
                InsertpackHistory(packId, individualId, null, pg);

                var packHistoryId = pg.GetPackHistoryId(female.Pack, female.Female_ID);
                
                // get paired femail id
                pg.InsertIndividual(new Individual { Name = female.Paired_female_ID });
                var pairedFemaleId = pg.GetIndividualId(female.Paired_female_ID);

                // get litter
                // Create litter
                pg.InsertSingleLitter(female.Litter, packId);

                var litterId = pg.GetLitterId(female.Litter);

                pg.AddConditioningFemale(packHistoryId, pairedFemaleId, litterId, female);


            }
        }

        public void ProcessBloodData(List<Jennis_blood_data> bloodData)
        {
            // freeze times are strings. need to turn em into times.
            throw new NotImplementedException();
        }

        public void ProcessHpaSamples(List<HPA_samples> hpaSamples)
        {
            // Second_blood_sample_stopwatch_time is string
            throw new NotImplementedException();
        }

        public void ProcessDnaSamples(List<DNA_SAMPLES> dnaSamples)
        {
            throw new NotImplementedException();
        }

        public void ProcessAntiParasite(List<Antiparasite_experiment> antiParasite)
        {
            throw new NotImplementedException();
        }
    }
}