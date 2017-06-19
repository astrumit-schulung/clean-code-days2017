namespace Core.Model
{
    public class Document : EntityBase
    {
        public virtual byte[] OriginalFile { get; set; }
        public virtual string FileType { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Descriptor { get; set; }
        public virtual string FilePath { get; set; }
    }
}
