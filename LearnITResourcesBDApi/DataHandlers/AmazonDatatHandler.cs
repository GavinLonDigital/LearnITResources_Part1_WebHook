using LearnITResourcesBDApi.Data;
using LearnITResourcesBDApi.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace LearnITResourcesBDApi.DataHandlers
{
    public class AmazonDataHandler : DataHandler
    {
        IList<AmazonBook>? amazonBooks;

        public AmazonDataHandler(dynamic JSONResult, ScrapeCaptureDbContext scrapeCaptureDbContext):base((object)JSONResult, scrapeCaptureDbContext)
        {
            
        }
        public override async Task<bool> AddData()
        {
            Console.WriteLine($"Adding Amazon data to our database = LanguageId {LanguageId}");

            if(this.amazonBooks?.Count > 0)
            {
                await this.scrapeCaptureDbContext.AddRangeAsync(this.amazonBooks);

                await SaveData();
               
                return true;
            }
            return false;

        }

        public override JArray GetDataToProcess(dynamic data)
        {
            return (JArray)data;
        }

        public override void PrepareData(JArray bookData)
        {

            string input = (string)bookData[1]["input"]["search"];
            
            SetLanguageId(bookData, input); //Method resides in base abstract class - DataHandler

            IList<AmazonBook> bookList = bookData.Select(b=> new AmazonBook
            {
                Title = (string)b["title"]!,
                Url = (string)b["url"]!,
                Rating = (string)b["rating"]!,
                Reviews = (string)b["reviews"]!,
                ImageURL = (string)b["image"]!,
                Input = (string)b["input"]["search"]!,
                Price = (string)b["price"]!,
                PreviousPrice = (string)b["previous_price"]!,
                LanguageId = LanguageId
               

            }).ToList();

            this.amazonBooks = bookList;
        }

        public override async Task RemoveData()
        {
            await this.scrapeCaptureDbContext.Database.ExecuteSqlRawAsync($"DELETE FROM [AmazonBooks] WHERE LanguageId = {LanguageId}");

        }

    }
}
