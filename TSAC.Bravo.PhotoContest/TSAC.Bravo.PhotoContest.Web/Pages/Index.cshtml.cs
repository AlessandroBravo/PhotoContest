using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TSAC.Bravo.PhotoContest.Cache;
using TSAC.Bravo.PhotoContest.Data;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Web.Pages
{
    public class IndexModel : PageModel
    {
        public readonly IDataAccess _data;
        public readonly ICacheAccess _cacheAccess;
        public UserManager<IdentityUser> _userManager;
        public IEnumerable<Photo> Photos { get; set; }

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userManager"></param>
        public IndexModel(IDataAccess data, UserManager<IdentityUser> userManager, ICacheAccess cacheAccess)
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
                    Photos = _cacheAccess.GetPhotos();
                }
                catch (Exception)
                {
                    Photos = _data.GetPhotos();
                }
            }
            catch (Exception)
            {
                Photos = null;
            }
        }

        /// <summary>
        /// Check if an image has been voted from the user
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public bool Voted(int photoId)
        {
            Vote vote = _data.GetPhotoUser(photoId, _userManager.GetUserId(User));
            if (vote != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check the owner of a photo
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public bool IsOwner(int photoId)
        {
            Photo photo = _data.GetPhoto(photoId);
            return photo.UserId == _userManager.GetUserId(User);
        }

        /// <summary>
        /// Update the database with the vote of the user
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="voteChoice"></param>
        /// <returns></returns>
        public IActionResult OnPostVote(int photoId, int voteChoice)
        {
            try
            {
                if (voteChoice != 0)
                {
                    Photo photo = _data.GetPhoto(photoId);
                    var nvote = photo.Votes + 1;
                    var total = photo.Total + voteChoice;
                    var average = (decimal)total / nvote;
                    _data.AddVote(
                        new Vote
                        {
                            PhotoId = photoId,
                            UserId = _userManager.GetUserId(User),
                            Rating = voteChoice
                        },
                        new Photo
                        {
                            Id = photoId,
                            Votes = nvote,
                            Average = average,
                            Total = total,
                            UserName = _userManager.GetUserId(User)
                        }
                        );
                    _cacheAccess.AddVote(photoId, voteChoice);
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
