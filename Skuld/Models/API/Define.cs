namespace Skuld.Models.API
{
    public class Define
    {
        private string word;
        private string definition;
        private string example;
        private string partofspeech;
        private string term;

        public Define(string word, string definition, string example, string partofspeech, string term)
        {
            this.word = word;
            this.definition = definition;
            this.example = example;
            this.partofspeech = partofspeech;
            this.term = term;
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
        public string Example
        {
            get
            {
                return example;
            }
        }
        public string PartOfSpeech
        {
            get
            {
                return partofspeech;
            }
        }
        public string Terms
        {
            get
            {
                return term;
            }
        }
    }
}
