using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Core.Model
{
    public class Person : EntityBase
    {
        public virtual List<PostalAddress> Addresses { get; set; }

        public virtual List<Communication> Communications { get; set; }

        public virtual List<Document> Documents { get; set; }

        public virtual string FirstName { get; set; }

        public virtual string LastName { get; set; }

        public virtual DateTime DateOfBirth { get; set; }

        public virtual MailAddress MailAddress
        {
            get
            {
                return Communications.Where(c => c.Type == CommunicationType.EMail)
                    .Select(c => new MailAddress(c.Value)).FirstOrDefault();
            }
        }

        public virtual IDictionary<string,string> CustomFields { get; set; }
    }
}