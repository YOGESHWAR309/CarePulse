using Microsoft.AspNetCore.Mvc;
using CarePulse.API.Services;

namespace CarePulse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ISmsService _smsService;

        public TestController(ISmsService smsService)
        {
            _smsService = smsService;
        }

        public class SendSmsRequest
        {
            public string PhoneNumber { get; set; }
            public string Message { get; set; }
        }

        [HttpPost("send-sms")]
        public async Task<IActionResult> SendSms([FromBody] SendSmsRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.PhoneNumber) || string.IsNullOrWhiteSpace(req.Message))
                return BadRequest(new { ok = false, error = "phoneNumber and message are required" });

            var ok = await _smsService.SendSmsAsync(req.PhoneNumber, req.Message);
            if (ok) return Ok(new { ok = true });
            return StatusCode(500, new { ok = false, error = "failed to send" });
        }
    }
}
