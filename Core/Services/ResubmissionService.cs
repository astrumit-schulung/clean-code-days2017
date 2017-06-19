using System.Linq;
using Core.Model;
using NHibernate.Linq;

namespace Core.Services
{
    public static class ResubmissionService
    {
        public static void DeleteResubmissions(Person person)
        {
            var currentSession = ServiceLocator.Instance.GetService<IDatabase>().CurrentSession;

            currentSession.Query<Resubmission>()
                .Where(res => res.Owner == person || res.Person == person)
                .ToList()
                .ForEach(res => currentSession.Delete(res));
        }
    }
}
