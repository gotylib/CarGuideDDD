using Microsoft.AspNetCore.Mvc;

namespace CarGuideDDD.MailService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MailController : ControllerBase
    {
        [HttpPost("SendMessageToMain")]
        public IActionResult SendMessageToMain([FromBody] Message message)
        {
            MessageSender.SendMessage(message);
            return Ok();   
        }
    }
}
