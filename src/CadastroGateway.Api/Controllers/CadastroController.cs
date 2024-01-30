using Microsoft.AspNetCore.Mvc;

namespace CadastroGateway.Api.Controllers
{
    public record InclusaoCadastroCommand
    {
        public DateOnly Date { get; set; }

        public int TemperatureC { get; set; }

        public string? Summary { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class CadastroController : ControllerBase
    {
        private readonly ILogger<CadastroController> _logger;

        public CadastroController(ILogger<CadastroController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetContas")]
        public IEnumerable<InclusaoCadastroCommand> Get()
        {
            _logger.LogInformation("GetContas");
            return Enumerable.Range(1, 5).Select(index => new InclusaoCadastroCommand
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55)
            })
            .ToArray();
        }

        [HttpPost(Name = "Post")]
        public IEnumerable<InclusaoCadastroCommand> Post()
        {
            _logger.LogInformation("PostContas");
            return Enumerable.Range(1, 5).Select(index => new InclusaoCadastroCommand
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55)
            })
            .ToArray();
        }
    }
}