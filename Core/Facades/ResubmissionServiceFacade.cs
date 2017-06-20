using Core.Model;
using Core.Services;

namespace Core.Facades
{
    public interface IResubmissionService
    {
        void DeleteResubmissions(Person person);
    }

    public class ResubmissionServiceFacade : IResubmissionService
    {
        public void DeleteResubmissions(Person person)
        {
            ResubmissionService.DeleteResubmissions(person);
        }
    }
}