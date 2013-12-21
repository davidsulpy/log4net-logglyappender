using log4net.Appender;
using log4net.Core;
using RestSharp;
using RestSharp.Serializers;

namespace Log4Net.LogglyAppender
{
    public class LogglyLog4NetAppender : AppenderSkeleton
    {
        public string InputKey { get; set; }
        public string LogUrl { get; set; }
        public string Environment { get; set; }
        public string Application { get; set; }
        public string Role { get; set; }
        public int TimeoutInSeconds { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            string logMessage = RenderLoggingEvent(loggingEvent);

            Send(logMessage);
        }

        public virtual void Send(string message)
        {
            string baseUrl = string.Format("{0}{1}/tag/{2},{3},{4}", LogUrl, InputKey, Application, Environment, Role);
            var client = new RestClient(baseUrl);
            client.Timeout = TimeoutInSeconds*1000;

            var request = new RestRequest("", Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = new JsonNonSerializer();
            request.AddBody(message);

            client.ExecuteAsync(request, response => { });
        }

        /// <summary>
        ///     This is a trick to prevent RestSharp from trying to be nice and serialize when it doesn't have to
        /// </summary>
        private class JsonNonSerializer : ISerializer
        {
            public string Serialize(object obj)
            {
                return obj.ToString();
            }

            public string RootElement { get; set; }
            public string Namespace { get; set; }
            public string DateFormat { get; set; }
            public string ContentType { get; set; }
        }
    }
}