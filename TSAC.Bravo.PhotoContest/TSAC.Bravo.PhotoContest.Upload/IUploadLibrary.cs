using System.IO;
using System.Threading.Tasks;

namespace TSAC.Bravo.PhotoContest.Upload
{
    public interface IUploadLibrary
    {
        Task Upload(Stream stream, string fileName);
        string GetCdn();
    }
}