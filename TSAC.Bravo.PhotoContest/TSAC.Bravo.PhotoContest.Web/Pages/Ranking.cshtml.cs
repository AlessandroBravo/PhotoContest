using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TSAC.Bravo.PhotoContest.Cache;
using TSAC.Bravo.PhotoContest.Data;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Web.Pages
{
    public class RankingModel : PageModel
    {
        private readonly IDataAccess _data;
        public UserManager<IdentityUser> _userManager;
        private readonly ICacheAccess _cacheAccess;
        public IEnumerable<Photo> Photos { get; set; }
       
        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userManager"></param>
        public RankingModel(IDataAccess data, UserManager<IdentityUser> userManager, ICacheAccess cacheAccess)
        {
            _data = data;
            _userManager = userManager;
            _cacheAccess = cacheAccess;
        }

        /// <summary>
        /// OnGet function
        /// </summary>
        public void OnGet()
        {
            try
            {
                try
                {
                    Photos = _cacheAccess.GetRank();
                }
                catch (Exception)
                {
                    Photos = _data.GetRanking();
                }
            }
            catch (Exception)
            {
                Photos = null;
            }
        }
    }
}