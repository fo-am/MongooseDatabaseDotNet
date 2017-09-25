using System;

namespace psDataImporter.Contracts.Access
{
    public class NewLifeHistory
    {
        public string AgeCat;
        public string Cause;
        public string Code;
        public string Comment;
        public DateTime Date;
        public DateTime date_entered;
        public string Edited;
        public string Exact;
        public string Indiv;
        public string Litter;
        public string Latitude;
        public string Longitude;
        public DateTime Lseen;
        public string Pack;
        public string PrevName;
        public string Sex;
        public string StartEnd;
        public string Status;

        public override string ToString()
        {
            return String.Format($"AgeCat={AgeCat} Cause={ Cause} Code={ Code}  Comment={ Comment} Date={  Date } date_entered={  date_entered}            Edited={ Edited}           Exact= { Exact}            Indiv={ Indiv}            Litter={ Litter}            Latitude={ Latitude }            Longitude={ Longitude}            Lseen={  Lseen } Pack={ Pack } PrevName={ PrevName } Sex={ Sex } StartEnd={ StartEnd } Status={ Status } ");





        }
}
}