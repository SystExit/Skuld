namespace Skuld.Models.API.Strawpoll
{
	public class SendPoll
	{
        private string Title;
        private bool Multi;
        private string[] Options;

        public SendPoll(string title, string[] options)
        {
            this.Title = title;
            this.Options = options;
            this.Multi = false;
        }

        public string title
        {
            get
            {
                return Title;
            }
        }
        public bool multi
        {
            get
            {
                return Multi;
            }
        }
        public string[] options
        {
            get
            {
                return Options;
            }
        }
    }
}
