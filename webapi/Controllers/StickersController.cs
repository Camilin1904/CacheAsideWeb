using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using webapi.SqlServer;

namespace webapi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class StickersController : ControllerBase
    {
        private readonly StickersContext _context;
        private readonly IDistributedCache _cache;

        public StickersController(StickersContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("{number:int}")]
        public async Task<IActionResult> GetSticker(int number)
        {
            var cacheKey = $"Sticker:{number}";
            var cachedSticker = await _cache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedSticker))
            {
                
                var sticker = JsonConvert.DeserializeObject<Sticker>(cachedSticker);
                return Ok(new { Source = "Redis Cache", Sticker = sticker });
            }

            var dbSticker = _context.Stickers.FirstOrDefault(x => x.Number == number);

            if (dbSticker != null)
            {
                await _cache.SetStringAsync(cacheKey, JsonConvert.SerializeObject(dbSticker));
                return Ok(new { Source = "SQL Database", Sticker = dbSticker });
            }

            return NotFound();
        }

        [HttpDelete("cache/{number:int}")]
        public async Task<IActionResult> RemoveStickerFromCache(int number)
        {
            var cacheKey = $"Sticker:{number}";
            await _cache.RemoveAsync(cacheKey);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSticker([FromBody] SqlServer.Sticker sticker)
        {
            _context.Stickers.Add(sticker);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSticker), new { number = sticker.Number }, sticker);
        }

        [HttpPut("{number:int}")]
        public async Task<IActionResult> UpdateSticker(int number, [FromBody] SqlServer.Sticker sticker)
        {
            var dbSticker = _context.Stickers.FirstOrDefault(x => x.Number == number);
            if (dbSticker == null)
            {
                return NotFound();
            }

            dbSticker.PlayerName = sticker.PlayerName;
            dbSticker.Country = sticker.Country;
            await _context.SaveChangesAsync();

            var cacheKey = $"Sticker:{number}";
            await _cache.RemoveAsync(cacheKey);

            return NoContent();
        }

        [HttpDelete("{number:int}")]
        public async Task<IActionResult> DeleteSticker(int number)
        {
            var dbSticker = _context.Stickers.FirstOrDefault(x => x.Number == number);
            if (dbSticker == null)
            {
                return NotFound();
            }

            _context.Stickers.Remove(dbSticker);
            await _context.SaveChangesAsync();

            var cacheKey = $"Sticker:{number}";
            await _cache.RemoveAsync(cacheKey);

            return NoContent();
        }
    }
}
