namespace Skuld.Models.API
{
    public class IMDb
    {
        private string title, year, released, runtime, genre, plot, language, country, awards, poster, imdbrating, imdbid, type, totalseasons, imdbvotes;

        public IMDb(string awards, string country, string genre, string imdbid, string imdbrating, string votes, string language, string plot, string poster, string released, string runtime, string title, string totalseasons, string type, string year)
        {
            this.title = title;
            this.year = year;
            this.released = released;
            this.runtime = runtime;
            this.genre = genre;
            this.plot = plot;
            this.language = language;
            this.country = country;
            this.awards = awards;
            this.poster = poster;
            this.imdbrating = imdbrating;
            this.imdbid = imdbid;
            this.type = type;
            this.totalseasons = totalseasons;
            this.imdbvotes = votes;
        }
        public string Title
        {
            get
            {
                return title;
            }
        }
        public string Year
        {
            get
            {
                return year;
            }
        }
        public string Released
        {
            get
            {
                return released;
            }
        }
        public string Runtime
        {
            get
            {
                return runtime;
            }
        }
        public string Genre
        {
            get
            {
                return genre;
            }
        }
        public string Plot
        {
            get
            {
                return plot;
            }
        }
        public string Language
        {
            get
            {
                return language;
            }
        }
        public string Country
        {
            get
            {
                return country;
            }
        }
        public string Awards
        {
            get
            {
                return awards;
            }
        }
        public string Poster
        {
            get
            {
                return poster;
            }
        }
        public string imdbRating
        {
            get
            {
                return imdbrating;
            }
        }
        public string imdbID
        {
            get
            {
                return imdbid;
            }
        }
        public string Type
        {
            get
            {
                return type;
            }
        }
        public string totalSeasons
        {
            get
            {
                return totalseasons;
            }
        }
        public string imdbVotes
        {
            get
            {
                return imdbvotes;
            }
        }
    }
}
