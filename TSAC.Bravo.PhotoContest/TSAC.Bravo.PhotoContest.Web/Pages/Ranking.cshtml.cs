using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TSAC.Bravo.PhotoContest.Data;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Web.Pages
{
    public class RankingModel : PageModel
    {
        private readonly IDataAccess _data;
        public UserManager<IdentityUser> _userManager;
        public IEnumerable<Photo> Photos { get; set; }
       
        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userManager"></param>
        public RankingModel(IDataAccess data, UserManager<IdentityUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }

        /// <summary>
        /// OnGet function
        /// </summary>
        public void OnGet()
        {
            Photos = _data.GetRanking();
        }
    }
}