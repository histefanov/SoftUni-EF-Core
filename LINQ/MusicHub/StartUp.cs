namespace MusicHub
{
    using System;
    using System.Linq;
    using System.Text;
    using Data;
    using Initializer;
    using Microsoft.EntityFrameworkCore;

    public class StartUp
    {
        public static void Main(string[] args)
        {
            MusicHubDbContext context = 
                new MusicHubDbContext();

            //DbInitializer.ResetDatabase(context);

            var albumInfo = ExportSongsAboveDuration(context, 4);
            Console.WriteLine(albumInfo);
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            var albumsInfo = context.Producers
                .FirstOrDefault(p => p.Id == producerId)
                .Albums
                .Select(a => new
                {
                    AlbumName = a.Name,
                    ReleaseDate = a.ReleaseDate,
                    ProducerName = a.Producer.Name,
                    AlbumPrice = a.Price,
                    Songs = a.Songs.Select(s => new
                    {
                        SongName = s.Name,
                        Price = s.Price,
                        Writer = s.Writer.Name
                    })
                    .OrderByDescending(s => s.SongName)
                    .ThenBy(s => s.Writer)
                    .ToList()
                })
                .OrderByDescending(a => a.AlbumPrice)
                .ToList();

            var sb = new StringBuilder();

            foreach (var album in albumsInfo)
            {
                sb.AppendLine($"-AlbumName: {album.AlbumName}")
                    .AppendLine($"-ReleaseDate: {album.ReleaseDate:MM/dd/yyyy}")
                    .AppendLine($"-ProducerName: {album.ProducerName}")
                    .AppendLine("-Songs:");                  

                var songNumber = 1;

                foreach (var song in album.Songs)
                {
                    sb.AppendLine($"---#{songNumber++}")
                        .AppendLine($"---SongName: {song.SongName}")
                        .AppendLine($"---Price: {song.Price:F2}")
                        .AppendLine($"---Writer: {song.Writer}");
                }

                sb.AppendLine($"-AlbumPrice: {album.AlbumPrice:F2}");
            }

            return sb.ToString().TrimEnd();
        }

        public static string ExportSongsAboveDuration(MusicHubDbContext context, int duration)
        {
            var songs = context.Songs
                .ToList()
                .Where(s => s.Duration.TotalSeconds > duration)
                .Select(s => new
                {
                    SongName = s.Name,
                    Writer = s.Writer.Name,
                    PerformerFullName = s.SongPerformers
                                .Select(x => x.Performer.FirstName + " " + x.Performer.LastName)
                                .FirstOrDefault(),
                    AlbumProducer = s.Album.Producer.Name,
                    s.Duration
                })
                .OrderBy(s => s.SongName)
                .ThenBy(s => s.Writer)
                .ThenBy(s => s.PerformerFullName)
                .ToList();

            var sb = new StringBuilder();
            var counter = 1;

            foreach (var song in songs)
            {
                sb.AppendLine($"-Song #{counter++}")
                    .AppendLine($"---SongName: {song.SongName}")
                    .AppendLine($"---Writer: {song.Writer}")
                    .AppendLine($"---Performer: {song.PerformerFullName}")
                    .AppendLine($"---AlbumProducer: {song.AlbumProducer}")
                    .AppendLine($"---Duration: {song.Duration:c}");
            }

            return sb.ToString().TrimEnd();
        }
    }
}
