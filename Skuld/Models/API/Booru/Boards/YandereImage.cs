namespace Skuld.Models.API.Booru
{
    public class YandereImage : GelbooruImage
	{
		public override string PostUrl { get { return "https://yande.re/post/show/" + ID; } }
	}
}
