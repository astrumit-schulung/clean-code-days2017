namespace Core.Model
{
    public class Communication : EntityBase
    {
        public virtual CommunicationType Type { get; set; }
        public string Value { get; set; }
    }
}