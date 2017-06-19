using Core.Model;
using NHibernate;

namespace Core.Services
{
    public interface IDatabase
    {
        ISession CurrentSession { get; }
    }
}