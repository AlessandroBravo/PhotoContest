using System.IO;
using System.Threading.Tasks;

namespace TSAC.Bravo.PhotoContest.Upload
{
    public interface IUploadHelper
    {
        Task UploadToS3(Stream stream, string fileName);
    }
}