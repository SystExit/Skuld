namespace Skuld.Models.API
{
    public class Kitty
    {
        public string ImageURL { get; set; }
        public Kitty(string image)
        {
            this.ImageURL = image;
        }
    }
}
