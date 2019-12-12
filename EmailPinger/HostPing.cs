using System.Net.NetworkInformation;
using System;
using System.Net.Mail;
using System.Configuration;

namespace EmailPinger
{
    public class HostPing
    {
        public static void Main(string[] args)
        {
            if (!PingHost(ConfigurationManager.AppSettings["Host"]))
            { 
                SendEmail();
            }
        }

        /// <summary>
        /// Method that will send an email 
        /// </summary>
        private static void SendEmail()
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient
                {
                    Host = ConfigurationManager.AppSettings["EmailHost"],
                    Port = int.Parse(ConfigurationManager.AppSettings["Host"]),
                    Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailAddress"], ConfigurationManager.AppSettings["EmailPassword"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = true
                };

                mail.From = new MailAddress(ConfigurationManager.AppSettings["EmailAddress"]);
                mail.To.Add(ConfigurationManager.AppSettings["EmailTo"]);
                mail.Subject = ConfigurationManager.AppSettings["Subject"];
                mail.Body = ConfigurationManager.AppSettings["Message"];
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.SubjectEncoding = System.Text.Encoding.Default;
                mail.IsBodyHtml = true;

                SmtpServer.Send(mail);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Method that will ping the host that was passed in.
        /// </summary>
        /// <param name="host">The host that we want to ping</param>
        /// <returns>True if the ping works; false otherwise.</returns>
        private static bool PingHost(string host)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(host);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                return false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
    }
}
