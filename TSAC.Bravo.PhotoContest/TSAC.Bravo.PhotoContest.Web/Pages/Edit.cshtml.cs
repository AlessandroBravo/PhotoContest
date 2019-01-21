using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TSAC.Bravo.PhotoContest.Data;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Web.Pages
{
    public class EditModel : PageModel
    {
        private readonly IDataAccess _data;
        public UserManager<IdentityUser> _userManager;
        public EditModel(IDataAccess data, UserManager<IdentityUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }

        public void OnGet(int id)
        {
            photo = _data.GetPhoto(id);
        }

        [BindProperty]
        public Photo photo { get; set; }

        public ActionResult OnPost()
        {
            if (ModelState.IsValid)
            {
                _data.UpdatePhoto(new Photo
                {
                    Title = photo.Title,
                    Description = photo.Description,
                    Id = photo.Id
                });
                return RedirectToPage("./Index");
            }
            return Page();  
        }



    }
}