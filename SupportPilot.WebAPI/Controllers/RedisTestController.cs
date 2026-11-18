using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportPilot.Application.Common.Interfaces;

namespace SupportPilot.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class RedisTestController : ControllerBase
{
    private readonly IRedisService _redisService;

    public RedisTestController(IRedisService redisService)
    {
        _redisService = redisService;
    }

    [HttpPost]
    public async Task<IActionResult> Set()
    {
        const string key = "supportpilot:test";
        const string value = "Redis is working!";

        await _redisService.SetAsync(
            key,
            value,
            TimeSpan.FromMinutes(5));

        return Ok(new
        {
            message = "Value saved to Redis.",
            key,
            value
        });
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        const string key = "supportpilot:test";

        var value = await _redisService.GetAsync(key);

        if (value is null)
        {
            return NotFound(new
            {
                message = "Key not found in Redis."
            });
        }

        return Ok(new
        {
            key,
            value
        });
    }

    [HttpDelete]
    public async Task<IActionResult> Delete()
    {
        const string key = "supportpilot:test";

        await _redisService.RemoveAsync(key);

        return Ok(new
        {
            message = "Key deleted from Redis."
        });
    }

    [HttpGet("exists")]
    public async Task<IActionResult> Exists()
    {
        const string key = "supportpilot:test";

        var exists = await _redisService.ExistsAsync(key);

        return Ok(new
        {
            key,
            exists
        });
    }
}