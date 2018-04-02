using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using DataPipe.Main.Model;
using DataPipe.Main.Model.LifeHistory;
using DataPipe.Main.Model.Oestrus;
using DataPipe.Main.Model.PupFocal;

using Microsoft.Data.Sqlite;
using NLog;
using NLog.Config;



namespace DataPipe.Main
{
    public class Data
    {
        private static Logger logger;

        public static IDbConnection SimpleDbConnection()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetLogger("Data");
            var dbFile = GetAppSettings.Get().SqliteLocation;

            var file = new FileInfo(dbFile);
            Console.WriteLine(file.Directory);

            if (File.Exists(dbFile))
            {
                logger.Info($"Database found at '{dbFile}' ");
            }
            else
            {
                logger.Error($"Database not found at '{dbFile}'");

                throw new FileNotFoundException($"database not located at '{dbFile}'");
            }

            return new SqliteConnection($"Data Source={dbFile};");
        }

        public static void MarkAsSent<T>(T message) where T : class, ISendable
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            logger = LogManager.GetLogger("Data");

            if (message.entity_type == "mongoose" || message.entity_type == "pack" || message.entity_type == "litter")
            {
                using (var cnn = SimpleDbConnection())
                {
                    logger.Info(
                        $"Marking {message} as sent. Type = '{message.entity_type}' entity_id = '{message.entity_id}'");

                    cnn.Open();

                    var t = cnn.BeginTransaction();

                    cnn.Execute("update sync_entity set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update sync_value_int set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update sync_value_real set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update sync_value_varchar set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });

                    t.Commit();
                }
            }
            else
            {
                using (var cnn = SimpleDbConnection())
                {
                    logger.Info(
                        $"Marking {message} as sent. Type = '{message.entity_type}' entity_id = '{message.entity_id}'");

                    cnn.Open();
                    var t = cnn.BeginTransaction();

                    cnn.Execute("update stream_entity set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update stream_value_int set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update stream_value_real set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });
                    cnn.Execute("update stream_value_varchar set sent = 1 where entity_id = @entity_id",
                        new { entity_id = message.entity_id });

                    t.Commit();
                }
            }
        }

        private static IEnumerable<T> RunSql<T>(string sql) where T : new()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
            var logger = LogManager.GetLogger("Data");

            using (var cnn = SimpleDbConnection())
            {
                List<T> result;
                try
                {
                    result = cnn.Query<T>(sql).ToList();
                }
                catch (Exception exception)
                {
                    logger.Error(exception, $"Could not get data from the database conn = '{cnn.ConnectionString}' sql='{sql}'");
                    result = new List<T>();
                }

                return result;
            }
        }

        public static IEnumerable<IndividualCreated> GetUnsyncedIndividuals()
        {
            var newIndividualsSql = @"
                                    select se.entity_id, 
                                    se.unique_id as 'UniqueId',
                                    se.sent,
                                    se.entity_type,
                                    svvName.value as 'Name',
                                    svvChip.value as 'ChipCode'	,
                                    svvDob.value as 'DateOfBirthString',
                                    svvLitter.value as 'litter_code',
                                    svvGender.value as 'Gender',
                                    svvPack.value as 'PackUniqueId',
                                    (select svv.value as PackCode
                                    from sync_entity se
                                    join sync_value_varchar svv on se.entity_id = svv.entity_id and svv.attribute_id = 'name'
                                    where se.unique_id = svvPack.value ) as 'PackCode',
                                    svrColler.value as CollerWeight,
                                    svrLat.value as Latitude,
                                    svrLon.value as Longitude
                                    from sync_entity se 
                                    left join sync_value_varchar svvName on svvName.entity_id = se.entity_id and svvName.attribute_id = 'name'
                                    left join sync_value_varchar svvChip on svvchip.entity_id = se.entity_id and svvchip.attribute_id = 'chip-code'
                                    left join sync_value_varchar svvDob on svvDob.entity_id = se.entity_id and svvDob.attribute_id = 'dob'
                                    left join sync_value_varchar svvGender on svvGender.entity_id = se.entity_id and svvGender.attribute_id = 'gender'
                                    left join sync_value_varchar svvLitter on svvLitter.entity_id = se.entity_id and svvLitter.attribute_id = 'litter-code'
                                    left join sync_value_varchar svvPack on svvPack.entity_id = se.entity_id and svvPack.attribute_id = 'pack-id'
                                    left join sync_value_real svrColler on svrColler.entity_id = se.entity_id and svrColler.attribute_id = 'collar_weight'
                                    left join sync_value_real svrLat on svrLat.entity_id = se.entity_id and svrLat.attribute_id = 'lat'
                                    left join sync_value_real svrLon on svrLon.entity_id = se.entity_id and svrLon.attribute_id = 'lon'
                                    
                                    where se.entity_type = 'mongoose' and se.sent = 0";
            var newIndividuals = RunSql<IndividualCreated>(newIndividualsSql).ToList();

            foreach (var individualCreated in newIndividuals)
            {
                if (DateTime.TryParse(individualCreated.DateOfBirthString, out var dateOfBirth))
                {
                    individualCreated.DateOfBirth = dateOfBirth;
                }
            }


            return newIndividuals;
        }

