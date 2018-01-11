using System;

namespace psDataImporter.Contracts.Access
{
    public class HPA_samples

    {
        public DateTime Date { get; set; }
        public string ID { get; set; }
        public string Time_in_trap { get; set; }
        public DateTime Time_of_capture { get; set; }
        public string First_blood_sample_stopwatch_time { get; set; }
        public string First_sample_number { get; set; }
        public DateTime First_sample_freezer_time { get; set; }
        public string Second_blood_sample_stopwatch_time { get; set; }
        public string Second_sample_number { get; set; }
        public DateTime Second_sample_freezer_time { get; set; }
        public int Head_width { get; set; }
        public int Weight { get; set; }
        public int Ticks { get; set; }
       
        
    }
}