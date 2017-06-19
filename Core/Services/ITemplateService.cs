using System.Net.Mail;
using Core.Model;

namespace Core.Services
{
    public interface ITemplateService
    {
        MailMessage Process(string templateId, Person person);
        MailMessage Process(string templateId, Project project, Person person);
        MailMessage Process(string templateId, Resubmission resubmission);
    }
}