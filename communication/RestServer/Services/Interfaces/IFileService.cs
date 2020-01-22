
namespace Communication.RestServer.Services.Interfaces
{
    interface IFileService
    {
        byte[] GetBinary(string FileID, string username);
        void UploadFile(string FileID, byte[] source, string username, bool sharing);
    }
}
