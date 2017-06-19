using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Core.Model;
using log4net;

namespace Core.Services
{
    public static class MailManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MailManager));

        public static MailMessage CreateMailMessageWithCalendarAttachment(Appointment meetingRequest)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = meetingRequest.Organizer.MailAddress;
            mailMessage.Subject = meetingRequest.Summary;
            mailMessage.Body = meetingRequest.Description;

            // Add body in iCalendar format
            string attendees = "";
            foreach (MailAddress attendee in meetingRequest.Attendees.Select(p => p.MailAddress))
            {
                attendees += "ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION;";
                attendees += "CN=\"" + attendee.DisplayName + "\":MAILTO:" + attendee.Address;
                attendees += "\r\n";
            }

            const string calendarDateFormat = "yyyyMMddTHHmmssZ";
            string bodyCalendar =
                "BEGIN:VCALENDAR\r\nMETHOD:PUBLISH\r\nPRODID:-//MUSTERMANN GmbH//{9}//EN\r\nVERSION:2.0\r\nBEGIN:VEVENT\r\nUID:{0}\r\nSTATUS:TENTATIVE\r\nSEQUENCE:1\r\nTRANSP:TRANSPARENT\r\nCREATED:{1}\r\nLAST-MODIFIED:{1}\r\nDTSTAMP:{1}\r\nDTSTART:{2}\r\nDTEND:{3}\r\nSUMMARY:{4}\r\nLOCATION:{5}\r\nDESCRIPTION:{6}\r\nORGANIZER;MAILTO:{7}\r\n{8}BEGIN:VALARM\r\nACTION:DISPLAY\r\nDESCRIPTION:REMINDER\r\nTRIGGER;RELATED=START:-PT00H15M00S\r\nEND:VALARM\r\nEND:VEVENT\r\nEND:VCALENDAR\r\n";
            bodyCalendar = string.Format(bodyCalendar,
                Guid.NewGuid().ToString("B"),
                DateTime.Now.ToUniversalTime().ToString(calendarDateFormat),
                meetingRequest.StartTime.ToUniversalTime().ToString(calendarDateFormat),
                meetingRequest.EndTime.ToUniversalTime().ToString(calendarDateFormat),
                meetingRequest.Title,
                meetingRequest.Location,
                meetingRequest.Description,
                meetingRequest.Organizer.MailAddress,
                attendees);

            ContentType contentTypeCalendar = new ContentType("text/calendar");
            contentTypeCalendar.Parameters.Add("method", "PUBLISH");
            contentTypeCalendar.Parameters.Add("charset", "utf-8");

            AlternateView viewCalendar = AlternateView.CreateAlternateViewFromString(bodyCalendar, contentTypeCalendar);
            viewCalendar.TransferEncoding = TransferEncoding.SevenBit;

            AddAttachment(
                mailMessage,
                Encoding.UTF8.GetBytes(bodyCalendar),
                "MeetingRequest.ics",
                new ContentType("text/calendar"));

            if (!mailMessage.AlternateViews.Contains(viewCalendar))
            {
                mailMessage.AlternateViews.Add(viewCalendar);
            }

            return mailMessage;
        }

        private static void AddAttachment(MailMessage mailMessage, byte[] content, string displayName,
            ContentType contentType)
        {
            var currentAttachment =
                new Attachment(new MemoryStream(content), contentType)
                {
                    ContentId = Guid.NewGuid().ToString(),
                    Name = displayName
                };
            mailMessage.Attachments.Add(currentAttachment);
        }

        public static void SendHtmlMail(string senderAddress, string recipientAddress, string subject, string body,
            bool sendAsync = true)
        {
            MailMessage mailMessage = new MailMessage(senderAddress, recipientAddress);
            mailMessage.Subject = subject;
            mailMessage.Priority = MailPriority.Normal;
            mailMessage.IsBodyHtml = true;
            mailMessage.Body = body;

            SendMail(mailMessage);
        }

        public static void SendMail(string senderAddress, string recipientAddress, string subject, string textMessage,
            MailPriority priority = MailPriority.Normal, bool sendAsync = true)
        {
            SendMail(senderAddress, recipientAddress, subject, textMessage, null, priority, sendAsync);
        }

        public static void SendMail(string senderAddress, string recipientAddress, string subject, string textMessage,
            List<MailAddress> replyToMailAddresses, MailPriority priority = MailPriority.Normal, bool sendAsync = true)
        {
            MailMessage mailMessage = new MailMessage(senderAddress, recipientAddress);
            mailMessage.Subject = subject;
            mailMessage.Priority = priority;
            mailMessage.Body = textMessage;

            AddReplyTo(mailMessage, replyToMailAddresses);

            SendMail(mailMessage);
        }

        public static void SendMail(MailMessage mailMessage)
        {
            if (mailMessage == null)
            {
                return;
            }

            SmtpClient smtpClient = null;

            try
            {
                smtpClient = InitializeSmtpServer();
                if (smtpClient == null)
                {
                    Log.DebugFormat("Mail not sent (smtpServer not set): '{0}'", mailMessage);
                    return;
                }

                using (smtpClient)
                {
                    smtpClient.Send(mailMessage);
                }
            }
            catch (SmtpFailedRecipientsException)
            {
                throw;
            }
            catch (SmtpException ex)
            {
                string exception = ex.StatusCode == SmtpStatusCode.MailboxUnavailable
                    ? string.Format("Mailbox {0} unavailables", mailMessage.To)
                    : string.Format("Mailserver {0} unavailable", AppSettings.Instance.MailSmtpServer);

                throw new Exception(exception, ex);
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Mail not sent: '{0}'", mailMessage), ex);
                Log.Error(GetSMTPClientConfiguration(smtpClient));
                throw;
            }
            finally
            {
                mailMessage.Dispose();
            }
            Log.DebugFormat("Mail sent: '{0}'", mailMessage);
        }

        private static void AddReplyTo(MailMessage mailMessage, IEnumerable<MailAddress> replyToMailAddresses)
        {
            if (replyToMailAddresses == null)
            {
                return;
            }
            foreach (MailAddress mailAddress in replyToMailAddresses)
            {
                mailMessage.ReplyToList.Add(mailAddress);
            }
        }

        private static SmtpClient InitializeSmtpServer()
        {
            string smtpServer = AppSettings.Instance.MailSmtpServer;
            if (string.IsNullOrWhiteSpace(smtpServer))
            {
                Log.WarnFormat("The smtp server is not configured.");
                return null;
            }

            SmtpClient smtpClient = new SmtpClient(smtpServer, AppSettings.Instance.MailSenderPort);

            string smtpUsername = AppSettings.Instance.MailSmtpUsername;
            string smtpPassword = AppSettings.Instance.MailSmtpPassword;

            if (string.IsNullOrWhiteSpace(smtpUsername) || string.IsNullOrWhiteSpace(smtpPassword))
            {
                smtpClient.Credentials = CredentialCache.DefaultNetworkCredentials;
                Log.DebugFormat("SMTP server: {0}, DefaultNetworkCredentials", smtpClient.Host);
            }
            else
            {
                smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                Log.DebugFormat("SMTP server: {0}, User name: {1}", smtpClient.Host, smtpUsername);
            }
            smtpClient.EnableSsl = AppSettings.Instance.MailEnableSsl;
            return smtpClient;
        }

        private static string GetSMTPClientConfiguration(SmtpClient smtpClient)
        {
            if (smtpClient == null)
            {
                return string.Empty;
            }

            if (smtpClient.Credentials is NetworkCredential)
            {
                NetworkCredential networkCredential = smtpClient.Credentials as NetworkCredential;
                return string.Format("NetworkCredential - UserName: '{0}', Password: '{1}'", networkCredential.UserName,
                    networkCredential.Password);
            }
            return string.Empty;
        }
    }
}