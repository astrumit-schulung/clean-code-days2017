using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Core
{
    public static class Utilities
    {
        public static void ThrowIfNull(object obj, string argumentName)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }

        public static string CombineBaseDirectoryWithVirtualPath(string virtualPath)
        {
            virtualPath = virtualPath.Replace("~/", "").Replace("/", @"\");
            virtualPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, virtualPath);
            virtualPath = virtualPath.Replace("Core\\Documents", "CRMS\\Documents");
            return virtualPath;
        }

        public static void Create(string directoryPath)
        {
            DirectoryInfo info = new DirectoryInfo(directoryPath);
            if (!info.Exists)
            {
                info.Create();
            }
        }

        public static string ConvertToValidFileName(string fileName)
        {
            fileName = Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c, '_'));
            fileName = fileName.Replace(' ', '_');
            fileName = Regex.Replace(fileName, "_+", "_");
            return fileName;
        }
    }
}