using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace VaporStore.DataProcessor.Dto.Export
{
    public class GenreExportDto
    {
        public int Id { get; set; }

        [JsonProperty("Genre")]
        public string Name { get; set; }

        public ICollection<GameExportDto> Games { get; set; }

        public int TotalPlayers { get; set; }
    }
}
