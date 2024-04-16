using System;

namespace pgDataImporter.Contracts.Access
{
    public class CapturesNew2013
    {
        public DateTime? Capture_DATE { get; set; }
        public string INDIV { get; set; }
        public string TRANSPONDER { get; set; }
        public string PACK { get; set; }
        public string PACK_STATUS { get; set; }
        public string TRAP_LOCN { get; set; }
        public DateTime? TRAP_TIME { get; set; }
        public DateTime? PROCESS_TIME { get; set; }
        public DateTime? BLEED_TIME { get; set; }
        public DateTime? RELEASE_TIME { get; set; }
        public string AGE { get; set; }
        public string Examiner { get; set; }
        public string DRUGS { get; set; }
        public string SEX { get; set; }
        public string REPRO_STATUS { get; set; }
        public string TEATS_EXT { get; set; }
        public string ULTRASOUND { get; set; }
        public int? FOETUSES { get; set; }
        public string FOET_SIZE { get; set; }
        public int? WEIGHT { get; set; }
        public decimal? HEAD_WIDTH { get; set; }
        public decimal? HEAD_LENGTH { get; set; }
        public int? BODY_LENGTH { get; set; }
        public decimal? HINDFOOT_LENGTH { get; set; }
        public int? TAIL_LENGTH { get; set; }
        public int? TAIL_CIRC { get; set; }
        public int? TICKS { get; set; }
        public int? FLEAS { get; set; }
        public string SCARS_WOUNDS { get; set; }
        public string PLASMA_SAMPLE_PL { get; set; }
        public DateTime? FREEZE_TIME_PL { get; set; }
        public string BLOOD_SAMPLE_BL { get; set; }
        public DateTime? FREEZE_TIME_BL { get; set; }
        public int? BUCKET_PLxxx_AND_BLxxx { get; set; }
        public string White_blood_WBC { get; set; }
        public DateTime? FREEZE_TIME_WBC { get; set; }
        public int? BUCKET_WBC { get; set; }
        public string WHISKER_SAMPLE_WSK { get; set; }
        public string EAR_CLIP_TAKEN { get; set; }
        public string TAIL_TIP { get; set; }
        public string twoDfourD_photos { get; set; }
        public string AGD_photos { get; set; }
        public double Blood_sugar { get; set; }
        public double Red_cell_percentage { get; set; }
        public string Fat_neck_1 { get; set; }
        public string Fat_neck_2 { get; set; }
        public double Fat_armpit { get; set; }
        public double Fat_thigh { get; set; }
        public string COMMENTS { get; set; }
        public string EDITED { get; set; }
        public string TESTES_L { get; set; }
        public string TESTES_W { get; set; }
        public string TOOTH_WEAR { get; set; }
        public double TESTES_DEPTH { get; set; }
    }
}