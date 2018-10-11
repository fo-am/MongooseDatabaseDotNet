using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Linq;
using Dapper;
using NLog;
using psDataImporter.Contracts.Access;

namespace psDataImporter.Data
{
    public class AccessRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public IEnumerable<Weights> GetWeights()
        {
            IEnumerable<Weights> weights = new List<Weights>();
            try
            {
                using (var conn = new OleDbConnection(ConfigurationManager
                    .ConnectionStrings["accessConnectionString"]
                    .ConnectionString))
                {
                    Logger.Info("Getting weight data");
                    conn.Open();
                    weights = conn.Query<Weights>(
                        @"SELECT *, [DATE] + IIF(ISNULL([TIME]),0,[TIME]) as [TimeMeasured] FROM WEIGHTS"); // null times are turned to a value.
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Access error" + ex.Message);
            }
            return weights;
        }

        public IEnumerable<Ultrasound> GetUltrasounds()
        {
            IEnumerable<Ultrasound> ultrasounds = new List<Ultrasound>();
            try
            {
                using (var conn = new OleDbConnection(ConfigurationManager
                    .ConnectionStrings["accessConnectionString"]
                    .ConnectionString))
                {
                    Logger.Info("Getting ultrasound data");
                    conn.Open();
                    ultrasounds = conn.Query<Ultrasound>(
                        @"SELECT 
                            [DATE] ,
                            [INDIV] ,
                            [PACK] ,
                            [FOETUS NUMBER] ,
                            [FOETUS SIZE] as FOETUS_SIZE,
                            [FOETUS 1 - CROSS VIEW LENGTH] as FOETUS_1_CROSS_VIEW_LENGTH ,
                            [FOETUS 1 - CROSS VIEW WIDTH] as FOETUS_1_CROSS_VIEW_WIDTH ,
                            [FOETUS 1 - LONG VIEW LENGTH] as FOETUS_1_LONG_VIEW_LENGTH ,
                            [FOETUS 1 - LONG VIEW WIDTH] as FOETUS_1_LONG_VIEW_WIDTH ,
                            [FOETUS 2 - CROSS VIEW LENGTH] as FOETUS_2_CROSS_VIEW_LENGTH ,
                            [FOETUS 2 - CROSS VIEW WIDTH] as FOETUS_2_CROSS_VIEW_WIDTH ,
                            [FOETUS 2 - LONG VIEW LENGTH] as FOETUS_2_LONG_VIEW_LENGTH ,
                            [FOETUS 2 - LONG VIEW WIDTH] as FOETUS_2_LONG_VIEW_WIDTH ,
                            [FOETUS 3 -  CROSS VIEW LENGTH] as FOETUS_3_CROSS_VIEW_LENGTH ,
                            [FOETUS 3 - CROSS VIEW WIDTH] as FOETUS_3_CROSS_VIEW_WIDTH ,
                            [FOETUS 3 - LONG VIEW LENGTH] as FOETUS_3_LONG_VIEW_LENGTH ,
                            [FOETUS 3 - LONG VIEW WIDTH] as FOETUS_3_LONG_VIEW_WIDTH ,
                            [FOETUS 4 - CROSS VIEW LENGTH] as FOETUS_4_CROSS_VIEW_LENGTH ,
                            [FOETUS 4 - CROSS VIEW WIDTH] as FOETUS_4_CROSS_VIEW_WIDTH ,
                            [FOETUS 4 - LONG VIEW LENGTH] as FOETUS_4_LONG_VIEW_LENGTH ,
                            [FOETUS 4 - LONG VIEW WIDTH] as FOETUS_4_LONG_VIEW_WIDTH ,
                            [FOETUS 5 - CROSS VIEW LENGTH] as FOETUS_5_CROSS_VIEW_LENGTH ,
                            [FOETUS 5 - CROSS VIEW WIDTH] as FOETUS_5_CROSS_VIEW_WIDTH ,
                            [FOETUS 5 - LONG VIEW LENGTH] as FOETUS_5_LONG_VIEW_LENGTH ,
                            [FOETUS 5 - LONG VIEW WIDTH] as FOETUS_5_LONG_VIEW_WIDTH ,
                            [FOETUS 6 - CROSS VIEW LENGTH] as FOETUS_6_CROSS_VIEW_LENGTH ,
                            [FOETUS 6 - CROSS VIEW WIDTH] as FOETUS_6_CROSS_VIEW_WIDTH ,
                            [FOETUS 6 - LONG VIEW LENGTH] as FOETUS_6_LONG_VIEW_LENGTH ,
                            [FOETUS 6 - LONG VIEW WIDTH] as FOETUS_6_LONG_VIEW_WIDTH ,
                            [OBSERVER] ,
                            [COMMENT]
                            FROM
                            [ULTRASOUND DATA]");
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Access error" + ex.Message);
            }
            return ultrasounds;
        }

        public IEnumerable<RadioCollar> GetRadioCollars()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Radio Collar data");
                return conn.Query<RadioCollar>(@" 
                                        SELECT
                                        PACK,
                                        INDIVIDUAL,
                                        FREQUENCY,
                                        [TURNED ON] AS TURNED_ON,
                                        FITTED,
                                        REMOVED,
                                        [WEIGHT(G)] AS WEIGHT,
                                        DATE_ENTERED
                                        FROM [RADIOCOLLAR RECORDS]").ToList();
            }
        }

        public List<NewLifeHistory> GetLifeHistorys()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Life History data");

                return conn.Query<NewLifeHistory>(@" 
                                      SELECT DATE, 
                                            PACK, 
                                            INDIV, 
                                            SEX, 
                                            [AGE CAT] as AgeCat, 
                                            STATUS, 
                                            [START/END] as StartEnd, 
                                            CODE, 
                                            EXACT, 
                                            LSEEN, 
                                            CAUSE, 
                                            LITTER, 
                                            [PREV NAME] as PrevName, 
                                            COMMENT, 
                                            EDITED, 
                                            Latitude,
                                            Longitude,
                                            date_entered
                                            FROM [NEW LIFE HISTORY];
                                            ").ToList();
            }
        }

