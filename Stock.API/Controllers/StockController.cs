using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Stock.API.Controllers
{
    [Route("api/stocks")]
    [ApiController]
    public class StockController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // await Task.Delay(10000); // 10 saniye gecikme

            return Ok("Stock API çalışıyor ama canı çıkmış durumda...");
        }
    }
}
