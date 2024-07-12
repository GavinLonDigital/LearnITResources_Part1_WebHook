using LearnITResourcesBDApi.BackgroundServices;
using LearnITResourcesBDApi.Enums;
using LearnITResourcesBDApi.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace LearnITResourcesBDApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HandleBDDataWebhook : ControllerBase
    {
        private readonly FetchDataBackgroundService fetchDataBackgroundService;

        public HandleBDDataWebhook(FetchDataBackgroundService fetchDataBackgroundService)
        {
            this.fetchDataBackgroundService = fetchDataBackgroundService;

        }

        [HttpPost()]
        public async Task<IActionResult> Post()
        {
            try
            {
                var requestBody = await Request.Body.ReadAsStringAsync();

                dynamic dynamicObject = JsonConvert.DeserializeObject<dynamic>(requestBody);

                Console.WriteLine($"Raw data first sent from BrightData to our Webhook: {dynamicObject}");

                if (dynamicObject != null) 
                {
                    //TIOBE collector c_lv3l43x61hnemlo6b5
                    //Amazon collector c_lv6dn9c92fs2qhddkg 

                    var fetchDatasetId = (string)dynamicObject.id;

                    if (dynamicObject["collector_id"] == "c_lv3l43x61hnemlo6b5") //TIOBE
                    {
                       
                        Console.WriteLine($"fetchDatasetId: {fetchDatasetId}");

                        this.fetchDataBackgroundService.InitializeBackgroundService(fetchDatasetId, ScraperType.TIOBE);
                    }
                    else if(dynamicObject["collector_id"] == "c_lv6dn9c92fs2qhddkg") //Amazon
                    {
                        
                        Console.WriteLine($"Amazon: fetchDatasetId: {fetchDatasetId}");
                       
                        this.fetchDataBackgroundService.InitializeBackgroundService(fetchDatasetId, ScraperType.Amazon);
                    }

                }





            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok();
        
        }


    }
}