        private static DateTime? GetDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out var dateTime))
            {
                return dateTime;
            }

            return null;
        }

        public static IEnumerable<PackCreated> GetNewPacks()
        {
            var newPacks = new List<PackCreated>();
            var stringsSql = @"
                        select se.entity_id, se.unique_id as unique_id,se.entity_type, svv.attribute_id as attribute_id ,svv.value
                        from sync_entity se
                        join sync_value_varchar svv on se.entity_id = svv.entity_id
                        where se.entity_type = ""pack"" and se.sent = 0
                        group by se.unique_id , svv.attribute_id ,svv.value;";
            var strings = RunSql<DatabaseRow<string>>(stringsSql).ToList();

            foreach (var uniqueId in strings.Select(s => s.unique_id).Distinct())
            {
                var pack = new PackCreated
                {
                    Name = strings.SingleOrDefault(s => s.unique_id == uniqueId && s.attribute_id == "name")?.value,
                    UniqueId = uniqueId,
                    entity_id = strings.SingleOrDefault(s => s.unique_id == uniqueId && s.attribute_id == "name")
                        .entity_id,
                    entity_type = strings.SingleOrDefault(s => s.unique_id == uniqueId && s.attribute_id == "name")
                        ?.entity_type
                };
                if (DateTime.TryParse(
                    strings.SingleOrDefault(s => s.unique_id == uniqueId && s.attribute_id == "time")?.value,
                    out var dateCreated))
                {
                    pack.CreatedDate = dateCreated;
                }

                if (!string.IsNullOrEmpty(pack.Name) && pack.CreatedDate != null)
                {
                    newPacks.Add(pack);
                }
            }

            return newPacks;
        }

        public static IEnumerable<WeightMeasure> GetUnsyncedWeights()
        {
            var newWeights = new List<WeightMeasure>();
            var stringSql = @"
                         select se.entity_type, se.entity_id, se.unique_id, svv.attribute_id, svv.value 
                         from stream_entity se
                         join stream_value_varchar svv on se.entity_id = svv.entity_id
                         where se.entity_type = ""group-comp-weight"" and se.sent = 0
                         ";

            var strings = RunSql<DatabaseRow<string>>(stringSql).ToList();

            var intSql = @" select se.entity_type, se.entity_id, se.unique_id, svi.attribute_id, svi.value 
                             from stream_entity se
                             join stream_value_int svi on se.entity_id = svi.entity_id
                            where se.entity_type = ""group-comp-weight"" and se.sent = 0";
            var ints = RunSql<DatabaseRow<int?>>(intSql).ToList();

            var realSql = @" select se.entity_type, se.entity_id, se.unique_id, svi.attribute_id, svi.value 
                             from stream_entity se
                             join stream_value_real svi on se.entity_id = svi.entity_id
                            where se.entity_type = ""group-comp-weight"" and se.sent = 0";
            var reals = RunSql<DatabaseRow<double>>(realSql).ToList();

            // get relevent ids for weights

            foreach (var unique_id in strings.Select(s => s.unique_id).Distinct())
            {
                using (var conn = SimpleDbConnection())
                {
                    var individualInfo = conn.Query<dynamic>(@"
                                            select svv.value as indivName, se.unique_id as indivUnique from sync_entity se
                                            join sync_value_varchar svv on se.entity_id = svv.entity_id
                                                where se.unique_id in (select svv.value  from stream_entity se 
                                                join stream_value_varchar svv on svv.entity_id = se.entity_id
                                            where se.unique_id = @unique_id and svv.attribute_id = ""id-mongoose"")
                                            and svv.attribute_id = ""name""",
                        new { unique_id }).FirstOrDefault();
                    var parentId = strings.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "parent")
                        ?.value;
                    var packInfo = conn.Query<dynamic>(
                        @"select svv.value as packName, se.unique_id as packUniqueId from sync_entity se
                                        join sync_value_varchar svv on se.entity_id = svv.entity_id
                                        where se.unique_id in (select svv.value as packUniqueId from stream_entity se
                                        join stream_value_varchar svv on se.entity_id = svv.entity_id
                                        where se.unique_id = @unique_id and svv.attribute_id = ""pack"")
                                        and svv.attribute_id = ""name""",
                        new { unique_id = parentId }).FirstOrDefault();
                    var newWeight = new WeightMeasure
                    {
                        UniqueId = unique_id,
                        IndividualName = individualInfo?.indivName,
                        IndividualUniqueId = individualInfo?.indivUnique,
                        PackId = packInfo?.packName,
                        PackUniqueId = packInfo?.packUniqueId,
                        Accurate = ints.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "accurate")
                            ?.value,
                        Weight = reals.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "weight")
                            .value,
                        CollarWeight = reals
                            .SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "cur-collar-weight")
                            ?.value,
                        Latitude = reals.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "lat")
                            ?.value,
                        Longitude = reals.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "lon")
                            ?.value,
                        Time = GetDate(strings
                            .SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "time")?.value),
                        entity_id = reals.SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "weight")
                            .entity_id,
                        entity_type = reals
                            .SingleOrDefault(s => s.unique_id == unique_id && s.attribute_id == "weight")
                            .entity_type
                    };

                    newWeights.Add(newWeight);
                }
            }

            return newWeights;
        }

        public static IEnumerable<LifeHistoryEvent> GetLifeHistoryEvents()
        {
            const string stringSql = @"select 
	                             se.entity_id,
	                             se.entity_type,
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'date') as 'Date',
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'type') as 'Type',
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'code') as 'Code',
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'entity-uid') as 'UniqueId',
	                             (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'entity-name') as 'entity_name',
	                             se.sent
                            from stream_entity se
                            where se.entity_type = 'lifehist-event' and se.sent = 0;";
            var lifeEvents = RunSql<LifeHistoryEvent>(stringSql).ToList();



            return lifeEvents;
        }

        public static IEnumerable<LitterCreated> GetUnsynchedLitters()
        {
            const string stringSql = @"select 
                             (select value from sync_value_varchar where entity_id = se.entity_id and attribute_id = 'date') as 'Date',
                             (select value from sync_value_varchar where entity_id = se.entity_id and attribute_id = 'name') as 'LitterName',
                             (select value from sync_value_varchar where entity_id = se.entity_id and attribute_id = 'unique_id') as 'UniqueId',
                             (select svv.value from sync_entity se
                                join sync_value_varchar svv on svv.entity_id = se.entity_id
                                where se.unique_id = svvPackId.value and svv.attribute_id = 'name') as 'PackName',
                             svvPackId.value as 'PackUniqueId',
                             'litter' as entity_type,
                             se.entity_id
                             from sync_entity se 
                             join sync_value_varchar svvPackId on svvPackId.entity_id = se.entity_id and svvPackId.attribute_id = 'parent'
                             where se.entity_type = 'litter' and se.sent = 0;";

            var lifeEvents = RunSql<LitterCreated>(stringSql).ToList();
            return lifeEvents;
        }

        public static IEnumerable<PackMove> GetUnsynchedPackMoves()
        {
            const string stringSql = @"select entity_id, entity_type, unique_id,
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'mongoose-id') as 'MongooseId',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'mongoose-name') as 'MongooseName',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'pack-dst-id') as 'PackDestinationId',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'pack-dst-name') as 'PackDestintionName',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'pack-src-id') as 'PackSourceId',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'pack-src-name') as 'PackSourceName',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'time') as 'Time',
                            (select value from stream_value_varchar where entity_id = se.entity_id and attribute_id = 'user') as 'User',
                            (select value from stream_value_real where entity_id = se.entity_id and attribute_id = 'lat') as 'Latitude',
                            (select value from stream_value_real where entity_id = se.entity_id and attribute_id = 'lon') as 'Longitude'
                            from stream_entity se
                            where se.entity_type = 'movepack-event' and se.sent = 0";

            var lifeEvents = RunSql<PackMove>(stringSql).ToList();
            return lifeEvents;
        }

        public static IEnumerable<InterGroupInteractionEvent> GetUnsynchedInterGroupInteractions()
        {
            const string stringSql = @"
                                 select se.entity_id 
                                ,se.sent
                                ,se.entity_type
                                ,se.unique_id as 'UniqueId'
                                ,outcome.value as 'outcome'
                                ,(select svv.value from sync_entity se
                                join sync_value_varchar svv on svv.entity_id = se.entity_id
                                where se.unique_id = leader.value and svv.attribute_id = 'name' ) as 'leaderName'
                                ,leader.value as 'leaderUniqueId'

                                ,(select svv.value from sync_entity se
                                join sync_value_varchar svv on svv.entity_id = se.entity_id
                                where se.unique_id = pack.value and svv.attribute_id = 'name' ) as 'packName'
                                ,pack.value as 'packUniqueId'
								
								 ,(select svv.value from sync_entity se
                                join sync_value_varchar svv on svv.entity_id = se.entity_id
                                where se.unique_id = otherPack.value and svv.attribute_id = 'name' ) as 'otherPackName'
                                ,otherPack.value as 'otherPackUniqueId'
								
                                ,time.value as 'time'
                                ,lat.value as 'latitude'
                                ,lon.value as 'longitude'
                                ,duration.value as 'duration'
                                from stream_entity se
                                join stream_value_varchar outcome on outcome.entity_id = se.entity_id and outcome.attribute_id = 'outcome'
                                join stream_value_varchar leader on leader.entity_id = se.entity_id and leader.attribute_id = 'id-leader'							 	
                                join stream_value_varchar pack on pack.entity_id = se.entity_id and pack.attribute_id = 'id-pack'
							    join stream_value_varchar otherPack on otherPack.entity_id = se.entity_id and otherPack.attribute_id = 'id-other-pack'
                                join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
                                join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
                                join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
                                join stream_value_int duration on duration.entity_id = se.entity_id and duration.attribute_id = 'duration'
                                where se.sent = 0 and se.entity_type = 'group-interaction';";

            return RunSql<InterGroupInteractionEvent>(stringSql).ToList();
        }

        public static IEnumerable<GroupAlarmEvent> GetUnsynchedGroupAlarms()
        {
            const string stringSql = @"
                        select
                        se.sent
                        ,se.entity_id
                        ,se.entity_type
                        ,se.unique_id as 'UniqueId'
                        ,cause.value  as cause
                        ,othersJoin.value as 'othersJoin'
                        ,time.value as 'time'
                        ,user.value as 'user'
                        ,(select svv.value from sync_entity se
                        join sync_value_varchar svv on svv.entity_id = se.entity_id
                        where se.unique_id = pack.value and svv.attribute_id = 'name' ) as 'packName'
                        ,pack.value as 'packUniqueId'
                        ,(select svv.value from sync_entity se
                        join sync_value_varchar svv on svv.entity_id = se.entity_id
                        where se.unique_id = caller.value and svv.attribute_id = 'name' ) as 'callerName'
                        ,caller.value as 'callerUniqueId'
                        ,lat.value as 'latitude'
                        ,lon.value as 'longitude'
                        from stream_entity se
                        join stream_value_varchar cause on cause.entity_id = se.entity_id and cause.attribute_id = 'cause'
                        join stream_value_varchar caller on caller.entity_id = se.entity_id and caller.attribute_id = 'id-caller'
                        join stream_value_varchar pack on pack.entity_id = se.entity_id and pack.attribute_id = 'id-pack'
                        join stream_value_varchar othersJoin on othersJoin.entity_id = se.entity_id and othersJoin.attribute_id = 'others-join'
                        join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
                        join stream_value_varchar user on user.entity_id = se.entity_id and user.attribute_id = 'user'
						join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
                        join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon' 
						where se.sent = 0 and se.entity_type = 'group-alarm';";

            return RunSql<GroupAlarmEvent>(stringSql).ToList();
        }

        public static IEnumerable<GroupMoveEvent> GetUnsynchedGroupMoves()
        {
            const string stringSql = @"
                         select  se.sent
                        ,se.entity_id
                        ,se.entity_type
                        ,se.unique_id as 'UniqueId'
						,(select svv.value from sync_entity se
                        join sync_value_varchar svv on svv.entity_id = se.entity_id
                        where se.unique_id = idpack.value and svv.attribute_id = 'name' ) as 'pack'
                        , idpack.value as 'packUniqueId'
                        ,(select svv.value from sync_entity se
                        join sync_value_varchar svv on svv.entity_id = se.entity_id
                        where se.unique_id = idleader.value and svv.attribute_id = 'name' ) as 'leader'
                        ,idleader.value as 'leaderUniqueId'
                        ,destination.value as 'Destination'
                        ,direction.value as 'Direction'
                        ,time.value as 'Time'
                        ,user.value as 'User'
                        ,CAST(packcount.value as NUMERIC) as 'HowMany'
                        ,CAST(packwidth.value as NUMERIC) as'Width'
                        ,CAST(packdepth.value as NUMERIC) as'Depth'
                        ,lat.value as 'latitude'
                        ,lon.value as 'longitude'                     
                        from stream_entity se 
                        left join stream_value_varchar destination on destination.entity_id = se.entity_id and destination.attribute_id = 'destination'
                        left join stream_value_varchar direction on direction.entity_id = se.entity_id and direction.attribute_id = 'direction'
                        left join stream_value_varchar idleader on idleader.entity_id = se.entity_id and idleader.attribute_id = 'id-leader'
                        left join stream_value_varchar idpack on idpack.entity_id = se.entity_id and idpack.attribute_id = 'id-pack'
                        left join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
                        left join stream_value_varchar user on user.entity_id = se.entity_id and user.attribute_id = 'user'
                        left join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
                        left join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon' 
                        left join stream_value_int packcount on packcount.entity_id = se.entity_id and packcount.attribute_id = 'pack-count' 
                        left join stream_value_int packdepth on packdepth.entity_id = se.entity_id and packdepth.attribute_id = 'pack-depth' 
                        left join stream_value_int packwidth on packwidth.entity_id = se.entity_id and packwidth.attribute_id = 'pack-width' 
                        where se.sent = 0 and se.entity_type = 'group-move' order by packcount.value is null;";

            var a = RunSql<GroupMoveEvent>(stringSql).ToList();
            return a;
        }

        public static IEnumerable<PupFocal>  GetUnsynchedPupFocals()
        {
            const string stringSql = @"
 select
 se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = subject.value and svv.attribute_id = 'name' ) as 'focalIndividualName'
