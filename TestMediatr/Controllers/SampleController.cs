using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestMediatr.Commands;

namespace TestMediatr.Controllers
{
    [Route("api/[controller]")]
    public class SampleController : Controller
    {
        private readonly IMediator _mediator;

        public SampleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            var pingCommand = new PingCommand();
            var result = await _mediator.Send(pingCommand).ConfigureAwait(false);
            return Ok(result);
        }
        
    }
}
