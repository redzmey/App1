﻿namespace App1.Models
{
    public class OmnivaLocation : BaseLocation
    {
        public string CommentEng { get; set; }
        public string CommentEst { get; set; }
        public string CommentLav { get; set; }
        public string CommentLit { get; set; }
        public string CommentRus { get; set; }

        public string CountryCode { get; set; }

        public string FullAddress { get; set; }
        public string Modified { get; set; }
        //public string A1Name { get; set; }
        //public string A2Name { get; set; }
        //public string A3Name { get; set; }
        //public string A4Name { get; set; }
        //public string A5Name { get; set; }
        //public string A6Name { get; set; }
        //public string A7Name { get; set; }
        //public string A8Name { get; set; }
        public string ServiceHours { get; set; }
        public string TempServiceHours { get; set; }
        public string TempServiceHours2 { get; set; }
        public string TempServiceHours2Until { get; set; }
        public string TempServiceHoursUntil { get; set; }
        public string Zip { get; set; }
        //=> string.Join(Environment.NewLine,A1Name, A2Name, A3Name, A4Name, A5Name, A6Name, A7Name,A8Name).Replace(Environment.NewLine + "NULL","");
    }
}