,subject.value as 'focalIndividualId'
,time.value as 'time'
,user.value as 'user'
,lon.value as 'longitude'
,lat.value as 'latitude'
,packCount.value as 'visibleIndividuals'
,packDepth.value as 'depth'
,packWidth.value as 'width'
,(select svv3.value as 'PackName'  from sync_entity se3
join sync_value_varchar svv3 on svv3.entity_id = se3.entity_id
where se3.unique_id = (
select svv2.value from sync_entity se2
join sync_value_varchar svv2 on svv2.entity_id = se2.entity_id
where se2.unique_id = subject.value and svv2.attribute_id = 'pack-id') and svv3.attribute_id = 'name') as packName
,(select svv2.value from sync_entity se2
join sync_value_varchar svv2 on svv2.entity_id = se2.entity_id
where se2.unique_id = subject.value and svv2.attribute_id = 'pack-id')as packUniqueId
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'pup-focal-nearest') as nearest
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'pup-focal-pupfeed') as pupFeed
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'pup-focal-pupfind') as pupFind
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'pup-focal-pupcare') as pupCare
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'pup-focal-pupaggr') as pupAggression  
  from stream_entity se 
left join stream_value_varchar subject on subject.entity_id = se.entity_id and subject.attribute_id = 'id-focal-subject' 
left join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
left join stream_value_varchar user on user.entity_id = se.entity_id and user.attribute_id = 'user' 
left join stream_value_real lon on lon.entity_id = se.entity_id  and lon.attribute_id = 'lon' 
left join stream_value_real lat on lat.entity_id = se.entity_id  and lat.attribute_id = 'lat' 
left join stream_value_int packCount on packCount.entity_id = se.entity_id and packCount.attribute_id = 'pack-count'
left join stream_value_int packDepth on packDepth.entity_id = se.entity_id and packDepth.attribute_id = 'pack-depth'
left join stream_value_int packWidth on packWidth.entity_id = se.entity_id and packWidth.attribute_id = 'pack-width'
 where se.entity_type = 'pup-focal' and se.sent = 0 
 order by packCount.value is null and time.value;";

            var pupFocals = RunSql<PupFocal>(stringSql).ToList();

            foreach (var pupFocal in pupFocals)
            {
                pupFocal.PupNearestList = GetPupNearest(pupFocal.nearest);
                pupFocal.PupAggressionList = GetPupAggressions(pupFocal.pupAggression);
                pupFocal.PupCareList = GetPupCare(pupFocal.pupCare);
                pupFocal.PupFeedList = GetPupFeeds(pupFocal.pupFeed);
                pupFocal.PupFindList = GetPupFinds(pupFocal.pupFind);
            }

            return pupFocals;
        }

        private static List<PupFind> GetPupFinds(string pupFocalPupFind)
        {

            var stringSql = $@"
            select se.entity_id 
        ,se.sent
        ,se.entity_type
        ,se.unique_id as 'UniqueId'
        ,size.value as 'size'
        ,time.value as 'time'
        ,lat.value as 'latitude'
        ,lon.value as 'longitude'
        from stream_entity se
        join stream_value_varchar size on size.entity_id = se.entity_id and size.attribute_id = 'size'
        join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
        join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
        join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
        where se.entity_id in ({pupFocalPupFind}) and entity_type = 'pup-focal-pupfind';;";

            return RunSql<PupFind>(stringSql).ToList();
        }

        private static List<PupFeed> GetPupFeeds(string pupFocalPupFeed)
        {
            var stringSql = $@"

 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = whoIndividual.value and svv.attribute_id = 'name' ) as 'whoIndividualName'
