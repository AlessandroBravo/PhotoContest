using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using TSAC.Bravo.PhotoContest.Data;
using TSAC.Bravo.PhotoContest.Data.Models;
using TSAC.Bravo.PhotoContest.Upload;

namespace TSAC.Bravo.PhotoContest.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IDataAccess _data;
        public UserManager<IdentityUser> _userManager;
        public IEnumerable<Photo> Photos { get; set; }

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userManager"></param>
        public IndexModel(IDataAccess data, UserManager<IdentityUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }

        /// <summary>
        /// OnGet function
        /// </summary>
        public void OnGet()
        {
            try
            {
                Photos = _data.GetPhotos();
            }
            catch (Exception)
            {
                Photos = null;
            }
        }

        /// <summary>
        /// Checks if an image has been voted from the user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Voted(int id)
        {
            Vote vote = _data.GetPhotoUser(id, _userManager.GetUserId(User));
            if (vote != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update the database with the vote of the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="voteChoice"></param>
        /// <returns></returns>
        public IActionResult OnPostVote(int id, int voteChoice)
        {
            try
            {
                if (voteChoice != 0)
                {
                    Photo photo = _data.GetPhoto(id);
                    var nvote = photo.Votes + 1;
                    var total = photo.Total + voteChoice;
                    var average = (decimal)total / nvote;
                    _data.AddVote(
                        new Vote
                        {
                            PhotoId = id,
                            UserId = _userManager.GetUserId(User),
                            Rating = voteChoice
                        },
                        new Photo
                        {
                            Id = id,
                            Votes = nvote,
                            Average = average,
                            Total = total,
                            UserName = _userManager.GetUserId(User)
                        }
                        );
                }
                return RedirectToPage("/Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return Page();
            }
        }

    }
}
