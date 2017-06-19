using System;
using System.Runtime.Serialization;

namespace Core.Data
{
    public class BinaryFile
    {
        private string fileType;

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string FileType
        {
            get { return fileType; }
            set { fileType = string.IsNullOrWhiteSpace(value) ? "" : value.ToLower(); }
        }

        [DataMember]
        public byte[] FileContent { get; set; }

        [DataMember]
        public long GuestId { get; set; }

        public Guid Guid { get; }

        public BinaryFile()
        {
            Guid = Guid.NewGuid();
            fileType = "";
        }
    }
}