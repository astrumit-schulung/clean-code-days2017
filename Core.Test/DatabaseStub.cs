using System;
using Core.Services;
using NHibernate;
using StructureMap;

namespace Core.Test
{
    public class DatabaseStub : IDatabase
    {
        public ISession CurrentSession
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class TestRegistry : Registry
    {
        public TestRegistry()
        {
            ForSingletonOf<IDatabase>().Use<DatabaseStub>();
        }
    }
}