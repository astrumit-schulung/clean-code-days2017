

using System;
using System.Collections.Generic;
using Core.Facades;
using Core.Model;
using Core.Services;
using NHibernate;
using NSubstitute;
using NUnit.Framework;

namespace Core.Test
{
    [TestFixture]
    public class CRMControllerTest
    {
        [Test]
        public void DeletePerson_does_not_throw()
        {
            var database = Substitute.For<IDatabase>();
            database.CurrentSession.Returns(Substitute.For<ISession>());

            var sut = new CRMController(Substitute.For<IResubmissionService>(),
                database, Substitute.For<Action<Person>>());

            var person = new Person
            {
                Documents = new List<Document>(),
                CustomFields = new Dictionary<string, string>()
            };
            sut.DeletePerson(person);
        }

        [Test]
        public void DeletePerson_deletes_documents()
        {
            var database = Substitute.For<IDatabase>();
            database.CurrentSession.Returns(Substitute.For<ISession>());

            var deleteDocuments = Substitute.For<Action<Person>>();
            var sut = new CRMController(Substitute.For<IResubmissionService>(),
                database, deleteDocuments);

            var person = new Person
            {
                Documents = new List<Document>(),
                CustomFields = new Dictionary<string, string>()
            };
            sut.DeletePerson(person);

            deleteDocuments.Received().Invoke(person);
        }


    }
}

