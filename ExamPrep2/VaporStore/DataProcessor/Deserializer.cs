namespace VaporStore.DataProcessor
{
	using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using VaporStore.Data.Models;
    using VaporStore.DataProcessor.Dto.Import;

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var outputSb = new StringBuilder();
			var games = new List<Game>();

			var gameDtos = JsonConvert.DeserializeObject<IEnumerable<GameImportDto>>(jsonString);

            foreach (var gameDto in gameDtos)
            {
                if (!IsValid(gameDto) ||
					gameDto.Tags.Count < 1)
                {
					outputSb.AppendLine("Invalid Data");
					continue;
                }

				var developer = context.Developers.FirstOrDefault(x => x.Name == gameDto.Developer)
					?? new Developer { Name = gameDto.Developer };

				var genre = context.Genres.FirstOrDefault(x => x.Name == gameDto.Genre)
					?? new Genre { Name = gameDto.Genre };

				var game = new Game
				{
					Name = gameDto.Name,
					Price = gameDto.Price,
					ReleaseDate = gameDto.ReleaseDate.Value,
					Developer = developer,
					Genre = genre
				};

                foreach (var jsonTag in gameDto.Tags)
                {
					var tag = context.Tags.FirstOrDefault(x => x.Name == jsonTag)
						?? new Tag { Name = jsonTag };
					
					game.GameTags.Add(new GameTag() { Tag = tag });
                }

				context.Games.Add(game);
				context.SaveChanges();
				outputSb.AppendLine($"Added {game.Name} ({game.Genre.Name}) with {game.GameTags.Count} tags");
            }

			return outputSb.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
            var outputSb = new StringBuilder();

			var data = JsonConvert.DeserializeObject<IEnumerable<UserImportDto>>(jsonString);

            foreach (var userDto in data)
            {
                if (!IsValid(userDto) ||
					!userDto.Cards.All(IsValid) ||
					userDto.Cards.Count == 0)
                {
					outputSb.AppendLine("Invalid Data");
					continue;
                }

				var user = new User
				{
					FullName = userDto.FullName,
					Username = userDto.Username,
					Email = userDto.Email,
					Age = userDto.Age,
					Cards = userDto.Cards.Select(c => new Card
					{
						Number = c.Number,
						Cvc = c.CVC,
						Type = c.Type.Value
					})
					.ToList()
				};

				context.Users.Add(user);
				context.SaveChanges();
				outputSb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
            }

			return outputSb.ToString().TrimEnd();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var sb = new StringBuilder();
			var xmlSerializer = new XmlSerializer(
				typeof(PurchaseImportDto[]), new XmlRootAttribute("Purchases"));

			var data = (PurchaseImportDto[])xmlSerializer.Deserialize(new StringReader(xmlString));

            foreach (var purchaseDto in data)
            {
                if (!IsValid(purchaseDto))
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				var dateIsValid = DateTime.TryParseExact(
					purchaseDto.Date,
					"dd/MM/yyyy HH:mm",
					CultureInfo.InvariantCulture,
					DateTimeStyles.None,
					out DateTime date);

                if (!dateIsValid)
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				var purchase = new Purchase()
				{
					Type = purchaseDto.Type.Value,
					ProductKey = purchaseDto.ProductKey,					
					Date = date
				};

				purchase.Card = context.Cards
						.FirstOrDefault(x => x.Number == purchaseDto.Card);
				purchase.Game = context.Games
						.FirstOrDefault(x => x.Name == purchaseDto.Title);

				context.Purchases.Add(purchase);
				context.SaveChanges();

				var user = context.Users.Where(x => x.Id == purchase.Card.UserId)
					.Select(x => x.Username).FirstOrDefault();
				sb.AppendLine($"Imported {purchaseDto.Title} for {user}");
            }

            Console.WriteLine(sb.ToString().TrimEnd());
			return sb.ToString().TrimEnd();
		}

		private static bool IsValid(object dto)
		{
			var validationContext = new ValidationContext(dto);
			var validationResult = new List<ValidationResult>();

			return Validator.TryValidateObject(dto, validationContext, validationResult, true);
		}
	}
}