using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public class InsertModel : PageModel
    {
        private readonly IDataAccess _data;
        public UserManager<IdentityUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IUploadLibrary _uploadAws;
        private readonly IUploadLibrary _uploadAzure;
        public InsertModel(IDataAccess data, UserManager<IdentityUser> userManager, IConfiguration config, IUploadLibrary uploadAws, IUploadLibrary uploadAzure)
        {
            _data = data;
            _userManager = userManager;
            _config = config;
            _uploadAws = uploadAws;
            _uploadAzure = uploadAzure;
        }

        public void OnGet()
        {
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

                        _data.AddPhoto(new Photo
                        {
                            Url = url,
                            Average = 0,
                            Votes = 0,
                            Total = 0,
                            UserName = _userManager.GetUserId(User),
                            Title = Photo.Title,
                            Description = Photo.Description
                        });

                        SendToQueue(url);
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

        public void SendToQueue(string message)
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine("rabbitmq error"+ e);
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

    }
}