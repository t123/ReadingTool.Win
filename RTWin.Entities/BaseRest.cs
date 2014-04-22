using System;

namespace RTWin.Entities
{
    public class BaseRest
    {
        public string Nonce { get; private set; }

        public string Date
        {
            get { return DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); }
        }

        public string AccessKey { get; private set; }
        public string Verb { get; set; }
        public string Uri { get; set; }

        public BaseRest(string accessKey)
        {
            AccessKey = accessKey;
            Nonce = Guid.NewGuid().ToString();
        }
    }
}
