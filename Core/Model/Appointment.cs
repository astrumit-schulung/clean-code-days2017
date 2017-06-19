using System;
using System.Collections.Generic;
using System.Net.Mail;

namespace Core.Model
{
    public class Appointment : EntityBase
    {
        public virtual List<Person> Attendees { get; set; }
        public virtual Person Organizer { get; set; }

        public virtual string Summary { get; set; }

        public virtual string Description { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }
        public virtual string Title { get; set; }
        public virtual string Location { get; set; }
    }
}