namespace Skuld.Models.API
{
    public class Urban
    {
        private string word;
        private string definition;
        private string permalink;
        private string example;
        private string author;
        private string upvotes;
        private string downvotes;

        public Urban(string word, string definition, string permalink, string example, string author, string upvotes, string downvotes)
        {
            this.word = word;
            this.definition = definition;
            this.permalink = permalink;
            this.example = example;
            this.author = author;
            this.upvotes = upvotes;
            this.downvotes = downvotes;
        }
        public string Word
        {
            get
            {
                return word;
            }
        }
        public string Definition
        {
            get
            {
                return definition;
            }
        }
        public string PermaLink
        {
            get
            {
                return permalink;
            }
        }
        public string Example
        {
            get
            {
                return example;
            }
        }
        public string Author
        {
            get
            {
                return author;
            }
        }
        public string UpVotes
        {
            get
            {
                return upvotes;
            }
        }
        public string DownVotes
        {
            get
            {
                return downvotes;
            }
        }
    }
}
