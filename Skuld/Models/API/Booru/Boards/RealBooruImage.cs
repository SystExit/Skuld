namespace Skuld.Models.API.Booru
{
    public class RealbooruImage : SafebooruImage
	{
		public override string ImageUrl
			=> "https://realbooru.com/images/" + Directory + "/" + Image;
		public override string PostUrl
			=> "https://realbooru.com/index.php?page=post&s=view&id=" + ID;
	}
}
