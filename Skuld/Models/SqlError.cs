namespace Skuld.Models
{
    public class SqlError
    {
        public bool Successful { get; private set; }
        public string Error { get; private set; }
        public SqlError() { }
        public SqlError(bool successful)
        {
            Successful = successful;
            Error = "SQL Command executed successfully";
        }
        public SqlError(bool successful, string error)
        {
            Successful = successful;
            Error = error;
        }
    }
}
