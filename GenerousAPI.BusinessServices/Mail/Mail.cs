using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GenerousAPI.BusinessServices
{
    public class Mail
    {

        /// <summary>
        /// Determines if the email was sent successfully or not
        /// </summary>
        public static bool EmailSentSuccessful = false;

        public static string NotificationBCC
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["NotificationBCC"]; }
        }

        /// <summary>
        /// Mail server details
        /// </summary>
        public static string MailServer
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["MailServer"]; }
        }

        /// <summary>
        /// Email from address for notifications
        /// </summary>
        public static string NotificationFromAddress
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["NotificationFromAddress"]; }
        }

        /// <summary>
        /// Email from address for errors
        /// </summary>
        public static string ErrorFromAddress
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["ErrorFromAddress"]; }
        }


        /// <summary>
        /// Email from address for registrations
        /// </summary>
        public static string RegistrationFromAddress
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["RegistrationFromAddress"]; }
        }

        /// <summary>
        /// Address to send errors to
        /// </summary>
        public static string ErrorToAddress
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["ErrorToAddress"]; }
        }

        /// <summary>
        /// Log file to send errors to
        /// </summary>
        public static string ErrorLogFile
        {
            get { return System.Configuration.ConfigurationManager.AppSettings["ErrorLogFile"]; }
        }


        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="message">Email message to send</param>
        /// <param name="setFooter">True if adding a footer, false otherwise</param>
        public static void SendMail(MailMessage message, bool setFooter = false)
        {
            try
            {
                if (String.IsNullOrEmpty(MailServer) || String.IsNullOrEmpty(NotificationFromAddress))
                    throw new ApplicationException("Mail server or notification from addess has not been configured.");

                if (message.From == null)
                    message.From = new MailAddress(NotificationFromAddress);

                message.IsBodyHtml = true;

                if (setFooter)
                    message.Body += "<p>Regards,</p><p>The Generous For Churches team<br /><a href=\"{1}\">{1}</a></p>";


                //if (!String.IsNullOrEmpty(Config.NotificationBCC))
                //    message.Bcc.Add(Config.NotificationBCC);


                SmtpClient client = new SmtpClient();
                client.Host = MailServer;
                client.Send(message);  //"commented for testing"
                EmailSentSuccessful = true;
            }
            catch (Exception ex)
            {
                LogSendMailException(message);
                LogException(ex);
            }
        }

        /// <summary>
        /// Logs the details of the email that failed to send
        /// </summary>
        /// <param name="messageToLog">Email details</param>
        public static void LogSendMailException(MailMessage emailMessageToLog)
        {
            try
            {
                var errorMessageToLog = new StringBuilder();

                errorMessageToLog.Append("FromAddress: " + emailMessageToLog.From + Environment.NewLine);
                errorMessageToLog.Append("ToAddress: " + emailMessageToLog.To + Environment.NewLine);
                errorMessageToLog.Append("Subject: " + emailMessageToLog.Subject + Environment.NewLine);
                errorMessageToLog.Append("Body: " + emailMessageToLog.Body + Environment.NewLine);

                System.IO.File.AppendAllText(DateTime.Now.ToString("yyyyMMddhhmmss") + "-"
                    + emailMessageToLog.To[0].Address + "-FailedEmail.log", errorMessageToLog.ToString());
            }
            catch (Exception ex)
            {
                LogException(ex);
            }
        }

        /// <summary>
        /// Build an error log based on exception detials
        /// </summary>
        /// <param name="ex">Exception details</param>
        /// <param name="sb">String builder of error details</param>
        /// <returns>Error string</returns>
        public static string BuildErrorLog(Exception ex, StringBuilder sb)
        {
            
            // Handle other types of errors in a generic fashion
            sb.AppendLine("Message: " + ex.Message);
            sb.AppendLine("Stack Trace: ");
            sb.AppendLine(ex.StackTrace);

            if (ex.InnerException != null)
            {
                sb.AppendLine("Inner Exception:");
                BuildErrorLog(ex.InnerException, sb);
            }

            return sb.ToString();

        }


        /// <summary>
        /// Logs an exception
        /// </summary>
        /// <param name="exception">Exception details</param>
        public static void LogException(Exception exception)
        {
            try
            {
                var errorMessage = new StringBuilder();

                BuildErrorLog(exception, errorMessage);


                System.IO.File.AppendAllText(ErrorLogFile, DateTime.Now + " " + errorMessage.ToString());

                System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage(ErrorFromAddress, ErrorToAddress);
                message.Body = errorMessage.ToString();


                message.Subject = String.Format("Error occurred on {0} - {1}", Environment.MachineName, exception.GetType().ToString());
                SendMail(message);
            }
            catch (Exception)
            {
                // don't stop the system from running just because there is an error logging - just continue on
            }
        }
    }
}
