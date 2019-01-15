using System;
using System.Collections.Generic;
using System.Text;

namespace TSAC.Bravo.PhotoContest.Data.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int PhotoId { get; set; }
        public int Rating { get; set; }
    }
}
