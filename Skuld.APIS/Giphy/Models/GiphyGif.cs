namespace Skuld.APIS.Giphy.Models
{
    public class GiphyGif
    {
        public string ID { get; set; }

        public string Url
        {
            get
            {
                return "https://i.giphy.com/media/" + ID + "/giphy.gif";
            }
        }
    }
}