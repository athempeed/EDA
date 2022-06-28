namespace EDA.Models
{
    public class EmailData
    {

        public string Subject { get; set; }
        public string To { get; set; }
        public string? From { get; set; }
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Body { get; set; }
    }
}
