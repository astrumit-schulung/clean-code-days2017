using NHibernate;

namespace Core.Services
{
    public class NHibernateGate : IDatabase
    {
        private readonly ISessionFactory sessionFactory;

        public NHibernateGate(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public ISession CurrentSession => sessionFactory.GetCurrentSession();
    }
}
