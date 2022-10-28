using Microsoft.AspNetCore.Mvc;
using APIMemoryInCache_Countries.Model;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;


namespace APIMemoryInCache_Countries.Controllers
{
    [Route("api/[Controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IMemoryCache _memoryCache;
        private const string COUNTRIES_KEY = "Countries";
        public CountriesController(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            const string restCountriesUrl = "https://viacep.com.br/ws/01001000/json/";

            if (_memoryCache.TryGetValue(COUNTRIES_KEY, out List<Country> countries))
            {
                return Ok(countries);
            }

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(restCountriesUrl);
                
                var responseData = await response.Content.ReadAsStringAsync();

                countries = JsonSerializer.Deserialize<List<Country>>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


                var memoryCacheEntryOption = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3600),
                    SlidingExpiration = TimeSpan.FromSeconds(1200)
                };

                _memoryCache.Set(COUNTRIES_KEY, countries, memoryCacheEntryOption);

                return Ok(countries);
            }
        }
    }
}
