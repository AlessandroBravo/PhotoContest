using System.Collections.Generic;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Data
{
    public interface IDataAccess
    {
        void AddPhoto(Photo photo);
        void AddVote(Vote vote, Photo photo);
        Photo GetPhoto(int photoId);
        IEnumerable<Photo> GetPhotos();
        Vote GetPhotoUser(int photoId, string userId);
        IEnumerable<Photo> GetRanking();
        void UpdatePhoto(Photo photo);
    }
}