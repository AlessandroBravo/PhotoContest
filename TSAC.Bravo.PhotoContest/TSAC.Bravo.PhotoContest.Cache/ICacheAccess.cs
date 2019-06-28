using System.Collections.Generic;
using System.Threading.Tasks;
using TSAC.Bravo.PhotoContest.Data.Models;

namespace TSAC.Bravo.PhotoContest.Cache
{
    public interface ICacheAccess
    {
        void AddVote(int photoId, int vote);
        Photo GetPhoto(int photoId);
        IEnumerable<Photo> GetPhotos();
        IEnumerable<Photo> GetRank();
        void InsertPhoto(Photo photo);
    }
}