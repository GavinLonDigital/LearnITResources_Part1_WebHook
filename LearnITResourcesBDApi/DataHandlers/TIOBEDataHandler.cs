
using LearnITResourcesBDApi.Data;
using LearnITResourcesBDApi.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace LearnITResourcesBDApi.DataHandlers
{
    public class TIOBEDataHandler:DataHandler
    {
        private IList<TIOBERankedLanguage>? rankedLanguages;
        public TIOBEDataHandler(dynamic JSONResult, ScrapeCaptureDbContext scrapeCaptureDbContext) : base((object)JSONResult, scrapeCaptureDbContext)
        {

        }

        public override JArray GetDataToProcess(dynamic data)
        {
            return (JArray)data[0].data.rankingArr;
        }
        public override void PrepareData(JArray languageDataArray)
        {


            IList<TIOBERankedLanguage> languages = languageDataArray.Select(l => new TIOBERankedLanguage
            {
                RankOrder = Int32.Parse((string)l["ranking"]!),
                LanguageName = (string)l["pLang"]!,
                ImagePath = (string)l["imagePath"]!
            }).ToList();

            this.rankedLanguages = languages;
        }

        public override async Task RemoveData()
        {
            await this.scrapeCaptureDbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [AmazonBooks]");
            await this.scrapeCaptureDbContext.Database.ExecuteSqlRawAsync("TRUNCATE TABLE [TIOBERankedLanguages]");

       }

        public override async Task<bool> AddData()
        {
            if(this.rankedLanguages?.Count > 0)
            {
                await this.scrapeCaptureDbContext.AddRangeAsync(this.rankedLanguages);
                await this.SaveData();

                return true;
            }

            return false;
        }

    }
}
