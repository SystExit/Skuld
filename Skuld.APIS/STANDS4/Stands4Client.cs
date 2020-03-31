using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Skuld.APIS.STANDS4.Models;
using Skuld.Core.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Skuld.APIS
{
    public class Stands4Client
    {
        private readonly RateLimiter rateLimiter;
        private readonly int stands4userid;
        private readonly string stands4usertoken;

        public Stands4Client(int userid, string token)
        {
            rateLimiter = new RateLimiter();
            stands4userid = userid;
            stands4usertoken = token;
        }

        public async Task<Define> GetWordAsync(string word)
        {
            if (rateLimiter.IsRatelimited()) return null;

            var stringifiedxml = await HttpWebClient.ReturnStringAsync(new Uri($"http://www.stands4.com/services/v2/defs.php?uid={stands4userid}&tokenid={stands4usertoken}&word={word}"));
            var xml = new XmlDocument();

            xml.LoadXml(stringifiedxml);
            XObject xNode = XDocument.Parse(xml.InnerXml);
            var jobject = JObject.Parse(JsonConvert.SerializeXNode(xNode));
            dynamic item = jobject["results"]["result"].First();

            return new Define
            {
                Word = word,
                Definition = item["definition"].ToString(),
                Example = item["example"].ToString(),
                PartOfSpeech = item["partofspeech"].ToString(),
                Terms = item["term"].ToString()
            };
        }
    }
}