using System.Collections.Generic;

namespace Core.Model
{
    public class Company : EntityBase
    {
        public virtual List<Person> Employees { get; set; }
        public virtual List<PostalAddress> Addresses { get; set; }
    }
}