,whoIndividual.value as 'whoIndividualId'
,size.value as 'size'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar whoIndividual on whoIndividual.entity_id = se.entity_id and whoIndividual.attribute_id = 'id-who'
join stream_value_varchar size on size.entity_id = se.entity_id and size.attribute_id = 'size'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({pupFocalPupFeed});";

            return RunSql<PupFeed>(stringSql).ToList();
        }

        private static List<PupCare> GetPupCare(string pupFocalPupCare)
        {
            var stringSql = $@"
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = whoIndividual.value and svv.attribute_id = 'name' ) as 'whoIndividualName'
,whoIndividual.value as 'whoIndividualId'
,type.value as 'type'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar whoIndividual on whoIndividual.entity_id = se.entity_id and whoIndividual.attribute_id = 'id-who'
join stream_value_varchar type on type.entity_id = se.entity_id and type.attribute_id = 'type'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({pupFocalPupCare});";

            return RunSql<PupCare>(stringSql).ToList();
        }

        private static List<PupAggressionEvent> GetPupAggressions(string pupFocalPupAggression)
        {
            var stringSql = $@"
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = withIndividual.value and svv.attribute_id = 'name' ) as 'withIndividualName'
,withIndividual.value as 'withIndividualId'
,initiate.value as 'initiate'
,level.value as 'level'
,over.value as 'over'
,win.value as 'win'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar withIndividual on withIndividual.entity_id = se.entity_id and withIndividual.attribute_id = 'id-with'
join stream_value_varchar initiate on initiate.entity_id = se.entity_id and initiate.attribute_id = 'initiate'
join stream_value_varchar level on level.entity_id = se.entity_id and level.attribute_id = 'level'
join stream_value_varchar over on over.entity_id = se.entity_id and over.attribute_id = 'over'
join stream_value_varchar win on win.entity_id = se.entity_id and win.attribute_id = 'win'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({pupFocalPupAggression});";

            return RunSql<PupAggressionEvent>(stringSql).ToList();
        }

        private static List<PupNearest> GetPupNearest(string pupFocalNearest)
        {
            var stringSql = $@"                      
             select se.entity_id 
            ,se.sent
            ,se.entity_type
            ,se.unique_id as 'UniqueId'
            ,listClose.value as listClose
            ,(select svv.value from sync_entity se
            join sync_value_varchar svv on svv.entity_id = se.entity_id
            where se.unique_id = idNearest.value and svv.attribute_id = 'name' ) as 'nearestIndividualName'
            ,idNearest.value as 'nearestIndividualId'
            ,time.value as 'scanTime'
            from stream_entity se
            join stream_value_varchar listClose on listClose.entity_id = se.entity_id and listClose.attribute_id = 'id-list-close'
            join stream_value_varchar idNearest on idNearest.entity_id = se.entity_id and idNearest.attribute_id = 'id-nearest'
            join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'scan-time'
            where se.entity_id in ({pupFocalNearest});";

            var list = RunSql<PupNearest>(stringSql).ToList();
            foreach (var pupNearest in list)
            {
                pupNearest.CloseListNames = GetNamesFromIds(pupNearest.listClose);
            }

            return list;
        }

        public static IEnumerable<OestrusEvent> GetUnsynchedOesturusFocals()
        {
            const string stringSql = @"
select 
se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = subject.value and svv.attribute_id = 'name' ) as 'focalIndividualName'
,subject.value as 'focalIndividualId'
,time.value as 'time'
,user.value as 'user'
,lon.value as 'longitude'
,lat.value as 'latitude'
,packCount.value as 'visibleIndividuals'
,packDepth.value as 'depth'
,packWidth.value as 'width'
,(select svv3.value as 'PackName'  from sync_entity se3
join sync_value_varchar svv3 on svv3.entity_id = se3.entity_id
where se3.unique_id = (
select svv2.value from sync_entity se2
join sync_value_varchar svv2 on svv2.entity_id = se2.entity_id
where se2.unique_id = subject.value and svv2.attribute_id = 'pack-id') and svv3.attribute_id = 'name') as packName
,(select svv2.value from sync_entity se2
join sync_value_varchar svv2 on svv2.entity_id = se2.entity_id
where se2.unique_id = subject.value and svv2.attribute_id = 'pack-id')as packUniqueId
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'oestrus-focal-nearest') as nearest
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'oestrus-focal-maleaggr') as maleaggr
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'oestrus-focal-mate') as mate
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'oestrus-focal-aggr') as aggr
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'oestrus-focal-affil') as affil

  
  from stream_entity se 
