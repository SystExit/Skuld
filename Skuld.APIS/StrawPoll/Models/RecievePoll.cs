using Newtonsoft.Json;

namespace Skuld.APIS.StrawPoll.Models
{
    public class RecievePoll
    {
        public int ID { get; }
        public string Title { get; }
        public bool Multi { get; }
        public string[] Options { get; }
        public int[] Votes { get; }
        public string Dupcheck { get; }
        public bool Captcha { get; }

        [JsonIgnore]
        public string Url
        {
            get
            {
                return "http://www.strawpoll.me/" + ID;
            }
        }
    }
}