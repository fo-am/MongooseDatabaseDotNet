using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Dapper;
using DataPipe.Main.Model;
using DataPipe.Main.Model.LifeHistory;
using DataPipe.Main.Model.Oestrus;
using DataPipe.Main.Model.PregnancyFocal;
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

                throw new FileNotFoundException($"database not located at '{dbFile}'{ Path.GetFullPath(dbFile)}");
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
                                    NULLIF(svvName.value,'NULL') as 'Name',
                                    NULLIF(svvChip.value,'NULL') as 'ChipCode'	,
                                    NULLIF(svvDob.value,'NULL') as 'DateOfBirthString',
                                    NULLIF(svvLitter.value,'NULL') as 'litter_code',
                                    NULLIF(svvGender.value,'NULL') as 'Gender',
                                    NULLIF(svvPack.value,'NULL') as 'PackUniqueId',
                                    (select svv.value as PackCode
                                    from sync_entity se
                                    join sync_value_varchar svv on se.entity_id = svv.entity_id and svv.attribute_id = 'name'
                                    where se.unique_id = svvPack.value ) as 'PackCode',
                                    svrColler.value as CollerWeight,
                                    NULLIF(svrLat.value,'NULL') as Latitude,
                                    NULLIF(svrLon.value,'NULL') as Longitude
                                    from sync_entity se 
                                    join sync_value_varchar svvName on svvName.entity_id = se.entity_id and svvName.attribute_id = 'name'
                                    left join sync_value_varchar svvChip on svvchip.entity_id = se.entity_id and svvchip.attribute_id = 'chip-code'
                                    left join sync_value_varchar svvDob on svvDob.entity_id = se.entity_id and svvDob.attribute_id = 'dob'
                                    left join sync_value_varchar svvGender on svvGender.entity_id = se.entity_id and svvGender.attribute_id = 'gender'
                                    left join sync_value_varchar svvLitter on svvLitter.entity_id = se.entity_id and svvLitter.attribute_id = 'litter-code'
                                    left join sync_value_varchar svvPack on svvPack.entity_id = se.entity_id and svvPack.attribute_id = 'pack-id'
                                    left join sync_value_real svrColler on svrColler.entity_id = se.entity_id and svrColler.attribute_id = 'collar_weight'
                                    left join sync_value_real svrLat on svrLat.entity_id = se.entity_id and svrLat.attribute_id = 'lat'
                                    left join sync_value_real svrLon on svrLon.entity_id = se.entity_id and svrLon.attribute_id = 'lon'                                    
                                    where se.entity_type = 'mongoose' and se.sent = 0 ORDER BY NULLIF(svrLat.value,'NULL') IS NULL, se.entity_id;";
            var newIndividuals = RunSql<IndividualCreated>(newIndividualsSql).ToList();

            // date of birth can be unknown or null. if it can be turned into a date then do so.
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
            var sql = @"select se.entity_id, 
                        se.unique_id as 'UniqueId',                                    
                        se.entity_type,
                        svvName.value as 'Name',   
                        NULLIF(svvTime.value,'NULL') as 'CreatedDate',
						NULLIF(svvUser.value,'NULL') as 'User',
                        NULLIF(svrLat.value,'NULL') as Latitude,
                        NULLIF(svrLon.value,'NULL') as Longitude,
						se.sent
                        from sync_entity se 
                        join sync_value_varchar svvName on svvName.entity_id = se.entity_id and svvName.attribute_id = 'name'
                        left join sync_value_varchar svvTime on svvTime.entity_id = se.entity_id and svvTime.attribute_id = 'time'
                        left join sync_value_varchar svvUser on svvUser.entity_id = se.entity_id and svvUser.attribute_id = 'user'
                        left join sync_value_real svrLat on svrLat.entity_id = se.entity_id and svrLat.attribute_id = 'lat'
                        left join sync_value_real svrLon on svrLon.entity_id = se.entity_id and svrLon.attribute_id = 'lon'
                        
                        where se.entity_type = 'pack'  and se.sent = 0
						order by NULLIF(svvTime.value,'NULL') is null, se.entity_id;";
            var packs = RunSql<PackCreated>(sql).ToList();
            return packs;
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

        public static List<PregnancyFocal> GetUnsynchedPregnancyFocals()
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
  where svv2.value = se.unique_id and se2.entity_type = 'pregnancy-focal-nearest') as pregnancyNearest
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'pregnancy-focal-affil') as pregnancyAffil
,(select group_concat(se2.entity_id) from stream_entity se2  
left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
  where svv2.value = se.unique_id and se2.entity_type = 'pregnancy-focal-aggr') as pregnancyAggression  
  from stream_entity se 
