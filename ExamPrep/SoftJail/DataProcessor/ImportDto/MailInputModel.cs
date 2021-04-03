using System.ComponentModel.DataAnnotations;

namespace SoftJail.DataProcessor.ImportDto
{
    public class MailInputModel
    {
        [Required]
        public string Description { get; set; }

        [Required]
        public string Sender { get; set; }

        [RegularExpression(@"[A-Za-z0-9 ]* str\.$")]
        public string Address { get; set; }
    }
}

//"Mails": [
//      {
//        "Description": "Invalid FullName",
//        "Sender": "Invalid Sender",
//        "Address": "No Address"
//      },

/*    • Id – integer, Primary Key
• Description– text (required)
• Sender – text (required)
• Address – text, consisting only of letters, spaces and numbers, which ends with “ str.” (required) (Example: “62 Muir Hill str.“)
• PrisonerId - integer, foreign key (required)
• Prisoner – the mail's Prisoner (required)*/