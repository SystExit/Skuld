namespace Skuld.Models.API
{
    public class DoggoImage
    {
        public string ImageURL { get; private set; }
        public DoggoImage(string image)
        {
            this.ImageURL = "https://random.dog/" + image;
        }
    }
}
