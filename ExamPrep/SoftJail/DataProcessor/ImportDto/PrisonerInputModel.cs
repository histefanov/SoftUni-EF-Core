using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Prisoner")]
    public class PrisonerInputModel
    {
        [XmlAttribute("Prisoner id")]
        public int Id { get; set; }
    }
}