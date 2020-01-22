using Communication.RestServer.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;

namespace Communication.RestServer.Services
{
    public class FileService : IFileService
    {
        //TODO: use database implement
        Dictionary<string, Tuple<string, bool>> fileIndex = new Dictionary<string, Tuple<string, bool>>();

        private byte[] Compile(byte[] source)
        {
            return source;
        }

        public byte[] GetBinary(string FileID, string username)
        {
            if (!fileIndex.TryGetValue(FileID, out Tuple<string, bool> fileInfo)) return null;
            if (username != fileInfo.Item1 && !fileInfo.Item2) return null;
            using var fs = new FileStream(FileID, FileMode.Open);
            byte[] raw = new byte[fs.Length];
            fs.Read(raw, 0, raw.Length);
            return raw;
        }

        public void UploadFile(string FileID, byte[] source, string username, bool sharing)
        {
            byte[] binary = Compile(source);
            using var fs = new FileStream(FileID, FileMode.Create);
            fs.Write(binary, 0, binary.Length);
            fileIndex[FileID] = new Tuple<string, bool>(username, sharing);
        }
    }
}