        public IEnumerable<Oestrus> GetOestruses()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Life History data");

                return conn.Query<Oestrus>(@" 
                                      SELECT 
                                            DATE,
                                            TIME,
                                            GROUP,
                                            [changes to data] as changes_to_data,   
                                            COMMENT,                                
                                            CONFIDENCE,                             
                                            COPULATION,  
                                            [FEMALE ID] as FEMALE_ID,               
                                            [GUARD ID] as GUARD_ID,                 
                                            Latitude,                               
                                            Longitude,                              
                                            [OESTRUS CODE] as OESTRUS_CODE,         
                                            [PESTERER ID] as PESTERER_ID,           
                                            [PESTERER ID 2] as PESTERER_ID_2,       
                                            [PESTERER ID 3] as PESTERER_ID_3,       
                                            [PESTERER ID 4] as PESTERER_ID_4,       
                                            STRENGTH                                         
                                            FROM [OESTRUS];
                                            ").ToList();
            }
        }

        public List<CapturesNew2013> GetCaptures()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Life History data");

                return conn.Query<CapturesNew2013>(@" 
                                      SELECT 
                                           [Capture DATE] as Capture_DATE ,
                                           [INDIV] ,
                                           [TRANSPONDER] ,
                                           [PACK] ,
                                           [PACK STATUS] as PACK_STATUS ,
                                           [TRAP LOCN] as TRAP_LOCN ,
                                           [TRAP TIME] as TRAP_TIME ,
                                           [PROCESS TIME] as PROCESS_TIME ,
                                           [BLEED TIME] as BLEED_TIME,
                                           [RELEASE TIME] as RELEASE_TIME ,
                                           [AGE] ,
                                           [Examiner] ,
                                           [DRUGS] ,
                                           [SEX] ,
                                           [REPRO STATUS] as REPRO_STATUS,
                                           [TEATS EXT?] as TEATS_EXT ,
                                           [ULTRASOUND?] as ULTRASOUND ,
                                           [FOETUSES] ,
                                           [FOET SIZE] as FOET_SIZE,
                                           [WEIGHT] ,
                                           [HEAD WIDTH] as HEAD_WIDTH ,
                                           [HEAD LENGTH] as HEAD_LENGTH ,
                                           [BODY LENGTH] as BODY_LENGTH ,
                                           [HINDFOOT LENGTH] as HINDFOOT_LENGTH,
                                           [TAIL LENGTH] as TAIL_LENGTH,
                                           [TAIL CIRC] as TAIL_CIRC ,
                                           [TICKS] ,
                                           [FLEAS] ,
                                           [SCARS / WOUNDS] as SCARS_WOUNDS ,
                                           [PLASMA SAMPLE PL] as PLASMA_SAMPLE_PL ,
                                           [FREEZE TIME PL] as FREEZE_TIME_PL,
                                           [BLOOD SAMPLE BL] as  BLOOD_SAMPLE_BL,
                                           [FREEZE TIME BL] as FREEZE_TIME_BL,
                                           [BUCKET PLxxx AND BLxxx] as BUCKET_PLxxx_AND_BLxxx ,
                                           [White blood WBC]  as White_blood_WBC,
                                           [FREEZE TIME WBC] as FREEZE_TIME_WBC,
                                           [BUCKET WBC] as BUCKET_WBC,
                                           [WHISKER SAMPLE WSK] as WHISKER_SAMPLE_WSK,
                                           [EAR CLIP TAKEN?] as EAR_CLIP_TAKEN,
                                           [TAIL TIP?] as TAIL_TIP ,
                                           [2D4D photos?] as twoDfourD_photos ,
                                           [AGD photos?] as AGD_photos ,
                                           [Blood sugar] as Blood_sugar,
                                           [Red cell percentage] ,
                                           [Fat: neck 1] as  Fat_neck_1 ,
                                           [Fat: neck 2] as Fat_neck_2 ,
                                           [Fat: armpit] as Fat_armpit ,
                                           [Fat: thigh] as Fat_thigh ,
                                           [COMMENTS] ,
                                           [EDITED] ,
                                           [TESTES L] as TESTES_L,
                                           [TESTES W] as TESTES_W,
                                           [TOOTH WEAR] as TOOTH_WEAR,
                                           [TESTES DEPTH] as TESTES_DEPTH
                                        FROM [CAPTURES NEW 2013];
                                            ").ToList();
            }
        }

        public List<PupAssociation> GetPupAssocs()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting pup associations.");

                return conn.Query<PupAssociation>(@" 
                                      SELECT                         
                                        [DATE]                              
                                        ,[SESSION]                           
                                        ,GROUP                             
                                        ,LITTER                            
                                        ,PUP                               
                                        ,[PUP SEX] as PUP_SEX              
                                        ,ESCORT                            
                                        ,[ESC SEX] as ESC_SEX              
                                        ,STRENGTH                          
                                        ,CONFIDENCE                        
                                        ,COMMENT                           
                                        ,[Editing comments] as Editing_comments                
                                        ,Latitude                                 
                                        ,Longitude
                                        from 
                                        [PUP ASSOCIATION];").ToList();
            }
        }

        public List<BABYSITTING_RECORDS> GetBabysittingRecords()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Babysitting Records.");

                return conn.Query<BABYSITTING_RECORDS>(@"
            Select 
            [DATE]
            ,GROUP
            ,[LITTER CODE] as LITTER_CODE
            ,BS
            ,SEX
            ,TYPE
            ,[TIME START] as TIME_START
            ,[DEN DIST] as DEN_DIST
            ,[TIME END] as TIME_END
            ,ACCURACY
            ,Edited
            ,COMMENT
            ,Latitude
            ,Longitude
             from 
            [BABYSITTING RECORDS];").ToList();
            }
        }


        public IEnumerable<DiaryAndGrpComposition> GetGroupCompositions()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Babysitting Records.");

                return conn.Query<DiaryAndGrpComposition>(@"
            Select           
            [Date],
            [Pack],
            [Observer],
            [Session],
            [Group status] as Group_status,
            [ST Weather] as ST_Weather,
            [END Weather] as END_Weather,
            [Males > 1 yr] as Males_one_yr,
            [Females > 1 yr] as Females_one_yr,
            [Males > 3 months] as Males_three_months,
            [Females > 3 months] as Females_three_months,
            [Male em pups] as Male_em_pups,
            [Female em pups] as Female_em_pups,
            [Unk em pups] as Unk_em_pups,
            [Pups in Den?] as Pups_in_Den,
            [Comment]
             
           FROM [DIARY AND GRP COMPOSITION];").ToList();
            }
        }

        public List<POO_DATABASE> GetPoo()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting poo samples.");

                return conn.Query<POO_DATABASE>(@"
                                select
                                 [Sample Number] as Sample_Number
                                ,[Date]
                                ,Pack
                                ,[Pack Status] as Pack_Status
                                ,[Emergence Time] as Emergence_Time
                                ,[Time in Freezer] as Time_in_Freezer
                                ,Individual
                                ,[Time of Collection] as Time_of_Collection
                                ,[Parasite sample taken] as Parasite_sample_taken
                                ,Comments
             
                   FROM [POO DATABASE];").ToList();
            }
        }

        public List<METEROLOGICAL_DATA> GetWeather()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting meterological data.");

                return conn.Query<METEROLOGICAL_DATA>(@"
                                        select
                                        [DATE]
                                        ,[RAIN_MWEYA] as RAIN_MWEYA
                                        ,[MAX TEMP] as MAX_TEMP
                                        ,[MIN TEMP] as MIN_TEMP
                                        ,[TEMP]
                                        ,[Min humidity %] as MIN_HUMIDITY
                                        ,[Max humidity %] as MAX_HUMIDITY
                                        ,[TIME TAKEN] as TIME_TAKEN
                                        ,[OBSERVER] as OBSERVER
                                           FROM [METEROLOGICAL DATA];").ToList();
            }
        }

        public List<Maternal_Condition_Experiment_Litters> GetMaternalConditionLitters()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting maternal condition:litters data.");

                return conn.Query<Maternal_Condition_Experiment_Litters>(@"
                                        select
                                             [Experiment Number] as Experiment_Number
                                            ,[Pack]
                                            ,[Litter]
                                            ,[Preg check trap date] as Preg_check_trap_date
                                            ,[Date started] as Date_started
                                            ,[Type of experiment] as Type_of_experiment
                                            ,[Foetus age at start (weeks)] as Foetus_age_at_start_weeks
                                            ,[No of T females] as No_of_T_females
                                            ,[No of C females] as No_of_C_females
                                            ,[Record]
                                            ,[Litter birth date] as Litter_birth_date
                                            ,[Notes]
                                         FROM [Maternal Condition Experiment: Litters];").ToList();
            }
        }

        public List<Maternal_Condition_Experiment_Females> GetMaternalConditionFemales()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting maternal condition:females data.");

                return conn.Query<Maternal_Condition_Experiment_Females>(@"
                                                            select
                                                            [Pack]
                                                            ,[Litter]
                                                            ,[Experiment type] as Experiment_type
                                                            ,[Female ID] as Female_ID
                                                            ,[Category]
                                                            ,[Paired female ID] as Paired_female_ID
                                                            ,[Notes]
                                                            FROM [Maternal Condition Experiment: Females];").ToList();
            }

    }

        public List<Jennis_blood_data> GetBloodData()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting blood data.");

                return conn.Query<Jennis_blood_data>(@"
                                         SELECT
                                            [Date] 
                                            ,[Mongoose] 
                                            ,[Trap time] as Trap_time
                                            ,[Bleed time (from stopwatch)] as Bleed_time
                                            ,[Weight]
                                            ,[Release time] as Release_time
                                            ,[Sample] 
                                            ,[Spinning time] as Spinning_time
                                            ,[Freeze time] as Freeze_time
                                            ,[Focal]
                                            ,[Amount of plasma (ul)] as Amount_of_plasma
                                            ,[Comment] as Comment
                                          FROM [Jenni's blood data];").ToList();
            }
        }

        public List<HPA_samples> GetHpaSamples()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting HPA sample data.");

                return conn.Query<HPA_samples>(@"
                                         SELECT
                                            [Date]
                                            ,[ID] as ID
                                            ,[Time in trap] as Time_in_trap
                                            ,[Time of capture] as Time_of_capture
                                            ,[First blood sample - stopwatch time] as First_blood_sample_stopwatch_time
                                            ,[First sample number] as First_sample_number
                                            ,[Second blood sample - stopwatch time] as Second_blood_sample_stopwatch_time
                                            ,[Second sample number] as Second_sample_number
                                            ,[Head width] as Head_width
                                            ,[Weight]
                                            ,[Ticks]
                                            ,[Second sample freezer time] as Second_sample_freezer_time
                                            ,[First sample freezer time] as First_sample_freezer_time
                                          FROM  [HPA samples];").ToList();
            }
        }

        public List<DNA_SAMPLES> GetDnaSamples()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting DNA sample data.");

                return conn.Query<DNA_SAMPLES>(@"
                                            SELECT
                                                [ID1]
                                                ,[DATE]
                                                ,[SAMPLE TYPE] as SAMPLE_TYPE
                                                ,[TISSUE]
                                                ,[STORAGE] as STORAGE_ID
                                                ,[ID]
                                                ,[TUBE ID] as TUBE_ID
                                                ,[AGE]
                                                ,[SEX]
                                                ,[DISPERSAL]
                                                ,[PACK] as PACK
                                                ,[LITTER CODE] as LITTER
                                                ,[COMMENT] as COMMENT
                                                ,[box slot] as box_slot
                                            FROM [DNA SAMPLES];").ToList();
            }
        }

        public List<Antiparasite_experiment> GetAntiParasite()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Antiparasite_experiment sample data.");

                return conn.Query<Antiparasite_experiment>(@"
                                            SELECT
                                                [PACK]
                                                ,[INDIV]
                                                ,[STARTED EXPERIMENT] as STARTED_EXPERIMENT
                                                ,[A FECAL SAMPLE] as A_FECAL_SAMPLE
                                                ,[FIRST capture] as FIRST_capture
                                                ,[EXPERIMENT GROUP] as EXPERIMENT_GROUP
                                                ,[B FECAL] as B_FECAL
                                                ,[C FECAL] as C_FECAL
                                                ,[SECOND CAPTURE] as SECOND_CAPTURE
                                                ,[D FECAL] as D_FECAL
                                                ,[E FECAL] as E_FECAL
                                                ,[F FECAL] as F_FECAL
                                                ,[notes] as notes
                                            FROM [Antiparasite experiment];").ToList();
            }
        }

        public List<ProvisioningData> GetMaternalConditionProvisioning()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Antiparasite_experiment sample data.");

                return conn.Query<ProvisioningData>(@"
                                            SELECT
                                             [Date]
                                            ,[Visit time] as Visit_time
                                            ,Pack
                                            ,Litter
                                            ,[Female ID] as Female_ID
                                            ,[Amount of egg] as Amount_of_egg
                                            ,Comments
                                            FROM [Maternal Condition Experiment: provisioning data];").ToList();
            }
        }

        public List<OxShieldingFeedingRecord> GetOxFeeding()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Ox shielding - feeding record data.");

                return conn.Query<OxShieldingFeedingRecord>(@"
                                 SELECT
                                  [Date]
                                  ,[AM/PM] as AMPM
                                  ,[Amount of egg (g)] as Amount_of_egg
                                  ,[Female ID] as Female_ID
                                  ,[Pack]
                                  ,[Comments]
                                  FROM [Ox shielding experiment - feeding record];").ToList();
            }
        }

        public List<OxShieldingMalesBeingSampled> GetOxMale()
        {
            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Ox shielding - males being sampled.");

                return conn.Query<OxShieldingMalesBeingSampled>(@"
                                 SELECT
                                 [PACK]
                                ,[ID]
                                ,[STATUS]
                                ,[DATE START] as DATE_START
                                ,[DATE STOP] as DATE_STOP
                                ,[COMMENT]

                                  FROM [Ox Shielding Experiment - males being sampled];").ToList();
            }
        }

        public List<OxShieldingFemaleTreatmentGroups> GetOxFemale()
        {

            using (var conn = new OleDbConnection(ConfigurationManager
                .ConnectionStrings["accessConnectionString"]
                .ConnectionString))
            {
                Logger.Info("Getting Ox shielding - males being sampled.");

                return conn.Query<OxShieldingFemaleTreatmentGroups>(@"
                                 SELECT
                                     [Pack]
                                    ,[ID]
                                    ,[Treatment Group] as Treatment_Group
                                    ,[Date Started] as Date_Started
                                    ,[Comment]
                                  FROM [Ox Shielding Experiment - female treatment groups];").ToList();
            }
        }
    }
}