left join stream_value_varchar subject on subject.entity_id = se.entity_id and subject.attribute_id = 'id-focal-subject' 
left join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
left join stream_value_varchar user on user.entity_id = se.entity_id and user.attribute_id = 'user' 
left join stream_value_real lon on lon.entity_id = se.entity_id  and lon.attribute_id = 'lon' 
left join stream_value_real lat on lat.entity_id = se.entity_id  and lat.attribute_id = 'lat' 
left join stream_value_int packCount on packCount.entity_id = se.entity_id and packCount.attribute_id = 'pack-count'
left join stream_value_int packDepth on packDepth.entity_id = se.entity_id and packDepth.attribute_id = 'pack-depth'
left join stream_value_int packWidth on packWidth.entity_id = se.entity_id and packWidth.attribute_id = 'pack-width'
 where se.entity_type = 'preg-focal' and se.sent = 0 
 order by packCount.value is null and time.value;";

            var pregnancyFocals = RunSql<PregnancyFocal>(stringSql).ToList();

            foreach (var pregnancyFocal in pregnancyFocals)
            {
                pregnancyFocal.PregnancyNearestList = GetPregnancyNearest(pregnancyFocal.pregnancyNearest);
                pregnancyFocal.PregnancyAggressionList = GetPregnancyAggression(pregnancyFocal.pregnancyAggression);
                pregnancyFocal.PregnancyAffiliationList = GetPregnancyAffiliation(pregnancyFocal.pregnancyAffil);

            }

            return pregnancyFocals;
        }

        private static List<PregnancyAffiliation> GetPregnancyAffiliation(string pregnancyFocalPregnancyAffil)
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
            where se.entity_id in ({pregnancyFocalPregnancyAffil});";

            return RunSql<PregnancyAffiliation>(stringSql).ToList();
        }

        private static List<PregnancyAggression> GetPregnancyAggression(string pregnancyFocalPregnancyAggression)
        {
            var stringSql = $@"
                 select  se.entity_id 
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
                where se.entity_id in ({pregnancyFocalPregnancyAggression});";

            return RunSql<PregnancyAggression>(stringSql).ToList();
        }

        private static List<PregnancyNearest> GetPregnancyNearest(string pregnancyNearest)
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
            where se.entity_id in ({pregnancyNearest});";

            var list = RunSql<PregnancyNearest>(stringSql).ToList();
            foreach (var nearest in list)
            {
                nearest.CloseListNames = GetNamesFromIds(nearest.listClose);
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

        private static List<string> GetNamesFromIds(string listIndividualIds)
        {
            //converts the comma delimenet list of individual is to a list of individual names
            var names = new List<string>();
            if (!string.IsNullOrEmpty(listIndividualIds))
            {
                var ids = listIndividualIds.Split(',');
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

        public static List<GroupComposition> GetUnsynchedGroupCompositions()
        {
            const string stringSql = @"
                    select 
                    se.entity_id 
                    ,se.sent
                    ,se.entity_type
                    ,se.unique_id as 'UniqueId'
                    ,babies.value as 'pupList'
                    ,groupCompCode.value as 'groupCompCode'
                    ,observer.value as 'observer'
                    ,(select svv.value from sync_entity se
                        join sync_value_varchar svv on svv.entity_id = se.entity_id
                        where se.unique_id = pack.value and svv.attribute_id = 'name' ) as 'packName'
                    ,pack.value as 'packUniqueId'		
                    ,pregnant.value as 'pregnantList'
                    ,time.value as 'time'
                    ,user.value as 'user'
                    ,lat.value as 'latitude'
                    ,lon.value as 'longitude'
                    ,(select group_concat(se2.entity_id) from stream_entity se2  
                        left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
                        where svv2.value = se.unique_id and se2.entity_type = 'group-comp-weight') as 'weightIds'
                    ,(select group_concat(se2.entity_id) from stream_entity se2  
                        left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
                        where svv2.value = se.unique_id and se2.entity_type = 'group-comp-mate-guard') as 'mateGuardIds'
                    ,(select group_concat(se2.entity_id) from stream_entity se2  
                        left join stream_value_varchar svv2 on svv2.entity_id = se2.entity_id
                        where svv2.value = se.unique_id and se2.entity_type = 'group-comp-pup-assoc') as 'pupAssociationIds'
                    from stream_entity se
                    left join stream_value_varchar babies on babies.entity_id = se.entity_id and babies.attribute_id = 'baby-seen'
                    left join stream_value_varchar groupCompCode on groupCompCode.entity_id = se.entity_id and groupCompCode.attribute_id = 'group-comp-code'
                    left join stream_value_varchar observer on observer.entity_id = se.entity_id and observer.attribute_id = 'main-observer'
                    left join stream_value_varchar pack on pack.entity_id = se.entity_id and pack.attribute_id = 'pack'
                    left join stream_value_varchar pregnant on pregnant.entity_id = se.entity_id and pregnant.attribute_id = 'pregnant'
                    left join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
                    left join stream_value_varchar user on user.entity_id = se.entity_id and user.attribute_id = 'user'
                    left join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
                    left join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
                    where se.entity_type = 'group-comp' and se.sent = 0";

            var groupCompostitions = RunSql<GroupComposition>(stringSql).ToList();

            foreach (var groupComposition in groupCompostitions)
            {
                groupComposition.PupNames = GetNamesFromIds(groupComposition.pupList);
                groupComposition.PregnantNames = GetNamesFromIds(groupComposition.pregnantList);

                groupComposition.WeightsList = GetWeightList(groupComposition.weightIds);
                groupComposition.MateGuardsList = GetMateGuardList(groupComposition.mateGuardIds);
                groupComposition.PupAssociationsList = GetPupAssociationList(groupComposition.pupAssociationIds);
            }

            return groupCompostitions;
        }

        private static List<PupAssociation> GetPupAssociationList(string groupCompositionPupAssociationIds)
        {
            var stringSql = $@"
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = individual.value and svv.attribute_id = 'name' ) as 'pupName'
,individual.value as 'pupIndividualId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = other.value and svv.attribute_id = 'name' ) as 'escortName'
,other.value as 'escortIndividualId'
,strength.value as 'strength'
,accurate.value as 'accurate'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar individual on individual.entity_id = se.entity_id and individual.attribute_id = 'id-mongoose'
join stream_value_varchar other on other.entity_id = se.entity_id and other.attribute_id = 'id-other'
join stream_value_varchar strength on strength.entity_id = se.entity_id and strength.attribute_id = 'strength'
join stream_value_varchar accurate on accurate.entity_id = se.entity_id and accurate.attribute_id = 'accurate'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({groupCompositionPupAssociationIds});";

            return RunSql<PupAssociation>(stringSql).ToList();
        }

        private static List<GroupCompositionMateGuard> GetMateGuardList(string groupCompositionMateGuardIds)
        {
            var stringSql = $@"
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = individual.value and svv.attribute_id = 'name' ) as 'femaleName'
,individual.value as 'femaleIndividualId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = other.value and svv.attribute_id = 'name' ) as 'guardName'
,other.value as 'guardIndividualId'
,strength.value as 'strength'
,pester.value as 'pester'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
from stream_entity se
join stream_value_varchar individual on individual.entity_id = se.entity_id and individual.attribute_id = 'id-mongoose'
join stream_value_varchar other on other.entity_id = se.entity_id and other.attribute_id = 'id-other'
join stream_value_varchar strength on strength.entity_id = se.entity_id and strength.attribute_id = 'strength'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_varchar user on user.entity_id = se.entity_id and user.attribute_id = 'user'
join stream_value_int pester on pester.entity_id = se.entity_id and pester.attribute_id = 'pester'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
where se.entity_id in ({groupCompositionMateGuardIds});";

            return RunSql<GroupCompositionMateGuard>(stringSql).ToList();
        }

        private static List<GroupWeightMeasure> GetWeightList(string groupCompositionWeightIds)
        {
            var stringSql = $@"
 select se.entity_id 
,se.sent
,se.entity_type
,se.unique_id as 'UniqueId'
,(select svv.value from sync_entity se
join sync_value_varchar svv on svv.entity_id = se.entity_id
where se.unique_id = individual.value and svv.attribute_id = 'name' ) as 'individualName'
,individual.value as 'individualId'
,name.value as 'name'
,time.value as 'time'
,lat.value as 'latitude'
,lon.value as 'longitude'
,weight.value as 'weight'
,collarWeight.value as 'collarWeight'
,present.value as 'present'
,accurate.value as 'accurate'
from stream_entity se
join stream_value_varchar individual on individual.entity_id = se.entity_id and individual.attribute_id = 'id-mongoose'
join stream_value_varchar name on name.entity_id = se.entity_id and name.attribute_id = 'name'
join stream_value_varchar time on time.entity_id = se.entity_id and time.attribute_id = 'time'
join stream_value_varchar user on user.entity_id = se.entity_id and user.attribute_id = 'user'
join stream_value_real weight on weight.entity_id = se.entity_id and weight.attribute_id = 'weight'
join stream_value_real collarWeight on collarWeight.entity_id = se.entity_id and collarWeight.attribute_id = 'cur-collar-weight'
join stream_value_real lat on lat.entity_id = se.entity_id and lat.attribute_id = 'lat'
join stream_value_real lon on lon.entity_id = se.entity_id and lon.attribute_id = 'lon'
join stream_value_int present on present.entity_id = se.entity_id and present.attribute_id = 'present'
join stream_value_int accurate on accurate.entity_id = se.entity_id and accurate.attribute_id = 'accurate'
where se.entity_id in ({groupCompositionWeightIds});";

            return RunSql<GroupWeightMeasure>(stringSql).ToList();
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