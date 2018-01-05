
using System;

namespace psDataImporter.Contracts.Access
{
    public class Maternal_Condition_Experiment_Litters
    {
        public int Experiment_Number { get; set; }
        public string Pack { get; set; }
        public string Litter { get; set; }
        public DateTime Preg_check_trap_date { get; set; }
        public DateTime Date_started { get; set; }
        public string Type_of_experiment { get; set; }
        public int Foetus_age_at_start_weeks { get; set; }
        public int No_of_T_females { get; set; }
        public int No_of_C_females { get; set; }
        public string Record { get; set; }
        public DateTime? Litter_birth_date { get; set; }
        public string Notes { get; set; }
    }
}