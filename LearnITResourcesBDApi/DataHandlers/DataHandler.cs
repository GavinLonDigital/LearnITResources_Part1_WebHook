
using LearnITResourcesBDApi.Data;
using LearnITResourcesBDApi.Entities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace LearnITResourcesBDApi.DataHandlers
{
    public abstract class DataHandler
    {
        private readonly dynamic JSONResult;
        protected readonly ScrapeCaptureDbContext scrapeCaptureDbContext;

        protected int LanguageId { get; set; } = 0;

        protected DataHandler(dynamic JSONResult, ScrapeCaptureDbContext scrapeCaptureDbContext)
        {
            this.JSONResult = JSONResult;
            this.scrapeCaptureDbContext = scrapeCaptureDbContext;
        }
        
        public abstract JArray GetDataToProcess(dynamic data);
        public abstract void PrepareData(JArray data);
        public abstract Task RemoveData();
        public abstract Task<bool> AddData();

        public bool PrepData()
        {
            Console.WriteLine("PrepData is called");

            if (this.JSONResult?.GetType().ToString() == "Newtonsoft.Json.Linq.JObject")
            {
                if (this.JSONResult?.ContainsKey("status") && (this.JSONResult?.status == "collecting" || this.JSONResult?.status == "building"))
                {
                    Console.WriteLine($"BrightData dataset is not ready yet. Status: {this.JSONResult?.status}");
                }
            }
            else if (this.JSONResult?.GetType().ToString() == "Newtonsoft.Json.Linq.JArray")
            {
                Console.WriteLine("BrightData dataset available as JArray.");

                JArray jArrayDataToProcess = this.GetDataToProcess(this.JSONResult);

                this.PrepareData(jArrayDataToProcess);

                return true;

            }
            return false;
        }
        protected async Task SaveData()
        {
            Console.WriteLine("Saving data to our database.");

            await scrapeCaptureDbContext.SaveChangesAsync();

        }
        protected void SetLanguageId(JArray data, string languageName)
        {
           // string input = (string)data[1]["input"]["search"];

            LanguageId = this.scrapeCaptureDbContext.TIOBERankedLanguages.FirstOrDefault(l => l.LanguageName == languageName).Id;

        }

    }
}
