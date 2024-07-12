using LearnITResourcesBDApi.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace LearnITResourcesBDApi.Data
{
    public class ScrapeCaptureDbContext:DbContext
    {
        public ScrapeCaptureDbContext(DbContextOptions options):base(options)
        {
            
        }

        public DbSet<TIOBERankedLanguage> TIOBERankedLanguages { get; set; }
        public DbSet<AmazonBook> AmazonBooks { get; set; }
        public DbSet<YouTubeChannel> YouTubeChannels { get; set; }


    }
}
