using System;

namespace pgDataImporter.Contracts.Access
{
    public class DiaryAndGrpComposition
    {
        public string ID { get; set; }
        public DateTime Date { get; set; }
        public string Pack { get; set; }
        public string Observer { get; set; }
        public string Session { get; set; }
        public string Group_status { get; set; }
        public string ST_Weather { get; set; }
        public string END_Weather { get; set; }
        public string Males_one_yr { get; set; }
        public string Females_one_yr { get; set; }
        public string Males_three_months { get; set; }
        public string Females_three_months { get; set; }
        public string Male_em_pups { get; set; }
        public string Female_em_pups { get; set; }
        public string Unk_em_pups { get; set; }
        public string Pups_in_Den { get; set; }
        public string Comment { get; set; }
    }
}