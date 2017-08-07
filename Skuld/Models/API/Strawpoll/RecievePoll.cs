namespace Skuld.Models.API.Strawpoll
{
    public class RecievePoll
    {
        private int id;
        private string title;
        private bool multi;
        private string[] options;
        private int[] votes;
        private string dupcheck;
        private bool captcha;

        public RecievePoll(int id, string title, bool multi, string[] options, int[] votes, string dupcheck, bool captcha)
        {
            this.id = id;
            this.title = title;
            this.multi = multi;
            this.options = options;
            this.votes = votes;
            this.dupcheck = dupcheck;
            this.captcha = captcha;
        }

        public int ID
        {
            get
            {
                return id;
            }
        }
        public string Title
        {
            get
            {
                return title;
            }
        }
        public bool Multi
        {
            get
            {
                return multi;
            }
        }
        public string[] Options
        {
            get
            {
                return options;
            }
        }
        public int[] Votes
        {
            get
            {
                return votes;
            }
        }
        public string Dupcheck
        {
            get
            {
                return dupcheck;
            }
        }
        public bool Captcha
        {
            get
            {
                return captcha;
            }
        }
        public string Url
        {
            get
            {
                return "http://www.strawpoll.me/" + id;
            }
        }
    }
}
