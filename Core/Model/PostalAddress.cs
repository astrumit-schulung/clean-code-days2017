namespace Core.Model
{
    public class PostalAddress : EntityBase
    {
        public virtual string Street { get; set; }
        public virtual string City { get; set; }
        public virtual string Code { get; set; }
    }
}