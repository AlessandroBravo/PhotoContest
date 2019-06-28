using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using TSAC.Bravo.PhotoContest.Data;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Cache
{
    public class CacheAccess : ICacheAccess
    {
        private readonly IDataAccess _dataAccess;
        private readonly string _cacheConnectionString;
        private readonly string _rankIndex = "rank";
        private readonly string _photosIndex = "photos";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="configuration"></param>
        public CacheAccess(IDataAccess dataAccess, IConfiguration configuration)
        {
            this._dataAccess = dataAccess;
            this._cacheConnectionString = configuration["redis:conn"];
        }

        /// <summary>
        /// returns a list of photos ordered by score
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Photo> GetRank()
        {
            try
            {
                using (var client = new RedisManagerPool(_cacheConnectionString).GetClient())
                {
                    var list = client.GetAllItemsFromSortedSetDesc(_rankIndex) ?? null;
                    if (list != null)
                    {
                        IList<Photo> rank = null;
                        foreach (var item in list)
                        {
                            rank.Add(JsonConvert.DeserializeObject<Photo>(item));
                        }
                        return rank.Take(6);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            catch (RedisException ex)
            {
                Console.WriteLine(ex);
                return _dataAccess.GetRanking();
            }
            catch (Exception)
            {
                Console.WriteLine("No cached items found");
                var rank = _dataAccess.GetRanking();
                rank
                  .ToList()
                  .ForEach(photo => AddPhotoToRank(photo));
                return rank;
            }
        }

        /// <summary>
        /// return a list of photo from cache
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Photo> GetPhotos()
        {
            try
            {
                using (var client = new RedisManagerPool(_cacheConnectionString).GetClient())
                {
                    IList<Photo> photos = new List<Photo>();
                    var list = client.GetAllItemsFromSet(_photosIndex).ToList() ?? null;
                    if (list != null && list.Count != 0)
                    {
                        list.ForEach(
                          item =>
                          {
                              photos.Add(JsonConvert.DeserializeObject<Photo>(item));
                          }
                        );
                        return photos.OrderByDescending(d => d.Votes);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            catch (RedisException ex)
            {
                Console.WriteLine(ex);
                return _dataAccess.GetPhotos();
            }
            catch (Exception)
            {
                Console.WriteLine("No cached items found");
                var photos = _dataAccess.GetPhotos();
                photos
                  .ToList()
                  .ForEach(photo => AddPhoto(photo));
                return photos;
            }
        }

        /// <summary>
        /// adds the photo to rank list
        /// </summary>
        /// <param name="photo"></param>
        private void AddPhotoToRank(Photo photo)
        {
            try
            {
                using (var client = new RedisManagerPool(_cacheConnectionString).GetClient())
                {
                    var score =  100 * photo.Average + 10 * photo.Votes;
                    client.AddItemToSortedSet(_rankIndex, JsonConvert.SerializeObject(photo), (double) score);
                    client.ExpireEntryAt(_rankIndex, DateTime.Now.AddDays(1));
                }
            }
            catch (RedisException rex)
            {
                Console.WriteLine(rex);
            }
        }

        /// <summary>
        /// adds the photo to list
        /// </summary>
        /// <param name="photo"></param>
        private void AddPhoto(Photo photo)
        {
            try
            {
                using (var client = new RedisManagerPool(_cacheConnectionString).GetClient())
                {
                    client.AddItemToSet(_photosIndex, JsonConvert.SerializeObject(photo));
                    client.ExpireEntryAt(_photosIndex, DateTime.Now.AddDays(1));
                }
            }
            catch (RedisException rex)
            {
                Console.WriteLine(rex);
            }
        }

        /// <summary>
        /// returns a photo 
        /// </summary>
        /// <param name="photoId"></param>
        /// <returns></returns>
        public Photo GetPhoto(int photoId)
        {
            try
            {
                var t = GetPhotos();
                return t.FirstOrDefault(x => x.Id == photoId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return _dataAccess.GetPhoto(photoId);
            }
        }

        /// <summary>
        /// insert a photo to cache 
        /// </summary>
        /// <param name="photo"></param>
        public void InsertPhoto(Photo photo)
        {
            AddPhoto(photo);
            AddPhotoToRank(photo);
        }

        /// <summary>
        /// update the photo information with a new vote 
        /// </summary>
        /// <param name="photoId"></param>
        /// <param name="vote"></param>
        public void AddVote(int photoId, int vote)
        {
            try
            {
                using (var client = new RedisManagerPool(_cacheConnectionString).GetClient())
                {
                    var oldPhoto = GetPhoto(photoId);
                    Photo newPhoto = new Photo
                    {
                        Id = oldPhoto.Id,
                        Url = oldPhoto.Url,
                        Title = oldPhoto.Title,
                        Votes = oldPhoto.Votes + 1,
                        Average = (oldPhoto.Average + vote) / (oldPhoto.Votes + 1),
                        Description = oldPhoto.Description,
                        UserId = oldPhoto.UserId,
                        UserName = oldPhoto.UserName
                    };

                    // remove oldPhoto from cache
                    client.RemoveItemFromSet(_photosIndex, JsonConvert.SerializeObject(oldPhoto));
                    client.RemoveItemFromSortedSet(_rankIndex, JsonConvert.SerializeObject(oldPhoto));
                    // add newPhoto to cache
                    client.AddItemToSet(_photosIndex, JsonConvert.SerializeObject(newPhoto));
                    client.AddItemToSortedSet(_rankIndex, JsonConvert.SerializeObject(newPhoto));
                }
            }
            catch (RedisException ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
