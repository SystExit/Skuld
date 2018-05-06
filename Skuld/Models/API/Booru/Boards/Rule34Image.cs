namespace Skuld.Models.API.Booru
{
    public class Rule34Image : SafebooruImage
	{
		public override string ImageUrl
			=> "https://rule34.xxx/images/" + Directory + "/" + Image;
		public override string PostUrl
			=> "https://rule34.xxx/index.php?page=post&s=view&id=" + ID;
	}
}
