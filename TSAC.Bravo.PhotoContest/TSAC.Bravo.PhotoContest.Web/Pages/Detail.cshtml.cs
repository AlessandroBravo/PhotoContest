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
    public class DetailModel : PageModel
    {
        private readonly IDataAccess _data;
        public UserManager<IdentityUser> _userManager;
        public Photo Photo { get; set; }

        public DetailModel(IDataAccess data, UserManager<IdentityUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }

        public void OnGet(int id)
        {
            Photo = _data.GetPhoto(id);
        }


    }
}