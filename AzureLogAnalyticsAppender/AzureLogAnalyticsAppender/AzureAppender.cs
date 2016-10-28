using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using log4net.Appender;
using log4net.Core;
using Newtonsoft.Json;

namespace AzureLogAnalyticsAppender
{
    /// <summary>
    /// Azure log analytics appender.
    /// </summary>
    public class AzureAppender : AppenderSkeleton
    {
        /// <summary>
        /// The Azure instance id.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string CustomerId { get; set; }

        /// <summary>
        /// The shared secret/key.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string SharedKey { get; set; }

        /// <summary>
        /// The log type.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public string LogType { get; set; }

        /// <summary>
        /// Subclasses of <see cref="T:log4net.Appender.AppenderSkeleton" /> should implement this method
        /// to perform actual logging.
        /// </summary>
        /// <param name="loggingEvent">The event to append.</param>
        /// <remarks>
        /// <para>
        /// A subclass must implement this method to perform
        /// logging of the <paramref name="loggingEvent" />.
        /// </para>
        /// <para>This method will be called by <see cref="M:DoAppend(LoggingEvent)" />
        /// if all the conditions listed for that method are met.
        /// </para>
        /// <para>
        /// To restrict the logging of events in the appender
        /// override the <see cref="M:PreAppendCheck()" /> method.
        /// </para>
        /// </remarks>
        protected override void Append(LoggingEvent loggingEvent)
        {
            var serializableEvent = new AzureLoggingEvent(loggingEvent,
                () =>
                {
                    if (Layout != null)
                    {
                        using (StringWriter writer = new StringWriter())
                        {
                            Layout.Format(writer, loggingEvent);
                            return writer.ToString();
                        }
                    }
                    return loggingEvent.RenderedMessage;
                });

            string jsonMessage = JsonConvert.SerializeObject(serializableEvent);

            Post(jsonMessage);
        }

        private void Post(string json, string apiVersion = "2016-04-01")
        {
            string requestUriString = $"https://{CustomerId}.ods.opinsights.azure.com/api/logs?api-version={apiVersion}";
            DateTime dateTime = DateTime.UtcNow;
            string dateString = dateTime.ToString("r");
            string signature = GetSignature("POST", json.Length, "application/json", dateString, "/api/logs");
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);
            request.ContentType = "application/json";
            request.Method = "POST";
            request.Headers["Log-Type"] = LogType;
            request.Headers["x-ms-date"] = dateString;
            request.Headers["Authorization"] = signature;
            byte[] content = Encoding.UTF8.GetBytes(json);
            using (Stream requestStreamAsync = request.GetRequestStream())
            {
                requestStreamAsync.Write(content, 0, content.Length);
            }
            using (HttpWebResponse responseAsync = (HttpWebResponse)request.GetResponse())
            {
                if (responseAsync.StatusCode != HttpStatusCode.OK && responseAsync.StatusCode != HttpStatusCode.Accepted)
                {
                    Stream responseStream = responseAsync.GetResponseStream();
                    if (responseStream != null)
                    {
                        using (StreamReader streamReader = new StreamReader(responseStream))
                        {
                            throw new Exception(streamReader.ReadToEnd());
                        }
                    }
                }
            }
        }

        private string GetSignature(string method, int contentLength, string contentType, string date, string resource)
        {
            string message = $"{method}\n{contentLength}\n{contentType}\nx-ms-date:{date}\n{resource}";
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            using (HMACSHA256 encryptor = new HMACSHA256(Convert.FromBase64String(SharedKey)))
            {
                return $"SharedKey {CustomerId}:{Convert.ToBase64String(encryptor.ComputeHash(bytes))}";
            }
        }
    }
}
