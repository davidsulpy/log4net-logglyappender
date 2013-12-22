using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using log4net.Appender;
using log4net.Core;

namespace Log4Net.LogglyAppender
{
    public sealed class LogglyLog4NetAppenderWebRequest : AppenderSkeleton
    {
        public bool Synchronous { get; set; }
        public string InputKey { get; set; }
        public string LogUrl { get; set; }
        public string Environment { get; set; }
        public string Application { get; set; }
        public string Role { get; set; }
        public int TimeoutInSeconds { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            try
            {
                string logMessage = RenderLoggingEvent(loggingEvent);

                if (Synchronous)
                {
                    SendMessageRequest(logMessage);
                }
                else
                {
                    var thread = new Thread(SendMessage);
                    thread.IsBackground = true;
                    thread.Start(logMessage);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private void SendMessageRequest(string message)
        {
            try
            {
                string url = string.Format("{0}{1}/tag/{2},{3},{4}", LogUrl, InputKey, Application, Environment, Role);

                Trace.WriteLine(url);
                Trace.WriteLine(message);

                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                byte[] contentBytes = Encoding.UTF8.GetBytes(message);
                request.ContentLength = contentBytes.Length;

                Stream stream = request.GetRequestStream();

                stream.Write(contentBytes, 0, contentBytes.Length);
                stream.Flush();
                stream.Flush();

                using (request.GetResponse())
                {
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }

        private void SendMessage(object message)
        {
            try
            {
                SendMessageRequest(message as string);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}