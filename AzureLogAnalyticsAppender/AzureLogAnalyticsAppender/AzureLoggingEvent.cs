using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using log4net.Core;
using log4net.Util;

namespace AzureLogAnalyticsAppender
{
    /// <summary>
    /// A serializable log4net event.
    /// </summary>
    [DataContract]
    public class AzureLoggingEvent
    {
        /// <summary>
        /// Creates the event from <paramref name="otherEvent"/>.
        /// </summary>
        /// <param name="otherEvent">The base log4net event.</param>
        /// <param name="formatMessage">Function to format the message.</param>
        public AzureLoggingEvent(LoggingEvent otherEvent, Func<string> formatMessage)
            : this (formatMessage, otherEvent.Domain, otherEvent.ExceptionObject, otherEvent.Identity, otherEvent.Level, otherEvent.LoggerName, otherEvent.Properties, otherEvent.RenderedMessage, otherEvent.ThreadName, otherEvent.TimeStamp, otherEvent.UserName)
        {
        }

        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="formatMessage">Function to format the message.</param>
        /// <param name="domain">Gets the AppDomain friendly name.</param>
        /// <param name="exceptionObject">Gets the exception object used to initialize this event.</param>
        /// <param name="identity">Gets the identity of the current thread principal.</param>
        /// <param name="level">Gets the log4net Level of the logging event.</param>
        /// <param name="loggerName">Gets the name of the logger that logged the event.</param>
        /// <param name="properties">Additional event specific properties.</param>
        /// <param name="message">Gets the message, rendered through the log4net.Repository.ILoggerRepository.RendererMap.</param>
        /// <param name="threadName">Gets the name of the current thread.</param>
        /// <param name="timestamp">Gets the time of the logging event.</param>
        /// <param name="userName">Gets the name of the current user.</param>
        public AzureLoggingEvent(Func<string> formatMessage, string domain, Exception exceptionObject, string identity, Level level, string loggerName, PropertiesDictionary properties, string message, string threadName, DateTime timestamp, string userName)
        {
            Domain = domain;
            ExceptionObject = exceptionObject;
            Identity = identity;
            LoggerName = loggerName;
            Properties = new Dictionary<string, object>();
            Message = formatMessage != null ? formatMessage() : message;
            ThreadName = threadName;
            Timestamp = timestamp;
            UserName = userName;
            LevelValue = level.Value;
            LevelName = level.Name;

            string[] propertyKeys = properties.GetKeys();
            foreach (string propertyKey in propertyKeys)
            {
                Properties[propertyKey] = properties[propertyKey];
            }
        }

        /// <summary>
        /// Gets the AppDomain friendly name.
        /// </summary>
        [DataMember]
        public string Domain { get; }

        /// <summary>
        /// Gets the exception object used to initialize this event.
        /// </summary>
        [DataMember]
        public Exception ExceptionObject { get; }

        /// <summary>
        /// Gets the identity of the current thread principal.
        /// </summary>
        [DataMember]
        public string Identity { get; }

        /// <summary>
        /// Gets the log4net Level of the logging event as integer.
        /// </summary>
        [DataMember]
        public int LevelValue { get; }

        /// <summary>
        /// Gets the log4net Level of the logging event as string.
        /// </summary>
        [DataMember]
        public string LevelName { get; }

        /// <summary>
        /// Gets the name of the logger that logged the event.
        /// </summary>
        [DataMember]
        public string LoggerName { get; }

        /// <summary>
        /// Additional event specific properties.
        /// </summary>
        [DataMember]
        public IDictionary<string, object> Properties { get; set; }

        /// <summary>
        /// Gets the message, rendered through the log4net.Repository.ILoggerRepository.RendererMap.
        /// </summary>
        [DataMember]
        public string Message { get; }

        /// <summary>
        /// Gets the name of the current thread.
        /// </summary>
        [DataMember]
        public string ThreadName { get; }

        /// <summary>
        /// Gets the time of the logging event.
        /// </summary>
        [DataMember]
        public DateTime Timestamp { get; }

        /// <summary>
        /// Gets the name of the current user.
        /// </summary>
        [DataMember]
        public string UserName { get; }
    }
}