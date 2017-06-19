using System;
using Core.Model;

namespace Core.Services
{
    public interface IDocumentRepository
    {
        Document Get(Guid id);

        void Save(Document document);

        void Delete(Document document);
    }
}
