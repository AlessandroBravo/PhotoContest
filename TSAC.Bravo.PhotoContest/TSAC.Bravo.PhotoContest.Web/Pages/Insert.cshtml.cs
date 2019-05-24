using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using TSAC.Bravo.PhotoContest.Cache;
using TSAC.Bravo.PhotoContest.Data;
using TSAC.Bravo.PhotoContest.Data.Models;
using TSAC.Bravo.PhotoContest.Queue;
using TSAC.Bravo.PhotoContest.Upload;

namespace TSAC.Bravo.PhotoContest.Web.Pages
{
    [Authorize]
    public class InsertModel : PageModel
    {
        public readonly IDataAccess _data;
        public UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IUploadLibrary _uploadAws;
        private readonly IUploadLibrary _uploadAzure;
        private readonly ICacheAccess _cacheAccess;
        private readonly IQueueAccess _queueAccess;

        public InsertModel(IDataAccess data, 
            UserManager<IdentityUser> userManager, 
            IConfiguration config, 
            IUploadLibrary uploadAws, 
            IUploadLibrary uploadAzure, 
            ICacheAccess cacheAccess, 
            IQueueAccess queueAccess)
        {
            _data = data;
            _userManager = userManager;
            _config = config;
            _uploadAws = uploadAws;
            _uploadAzure = uploadAzure;
            _cacheAccess = cacheAccess;
            _queueAccess = queueAccess;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public PhotoModel Photo { get; set; }

        public class PhotoModel
        {
            [DataType(DataType.Text)]
            public string Title { get; set; }

            [DataType(DataType.Text)]
            public string Description { get; set; }

            [DataType(DataType.Upload)]
            public IFormFile Image { get; set; }
        };

        /// <summary>
        /// form action to upload a photo
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (IsImage(Photo.Image))
                    {
                        string url = _uploadAws.GetCdn() + Photo.Image.FileName;

                        await _uploadAws.Upload(Photo.Image.OpenReadStream(), Photo.Image.FileName);

                        Photo photo = new Photo
                        {
                            Url = url,
                            //Average = 0,
                            //Votes = 0,
                            //Total = 0,
                            UserName = _userManager.GetUserId(User),
                            Description = Photo.Description,
                            Title = Photo.Title,
                            UploadTimestamp = new DateTime()
                        };

                        _data.AddPhoto(photo);
                        //_cacheAccess.InsertPhoto(photo);
                        _queueAccess.SendToQueue(url);
                    }

                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
                return RedirectToPage("./Index");
            }
            return Page();
        }

        /// <summary>
        /// Checks if the file in input is an image
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool IsImage(IFormFile file)
        {
            //-------------------------------------------
            //  Check image
            //-------------------------------------------
            if (file == null || file.Length == 0)
            {
                return false;
            }
            //-------------------------------------------
            //  Check the image mime types
            //-------------------------------------------
            if (file.ContentType.ToLower() != "image/jpg" &&
                        file.ContentType.ToLower() != "image/jpeg" &&
                        file.ContentType.ToLower() != "image/pjpeg" &&
                        file.ContentType.ToLower() != "image/gif" &&
                        file.ContentType.ToLower() != "image/x-png" &&
                        file.ContentType.ToLower() != "image/png")
            {
                return false;
            }

            //-------------------------------------------
            //  Check the image extension
            //-------------------------------------------
            if (Path.GetExtension(file.FileName).ToLower() != ".jpg"
                && Path.GetExtension(file.FileName).ToLower() != ".png"
                && Path.GetExtension(file.FileName).ToLower() != ".gif"
                && Path.GetExtension(file.FileName).ToLower() != ".jpeg")
            {
                return false;
            }
            return true;
        }

    }
}