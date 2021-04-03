namespace SoftJail.DataProcessor
{

    using Data;
    using Newtonsoft.Json;
    using SoftJail.DataProcessor.ExportDto;
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners = context.Prisoners
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.FullName,
                    CellNumber = x.Cell.CellNumber,
                    Officers = x.PrisonerOfficers.Select(po => new
                        {
                            OfficerName = po.Officer.FullName,
                            Department = po.Officer.Department.Name
                        })
                        .OrderBy(x => x.OfficerName)
                        .ToList(),
                    TotalOfficerSalary = decimal.Parse(x.PrisonerOfficers
                        .Sum(po => po.Officer.Salary)
                        .ToString("F2"))
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToList();

            var result = JsonConvert.SerializeObject(prisoners, Formatting.Indented);
            return result;
        }

        /*[
              {
                "Id": 3,
                "Name": "Binni Cornhill",
                "CellNumber": 503,
                "Officers": [
                  {
                    "OfficerName": "Hailee Kennon",
                    "Department": "ArtificialIntelligence"
                  },
                  {
                    "OfficerName": "Theo Carde",
                    "Department": "Blockchain"
                  }
                ],
                "TotalOfficerSalary": 7127.93
              },
        */

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var namesArray = prisonersNames
                .Split(',', StringSplitOptions.RemoveEmptyEntries);
            
            var prisoners = context.Prisoners
                .Where(x => namesArray.Contains(x.FullName))
                .Select(x => new PrisonerInboxOutputModel
                {
                    Id = x.Id,
                    Name = x.FullName,
                    IncarcerationDate = x.IncarcerationDate.ToString("yyyy-MM-dd"),
                    EncryptedMessages = x.Mails.Select(m => new EncryptedMessageOutputModel
                    {
                        Description = string.Join("", m.Description.Reverse())
                    })
                    .ToArray()
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToArray();

            var xmlSerializer = new XmlSerializer(
                typeof(PrisonerInboxOutputModel[]), new XmlRootAttribute("Prisoners"));

            var sb = new StringBuilder();

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            xmlSerializer.Serialize(new StringWriter(sb), prisoners, ns);

            return sb.ToString();
        }

        /*
         *<Prisoners>
              <Prisoner>
                <Id>3</Id>
                <Name>Binni Cornhill</Name>
                <IncarcerationDate>1967-04-29</IncarcerationDate>
                <EncryptedMessages>
                  <Message>
                    <Description>!?sdnasuoht evif-ytnewt rof deksa uoy ro orez artxe na ereht sI</Description>
                  </Message>
                </EncryptedMessages>
              </Prisoner>*/
    }
}