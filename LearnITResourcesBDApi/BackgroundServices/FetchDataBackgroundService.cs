
using LearnITResourcesBDApi.Data;
using LearnITResourcesBDApi.DataHandlers;
using LearnITResourcesBDApi.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace LearnITResourcesBDApi.BackgroundServices
{
    public class FetchDataBackgroundService : BackgroundService
    {
        private readonly PeriodicTimer periodicTimer = 
            new PeriodicTimer(TimeSpan.FromMilliseconds(5000));
        private readonly IServiceScopeFactory factory;
        private bool IsEnabled = false;

        private string fetchDatasetId = "";

        private ScraperType scraperType = ScraperType.TIOBE;


        public FetchDataBackgroundService(IServiceScopeFactory factory)
        {
            this.factory = factory;
        }

        public void InitializeBackgroundService(string fetchDatasetId, ScraperType scraperType)
        {
            this.fetchDatasetId = fetchDatasetId;

            this.scraperType = scraperType;

            IsEnabled = true;

            Console.WriteLine("FetchDataBackgroundService has been enabled");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           while (await periodicTimer.WaitForNextTickAsync(stoppingToken)
                && !stoppingToken.IsCancellationRequested)
            {
                if(IsEnabled)
                {
                    await ProcessDatasetAsync();
                }

            }
        }

        private async Task ProcessDatasetAsync()
        {
            DataHandler dataHandler = null;
            
            Console.WriteLine("ProcessDatasetAsync");

            await using AsyncServiceScope asyncScope = this.factory.CreateAsyncScope();

            ScrapeCaptureDbContext scrapeCaptureDbContext = asyncScope.ServiceProvider.GetRequiredService<ScrapeCaptureDbContext>();
            
            HttpClient httpClient = asyncScope.ServiceProvider.GetRequiredService<HttpClient>();


            dynamic jsonObjectReturned = await GetDataSetFromBrightData(httpClient);

            if(this.scraperType == ScraperType.TIOBE)
            {
                dataHandler = new TIOBEDataHandler(jsonObjectReturned, scrapeCaptureDbContext);
            }
            else if(this.scraperType == ScraperType.Amazon)
            {
                dataHandler = new AmazonDataHandler(jsonObjectReturned, scrapeCaptureDbContext);
            }

            if(dataHandler != null)
            {
                bool dataPrepped = dataHandler.PrepData();

                if (dataPrepped)
                {
                    await dataHandler.RemoveData();
                   
                    bool dataAdded = await dataHandler.AddData();

                    IsEnabled = !dataAdded; //stops the background service once data is added successfully

                }


            }
        
        }

        private void PrepareHttpClientHeader(HttpClient httpClient)
        {
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", "22f606a9-94ec-4aeb-a624-151a97519057");

            httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        }

        private async Task<dynamic> GetDataSetFromBrightData(HttpClient httpClient)
        {
            dynamic jsonResult = null;

            PrepareHttpClientHeader(httpClient);

            //https://api.brightdata.com/dca/dataset?id=COLLECTION_ID

            var requestUri = $"https://api.brightdata.com/dca/dataset?id={this.fetchDatasetId}";

            var response = await httpClient.GetAsync(requestUri);

            var statusCode = response.StatusCode;

            Console.WriteLine($"requestUri {requestUri}");
            Console.WriteLine($"statusCode {statusCode}");

            if (response.IsSuccessStatusCode)
            {
                string json = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Successful result back from bright data.{json}");

                dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(json);

                return jsonObject;
            }
            return null;

        }

    }
    //Below is a model created just for testing purposes
    //public class TIOBEModel
    //{
    //    public int RankOrder { get; set; }
    //    public string LanguageName { get; set; }
    //    public string ImagePath { get; set; }
    //}

}