left join stream_value_varchar subject on subject.entity_id = se.entity_id and subject.attribute_id = 'id-focal-subject' 
left join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
left join stream_value_varchar user on user.entity_id = se.entity_id and user.attribute_id = 'user' 
left join stream_value_real lon on lon.entity_id = se.entity_id  and lon.attribute_id = 'lon' 
left join stream_value_real lat on lat.entity_id = se.entity_id  and lat.attribute_id = 'lat' 
left join stream_value_int packCount on packCount.entity_id = se.entity_id and packCount.attribute_id = 'pack-count'
left join stream_value_int packDepth on packDepth.entity_id = se.entity_id and packDepth.attribute_id = 'pack-depth'
left join stream_value_int packWidth on packWidth.entity_id = se.entity_id and packWidth.attribute_id = 'pack-width'

 where se.entity_type = 'oestrus-focal' and se.sent = 0 
 order by packCount.value is null and time.value;";

            var oestrusFocal = RunSql<OestrusEvent>(stringSql).ToList();

            foreach (var oestrusEvent in oestrusFocal)
            {
                oestrusEvent.NearestList = GetOestrusNearest(oestrusEvent.nearest);
                oestrusEvent.MaleAggressionList = GetOestrusMaleAggressions(oestrusEvent.maleaggr);
                oestrusEvent.MateEventList = GetOestrusMateEvents(oestrusEvent.mate);
                oestrusEvent.AggressionEventList = GetOestrusAggressions(oestrusEvent.aggr);
                oestrusEvent.AffiliationEventList = GetOestrusAffiliations(oestrusEvent.affil);
            }

            return oestrusFocal;
        }

        private static List<OestrusAffiliationEvent> GetOestrusAffiliations(string oestrusEventAffil)
        {
            var stringSql = $@"
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = withIndividual.value and svv.attribute_id = 'name' ) as 'withIndividualName'
,withIndividual.value as 'withIndividualId'
,initiate.value as 'initiate'
,over.value as 'over'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar withIndividual on withIndividual.entity_id = se.entity_id and withIndividual.attribute_id = 'id-with'
join stream_value_varchar initiate on initiate.entity_id = se.entity_id and initiate.attribute_id = 'initiate'
join stream_value_varchar over on over.entity_id = se.entity_id and over.attribute_id = 'over'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({oestrusEventAffil});";

            return RunSql<OestrusAffiliationEvent>(stringSql).ToList();
        }

        private static List<OestrusAggressionEvent> GetOestrusAggressions(string oestrusEventAggr)
        {
            var stringSql = $@"
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = withIndividual.value and svv.attribute_id = 'name' ) as 'withIndividualName'
,withIndividual.value as 'withIndividualId'
,initiate.value as 'initiate'
,level.value as 'level'
,over.value as 'over'
,win.value as 'win'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar withIndividual on withIndividual.entity_id = se.entity_id and withIndividual.attribute_id = 'id-with'
join stream_value_varchar initiate on initiate.entity_id = se.entity_id and initiate.attribute_id = 'initiate'
join stream_value_varchar level on level.entity_id = se.entity_id and level.attribute_id = 'level'
join stream_value_varchar over on over.entity_id = se.entity_id and over.attribute_id = 'over'
join stream_value_varchar win on win.entity_id = se.entity_id and win.attribute_id = 'win'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({oestrusEventAggr});";

            return RunSql<OestrusAggressionEvent>(stringSql).ToList();
        }

        private static List<OestrusMateEvent> GetOestrusMateEvents(string oestrusEventMate)
        {

            var stringSql = $@"                      
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,behaviour.value as 'behaviour'
,fResponse.value as 'femaleResponse'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = withIndividual.value and svv.attribute_id = 'name' ) as 'withIndividualName'
,withIndividual.value as 'withIndividualId'
,mResponse.value as 'maleResponse'
,success.value as 'success'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar behaviour on behaviour.entity_id = se.entity_id and behaviour.attribute_id = 'behaviour'
join stream_value_varchar fResponse on fResponse.entity_id = se.entity_id and fResponse.attribute_id = 'female-response'
join stream_value_varchar withIndividual on withIndividual.entity_id = se.entity_id and withIndividual.attribute_id = 'id-with'
join stream_value_varchar mResponse on mResponse.entity_id = se.entity_id and mResponse.attribute_id = 'male-response'
join stream_value_varchar success on success.entity_id = se.entity_id and success.attribute_id = 'success'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({oestrusEventMate});";

            return RunSql<OestrusMateEvent>(stringSql).ToList();
        }

        private static List<OestrusMaleAggression> GetOestrusMaleAggressions(string oestrusEventMaleaggr)
        {
            var stringSql = $@"                      
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = initiator.value and svv.attribute_id = 'name' ) as 'initiatorIndividualName'
,initiator.value as 'initiatorIndividualId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = receiver.value and svv.attribute_id = 'name' ) as 'receiverIndividualName'
,receiver.value as 'receiverIndividualId'
,level.value as 'level'
,winner.value as 'winner'
,owner.value as 'owner'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar initiator on initiator.entity_id = se.entity_id and initiator.attribute_id = 'id-initiator'
join stream_value_varchar receiver on receiver.entity_id = se.entity_id and receiver.attribute_id = 'id-receiver'
join stream_value_varchar level on level.entity_id = se.entity_id and level.attribute_id = 'level'
join stream_value_varchar winner on winner.entity_id = se.entity_id and winner.attribute_id = 'winner'
join stream_value_varchar owner on owner.entity_id = se.entity_id and owner.attribute_id = 'owner'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({oestrusEventMaleaggr});";

            return RunSql<OestrusMaleAggression>(stringSql).ToList();
        }

        private static List<OestrusNearest> GetOestrusNearest(string oestrusEventNearest)
        {
            var stringSql = $@"                      
             select se.entity_id 
            ,se.sent
            ,se.entity_type
            ,se.unique_id as 'UniqueId'
            ,listClose.value as listClose
            ,(select svv.value from sync_entity se
            join sync_value_varchar svv on svv.entity_id = se.entity_id
            where se.unique_id = idNearest.value and svv.attribute_id = 'name' ) as 'nearestIndividualName'
            ,idNearest.value as 'nearestIndividualId'
            ,time.value as 'scanTime'
            from stream_entity se
            join stream_value_varchar listClose on listClose.entity_id = se.entity_id and listClose.attribute_id = 'id-list-close'
            join stream_value_varchar idNearest on idNearest.entity_id = se.entity_id and idNearest.attribute_id = 'id-nearest'
            join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'scan-time'
            where se.entity_id in ({oestrusEventNearest});";

            var list = RunSql<OestrusNearest>(stringSql).ToList();
            foreach (var oestrusNearest in list)
            {
                oestrusNearest.CloseListNames = GetNamesFromIds(oestrusNearest.listClose);
            }

            return list;
        }

        private static List<string> GetNamesFromIds(string listClose)
        {
            //converts the comma delimenet list of individual is to a list of individual names
            var names = new List<string>();
            if (!string.IsNullOrEmpty(listClose))
            {
                var ids = listClose.Split(',');
                foreach (var id in ids)
                {
                    names.Add(GetNameFromId(id));
                }
            }

            return names;
        }

        private static string GetNameFromId(string id)
        {
            if (id == "Unknown")
            {
                return "Unknown";
            }

            if (id == "None")
            {
                return null;
            }

            using (var cnn = SimpleDbConnection())
            {
                return cnn.ExecuteScalar<string>(@"select svv.value as 'name'
                                from sync_entity se
                                join sync_value_varchar svv on svv.entity_id = se.entity_id
                                where se.unique_id = @id and svv.attribute_id = 'name'  ;",
                    new { id = id });
            }
        }

    }



    public class DatabaseRow<T>
    {
        public string entity_type { get; set; }
        public int entity_id { get; set; }
        public string unique_id { get; set; }
        public string attribute_id { get; set; }
        public T value { get; set; }
    }
}