using System.Net.Mail;


namespace Fourth.Import.Exceptions
{
    public class ExceptionManager
    {
        public static void Publish(string message)
        {
            throw new System.Exception(message);
        }

        public static void Publish(string exceptionMessage, MailSetting emailSetting)
        {
            Publish("", exceptionMessage, emailSetting);
        }

        public static void Publish(string message, string exceptionMessage, MailSetting emailSetting)
        {
            MailMessage mail = new MailMessage
                                   {
                                       From = new MailAddress(emailSetting.FromEmail, emailSetting.Alias)
                                   };
            foreach (string toEmail in emailSetting.ToEmail)
            {
                mail.To.Add(toEmail);
            }
            mail.IsBodyHtml = true;
            mail.Subject = emailSetting.Subject;
            mail.Body = message + "<br/>" + exceptionMessage;
            mail.Priority = MailPriority.High;
            SmtpClient smtp = new SmtpClient();
            smtp.Send(mail);
        }
    }
}