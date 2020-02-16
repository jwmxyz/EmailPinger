using System.Net.NetworkInformation;
using System;
using System.Net.Mail;
using System.Configuration;
using EmailPinger.Models;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EmailPinger
{
    public class HostPing
    {
        private static readonly string FilePath = $@"{Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)}\results.wiz";
        private static readonly HttpClient _client = new HttpClient();

        public static void Main()
        {
            //if the CheckApi is a bool and true then check the API.
            if (bool.TryParse(ConfigurationManager.AppSettings["checkApi"], out bool checkApi))
            {
                if (checkApi)
                {
                    var response = _client.GetAsync(ConfigurationManager.AppSettings["apiCheckUrl"]).Result;
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.Content.ReadAsStringAsync().Result);
                        if (result.ContainsKey("value"))
                        {
                            if (bool.TryParse(result["value"], out bool returnedBool))
                            {
                                if (!returnedBool)
                                {
                                    Environment.Exit(0);
                                }
                            }
                        }
                    }
                }
            }

            var pingResult = PingHost(ConfigurationManager.AppSettings["Host"]);
            // Send an email if the host could not be pinged.
            if (!pingResult.Success)
            {
                SendEmail();
            }
        }

        /// <summary>
        /// Used to write the error message to file
        /// </summary>
        /// <param name="message">the message that we want to send to file.</param>
        private static void WriteToFile(string message)
        {
            File.AppendAllLines(FilePath, new[] { message });
        }

        /// <summary>
        /// Used to write the ping result to file.
        /// </summary>
        /// <param name="result">The result object that we want to send to file.</param>
        private static void WriteToFile(PingResult result)
        {
            File.AppendAllLines(FilePath, new[] { result.ToString() });
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
                    Port = int.Parse(ConfigurationManager.AppSettings["Port"]),
                    Credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailAddress"], ConfigurationManager.AppSettings["EmailPassword"]),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = true
                };

                mail.From = new MailAddress(ConfigurationManager.AppSettings["EmailAddress"]);
                mail.To.Add(ConfigurationManager.AppSettings["EmailTo"]);
                mail.Subject = ConfigurationManager.AppSettings["Subject"];
                mail.Body = string.Format(ConfigurationManager.AppSettings["Message"], ConfigurationManager.AppSettings["Host"]);
                mail.BodyEncoding = System.Text.Encoding.UTF8;
                mail.SubjectEncoding = System.Text.Encoding.Default;
                mail.IsBodyHtml = true;

                SmtpServer.Send(mail);
            }
            catch (Exception e)
            {
                WriteToFile($@"{{ Error: { e.ToString() } }},");
            }
        }

        /// <summary>
        /// Method that will ping the host that was passed in.
        /// </summary>
        /// <param name="host">The host that we want to ping</param>
        /// <returns>True if the ping works; false otherwise.</returns>
        private static PingResult PingHost(string host)
        {
            Ping pinger = null;
            PingResult pingResult = null;
            try
            {
                pinger = new Ping();
                //replace new Uri(host).Host with "host" if pinging an ip address.
                PingReply reply = pinger.Send(new Uri(host).Host);
                pingResult = new PingResult(reply.Status == IPStatus.Success, reply.RoundtripTime);
            }
            catch (PingException e)
            {
                return new PingResult(false, 0);
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingResult;
        }
    }
}
