namespace Skuld.Models.API
{
    public class Gif
    {
        public string Url { get; private set; }
        public Gif(string id)
        {
            Url = "https://i.giphy.com/media/"+id+"/giphy.gif";
        }
    }
}
