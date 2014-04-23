using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NPoco;
using RTWin.Entities;

namespace RTWin.Services
{
    public class SyncService
    {
        private readonly DatabaseService _databaseService;
        private readonly DbSettingService _settings;
        private readonly User _user;
        private readonly Database _database;
        private readonly Uri _serverUri;

        private const string GET = @"GET";
        private const string POST = @"POST";

        private class Header
        {
            public const string SIGNATURE = @"X-Signature";
            public const string ACCESSKEY = @"X-AccessKey";
        }

        public SyncService(DbSettingService settings, User user, Database database)
        {
            _settings = settings;
            _user = user;
            _database = database;

            _serverUri = new Uri(_settings.Get<string>(DbSetting.Keys.ApiServer));
        }

        private static string SHA(string content)
        {
            SHA256Managed crypt = new SHA256Managed();
            string hash = String.Empty;
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(content), 0, Encoding.UTF8.GetByteCount(content));

            foreach (byte bit in crypto)
            {
                hash += bit.ToString("x2");
            }

            return hash;
        }

        private string CreateSignature(string content)
        {
            return SHA(string.Format("{0}{1}", content, _user.AccessSecret));
        }

        private string GetWebClient(BaseRest rest)
        {
            var json = JsonConvert.SerializeObject(rest, Formatting.None).Trim();
            var signature = CreateSignature(json);

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                wc.Headers.Add(Header.SIGNATURE, signature);
                wc.Headers.Add(Header.ACCESSKEY, _user.AccessKey);
                wc.Encoding = Encoding.UTF8;

                if (rest.Verb == POST)
                {
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    var responseBytes = wc.UploadData(rest.Uri, rest.Verb, data);
                    return Encoding.UTF8.GetString(responseBytes);
                }

                throw new Exception("Unsupported verb");
            }
        }

        public DateTime? GetLastSync()
        {
            var requestUri = new Uri(_serverUri, "v1/api/lastsync");

            var sync = new BaseRest(_user.AccessKey)
            {
                Verb = POST,
                Uri = requestUri.ToString(),
            };

            var response = GetWebClient(sync);
            var result = JsonConvert.DeserializeObject<dynamic>(response);
            DateTime? lastSync = result.lastSync;
            return lastSync;
        }
    }
}
