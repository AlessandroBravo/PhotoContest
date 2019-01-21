using System.Collections.Generic;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Data
{
    public interface IDataAccess
    {
        void AddPhoto(Photo photo);
        void AddVote(Vote vote, Photo photo);
        Photo GetPhoto(int Id);
        IEnumerable<Photo> GetPhotos();
        Vote GetPhotoUser(int id, string userId);
        IEnumerable<Photo> GetRanking();
        void UpdatePhoto(Photo photo);
    }
}