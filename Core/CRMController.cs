using System;
using System.Collections.Generic;
using Core.Data;
using Core.Model;
using Core.Services;

namespace Core
{
    public class CRMController
    {
        public virtual void DeletePerson(Person person)
        {
            Utilities.ThrowIfNull(person, "person");

            ResubmissionService.DeleteResubmissions(person);
            DocumentService.DeleteDocuments(person);
            ServiceLocator.Instance.GetService<IDatabase>().CurrentSession.Delete(person);
            if (person.CustomFields.ContainsKey("need-notification"))
            {
                var temp = ServiceLocator.Instance.GetService<ITemplateService>()
                    .Process("delete-notification", person);
                MailManager.SendMail(temp);
            }
        }

        public virtual Person CreatePerson(string firstName, string lastName)
        {
            return new Person
            {
                FirstName = firstName,
                LastName = lastName,
                Addresses = new List<PostalAddress>(),
                Communications = new List<Communication>(),
                Documents = new List<Document>(),
                CustomFields = new Dictionary<string, string>()
            };
        }

        public virtual Appointment CreateAppointment(AppointmentData data)
        {
            return new Appointment
            {
                Attendees = new List<Person>(),
                Summary = data.Summary,
                Description = data.Description,
                Location = data.Location,
                StartTime = data.StartTime,
                EndTime = data.EndTime
            };
        }

        public virtual Project CreateProject(string name)
        {
            return new Project
            {
                StartTime = DateTime.Now,
                State = ProjectState.Started,
                Participants = new List<Person>(),
                Name = name
            };
        }

        public virtual void FinishProject(Project project)
        {
            project.State = ProjectState.Finished;
            if (!project.EndTime.HasValue)
            {
                project.EndTime = DateTime.Now;
            }
            foreach (var participant in project.Participants)
            {
                if (participant.CustomFields.ContainsKey("need-project-notifications"))
                {
                    var temp = ServiceLocator.Instance.GetService<ITemplateService>()
                        .Process("project-finished", project, participant);
                    MailManager.SendMail(temp);
                }
            }
            ServiceLocator.Instance.GetService<IDatabase>().CurrentSession.Save(project);
        }

        public virtual void AddDocument(Person person, BinaryFile binaryFile)
        {
            person.Documents.Add(DocumentService.GetDocumentFromBinaryFile(binaryFile));
            ServiceLocator.Instance.GetService<IDatabase>().CurrentSession.SaveOrUpdate(person);
        }

        public virtual void AddDocument(Project project, BinaryFile binaryFile)
        {
            if (project.State == ProjectState.Finished ||
                project.EndTime.HasValue && project.EndTime.Value < DateTime.Now)
            {
                throw new ArgumentException("project is already finished", "project");
            }
            project.Documents.Add(DocumentService.GetDocumentFromBinaryFile(binaryFile));
            ServiceLocator.Instance.GetService<IDatabase>().CurrentSession.SaveOrUpdate(project);

            foreach (var participant in project.Participants)
            {
                if (participant.CustomFields.ContainsKey("need-project-notifications"))
                {
                    var temp = ServiceLocator.Instance.GetService<ITemplateService>()
                        .Process("document-added", project, participant);
                    MailManager.SendMail(temp);
                }
            }
        }

        public virtual void RemoveDocument(Project project, Document document)
        {
            if (project.State == ProjectState.Finished)
            {
                throw new ArgumentException("project is already finished", "project");
            }
            project.Documents.Remove(document);

            foreach (var participant in project.Participants)
            {
                if (participant.CustomFields.ContainsKey("need-project-notifications"))
                {
                    var temp = ServiceLocator.Instance.GetService<ITemplateService>()
                        .Process("document-removed", project, participant);
                    MailManager.SendMail(temp);
                }
            }
        }

        public virtual Resubmission RePresent(Resubmission resubmission, DateTime dueTime)
        {
            Utilities.ThrowIfNull(resubmission, "resubmission");
            resubmission.IsActive = false;

            var newResubmission = new Resubmission
            {
                DueTime = dueTime,
                Person = resubmission.Person,
                Company = resubmission.Company,
                Project = resubmission.Project,
                Owner = resubmission.Owner,
                Subject = resubmission.Subject
            };

            var message = ServiceLocator.Instance.GetService<ITemplateService>()
                .Process("schedule-resubmission", newResubmission);
            MailManager.SendMail(message);

            return newResubmission;
        }

        public virtual Resubmission RePresent(Person owner, Project project, string subject, DateTime dueTime)
        {
            Utilities.ThrowIfNull(owner, "owner");

            var newResubmission = new Resubmission
            {
                DueTime = dueTime,
                Project = project,
                Owner = owner,
                Subject = subject
            };

            var message = ServiceLocator.Instance.GetService<ITemplateService>()
                .Process("schedule-resubmission", newResubmission);
            MailManager.SendMail(message);

            return newResubmission;
        }

        public virtual Resubmission RePresent(Person owner, Company company, string subject, DateTime dueTime)
        {
            Utilities.ThrowIfNull(owner, "owner");
            Utilities.ThrowIfNull(company, "company");

            var newResubmission = new Resubmission
            {
                DueTime = dueTime,
                Company = company,
                Owner = owner,
                Subject = subject
            };

            var message = ServiceLocator.Instance.GetService<ITemplateService>()
                .Process("schedule-resubmission", newResubmission);
            MailManager.SendMail(message);

            return newResubmission;
        }

        public virtual Resubmission RePresent(Person owner, Person person, string subject, DateTime dueTime)
        {
            var newResubmission = new Resubmission
            {
                DueTime = dueTime,
                Person = person,
                Owner = owner,
                Subject = subject
            };

            var message = ServiceLocator.Instance.GetService<ITemplateService>()
                .Process("schedule-resubmission", newResubmission);
            MailManager.SendMail(message);

            return newResubmission;
        }

        public virtual void ScheduleMeeting(Person person, List<Person> attendees, Appointment appointment)
        {
            appointment.Organizer = person;
            appointment.Attendees.AddRange(attendees);
            SendEmail(appointment);
        }

        private static void SendEmail(Appointment appointment)
        {
            var msg = MailManager.CreateMailMessageWithCalendarAttachment(appointment);
            MailManager.SendMail(msg);
        }
    }
}