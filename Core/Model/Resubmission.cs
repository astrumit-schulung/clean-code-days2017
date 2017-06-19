using System;

namespace Core.Model
{
    public class Resubmission : EntityBase
    {
        public virtual Person Owner { get; set; }
        public virtual string Subject { get; set; }
        public virtual string Content { get; set; }
        public virtual DateTime DueTime { get; set; }
        public virtual Person Person { get; set; }
        public virtual Project Project { get; set; }
        public virtual Company Company { get; set; }
        public virtual Resubmission Precursor { get; set; }
        public virtual bool IsActive { get; set; }
    }
}