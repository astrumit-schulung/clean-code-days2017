using System;
using System.Collections.Generic;

namespace Core.Model
{
    public class Project : EntityBase
    {
        public virtual string Name { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime? EndTime { get; set; }
        public virtual List<Document> Documents { get; set; }
        public virtual List<Person> Participants { get; set; }
        public virtual ProjectState State { get; set; }
    }
}