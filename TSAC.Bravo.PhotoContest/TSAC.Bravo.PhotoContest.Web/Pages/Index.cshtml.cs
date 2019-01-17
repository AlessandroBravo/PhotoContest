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
        private readonly IConfiguration _config;
        private readonly IUploadLibrary _uploadAws;
        private readonly IUploadLibrary _uploadAzure;
        public IEnumerable<Photo> Photos { get; set; }

        [BindProperty]
        public IFormFile photoUpload { get; set; }

        /// <summary>
        /// Costructor
        /// </summary>
        /// <param name="data"></param>
        /// <param name="userManager"></param>
        /// <param name="amazonS3"></param>
        /// <param name="config"></param>
        public IndexModel(IDataAccess data, UserManager<IdentityUser> userManager, IConfiguration config, IUploadLibrary uploadAws, IUploadLibrary uploadAzure)
        {
            _data = data;
            _userManager = userManager;
            _config = config;
            _uploadAws = uploadAws;
            _uploadAzure = uploadAzure;
        }

        /// <summary>
        /// OnGet function
        /// </summary>
        public void OnGet()
        {
            Photos = _data.GetPhotos();
        }


        /// <summary>
        /// Upload the selected image to Amazon S3
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> OnPost()
        {
            if (IsImage(photoUpload))
            {
                string url = _uploadAws.GetCdn() + photoUpload.FileName;

                await _uploadAws.Upload(photoUpload.OpenReadStream(), photoUpload.FileName);

                //await _uploadAzure.Upload(photoUpload.OpenReadStream(), photoUpload.FileName);

                _data.AddPhoto(new Photo
                {
                    Url = url,
                    Average = 0,
                    Votes = 0,
                    Total = 0,
                    UserName = _userManager.GetUserId(User)
                });

                //SendToQueue("hello world");

            }
            return RedirectToPage("./Index");
        }

        public void SendToQueue(string message)
        {
            var factory = new ConnectionFactory() { HostName = _config["RabbitMQ:HostName"], UserName = _config["RabbitMQ:UserName"], Password = _config["RabbitMQ:Password"] };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);


                var body = Encoding.UTF8.GetBytes(message);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: "task_queue",
                                     basicProperties: properties,
                                     body: body);
            }
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
                    var average = total / nvote;
                    _data.AddVotePhoto(new Photo { Id = id, Votes = nvote, Average = average, Total = total, UserName = _userManager.GetUserId(User) });
                    _data.AddVote(new Vote { PhotoId = id, UserId = _userManager.GetUserId(User), Rating = voteChoice });
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
