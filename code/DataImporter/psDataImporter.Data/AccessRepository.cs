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
    }
}