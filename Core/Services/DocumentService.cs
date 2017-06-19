using System;
using System.IO;
using Core.Data;
using Core.Model;

namespace Core.Services
{
    public static class DocumentService
    {
        public static Document GetDocumentById(Guid id)
        {
            return ServiceLocator.Instance.GetService<IDocumentRepository>().Get(id);
        }

        public static void DeleteDocuments(Person person)
        {
            foreach (var document in person.Documents)
            {
                DeleteFileFromFileSystem(document.FilePath);
                ServiceLocator.Instance.GetService<IDocumentRepository>().Delete(document);
            }
        }

        public static Guid Save(byte[] content, string contentType, DocumentType documentType)
        {
            Document document = new Document();
            document.OriginalFile = content;
            document.FileType = contentType;
            document.FileName = string.Format("{0}_{1}.{2}", documentType, DateTime.Now.ToString("ddMMyyHHmmss"),
                contentType.ToLower());
            document.Descriptor = documentType.ToString();

            ServiceLocator.Instance.GetService<IDocumentRepository>().Save(document);

            return document.Id;
        }

        public static string SaveToFileSystem(byte[] content, string virtualPath, Document document)
        {
            string virtualFilePath = GetVirtualFilePath(virtualPath, document);

            try
            {
                // Delete file before saving and/or delete old file name
                DeleteFileFromFileSystem(document.FilePath);
                DeleteFileFromFileSystem(virtualFilePath);

                string path = Utilities.CombineBaseDirectoryWithVirtualPath(virtualFilePath);
                Utilities.Create(Path.GetDirectoryName(path));

                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(fileStream))
                    {
                        writer.Write(content);
                    }
                }

                return virtualFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error saving document '{0}' to file system.", virtualFilePath), ex);
            }
        }

        public static void DeleteFromFileSystem(Document document)
        {
            if (!string.IsNullOrWhiteSpace(document.FilePath))
            {
                try
                {
                    DeleteFileFromFileSystem(document.FilePath);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error deleting document '{0}'.", document.FilePath), ex);
                }
            }
        }

        public static byte[] GetFileContent(Document document)
        {
            if (document != null)
            {
                if (document.OriginalFile != null)
                {
                    return document.OriginalFile;
                }
                return GetFileContentFromFileSystem(document);
            }

            return null;
        }

        public static string GetFileUrl(Document document)
        {
            return document == null ? "" : GetFileUrl(document.FilePath);
        }

        private static string GetVirtualFilePath(string virtualPath, Document document)
        {
            return string.Format("{0}/{1}.{2}", virtualPath,
                Utilities.ConvertToValidFileName(document.Descriptor), document.FileType.ToLower());
        }

        private static void DeleteFileFromFileSystem(string virtualFilePath)
        {
            if (!string.IsNullOrWhiteSpace(virtualFilePath))
            {
                string physicalFilePath = Utilities.CombineBaseDirectoryWithVirtualPath(virtualFilePath);
                if (File.Exists(physicalFilePath))
                {
                    File.Delete(physicalFilePath);
                }
            }
        }

        private static byte[] GetFileContentFromFileSystem(Document document)
        {
            if (!string.IsNullOrWhiteSpace(document.FilePath))
            {
                try
                {
                    string physicalFilePath = Utilities.CombineBaseDirectoryWithVirtualPath(document.FilePath);

                    if (File.Exists(physicalFilePath))
                    {
                        using (FileStream fileStream = new FileStream(physicalFilePath, FileMode.Open, FileAccess.Read))
                        {
                            using (BinaryReader reader = new BinaryReader(fileStream))
                            {
                                return reader.ReadBytes((int) fileStream.Length);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error reading file content from file '{0}'.", document.FilePath),
                        ex);
                }
            }

            return null;
        }

        private static string GetFileUrl(string virtualFilePath)
        {
            // Always returns WebInterface-Url either the request comes from WebInterface or WebService

            string webInterfaceUrl;

            try
            {
                // WebService has WebInterface-Url in AppSettings
                webInterfaceUrl = AppSettings.Instance.WebInterfaceUrl;
            }
            catch
            {
                webInterfaceUrl = "localhost:9000/CRM";
            }

            return string.Format("{0}/{1}", webInterfaceUrl, virtualFilePath.Replace("~", ""));
        }

        public static Document GetDocumentFromBinaryFile(BinaryFile binaryFile)
        {
            string fileName = binaryFile.FileName.Contains(".")
                ? binaryFile.FileName
                : string.Format("{0}.{1}", binaryFile.FileName, binaryFile.FileType.ToLower());

            Document document = new Document();
            document.FileName = fileName;
            document.Descriptor = fileName;
            document.FileType = binaryFile.FileType;
            document.OriginalFile = binaryFile.FileContent;
            return document;
        }
    }
}