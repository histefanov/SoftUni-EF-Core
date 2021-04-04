namespace VaporStore.DataProcessor
{
	using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.DataProcessor.Dto.Export;

    public static class Serializer
	{
		public static string ExportGamesByGenres(VaporStoreDbContext context, string[] genreNames)
		{
			var genreDtos = context.Genres
				.ToList()
				.Where(x => genreNames.Contains(x.Name))
				.Select(x => new GenreExportDto
				{
					Id = x.Id,
					Name = x.Name,
					Games = x.Games
						.Where(g => g.Purchases.Count > 0)
						.Select(g => new GameExportDto
						{
							Id = g.Id,
							Title = g.Name,
							Developer = g.Developer.Name,
							Tags = string.Join(", ", g.GameTags.Select(gt => gt.Tag.Name)),
							Players = g.Purchases.Count
						})
						.OrderByDescending(g => g.Players)
						.ThenBy(g => g.Id)
						.ToList(),
					TotalPlayers = x.Games.Sum(g => g.Purchases.Count)
				})
				.OrderByDescending(x => x.TotalPlayers)
				.ThenBy(x => x.Id)
				.ToList();

			var result = JsonConvert.SerializeObject(genreDtos, Formatting.Indented);
			return result;
		}

		public static string ExportUserPurchasesByType(VaporStoreDbContext context, string storeType)
		{
			var data = context.Users
				.ToList()
				.Where(u => u.Cards.Any(c => c.Purchases.Any(p => p.Type.ToString() == storeType)))
				.Select(u => new UserExportDto
				{
					Username = u.Username,
					TotalSpent = u.Cards
						.Sum(c => c.Purchases.Where(p => p.Type.ToString() == storeType)
						.Sum(p => p.Game.Price)),
					Purchases = u.Cards
						.SelectMany(c => c.Purchases)
						.Where(p => p.Type.ToString() == storeType)
						.Select(p => new PurchaseExportDto
						{
							Card = p.Card.Number,
							Cvc = p.Card.Cvc,
							Date = p.Date.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture),
							Game = new GameXmlExportDto
							{
								Title = p.Game.Name,
								Genre = p.Game.Genre.Name,
								Price = p.Game.Price
							}
						})
						.OrderBy(p => p.Date)
						.ToArray()
				})
				.OrderByDescending(u => u.TotalSpent)
				.ThenBy(u => u.Username)
				.ToArray();

			var sb = new StringBuilder();
			var xmlSerializer = new XmlSerializer(
				typeof(UserExportDto[]), new XmlRootAttribute("Users"));

			var ns = new XmlSerializerNamespaces();
			ns.Add("", "");

			xmlSerializer.Serialize(new StringWriter(sb), data, ns);

			return sb.ToString();
		}
	}
}