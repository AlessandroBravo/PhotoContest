using System;
using System.Collections.Generic;
using System.Text;

namespace TSAC.Bravo.PhotoContest.Data.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public int Votes { get; set; }
        public int Total { get; set; }
        public decimal Average { get; set; }
        public string UserName { get; set; }
    }
}
