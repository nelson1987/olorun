using Microsoft.Extensions.Logging;

namespace Olorun.Integration.Configs;

public enum ContaStatus { Cadastrado = 0, Aberto = 1, Fechado = 2, Bloqueado = 3 }

public class Conta
{
    public Guid Id { get; set; }
    public required string Documento { get; set; }
    public required string Numero { get; set; }
    public ContaStatus Status { get; set; }
}

public record CadastroContaCommand
{
    public required Guid Id { get; init; }
    public required string Documento { get; init; }
};

public record CadastroContaCommandEvent
{
    public required Guid Id { get; init; }
    public required string Documento { get; init; }
    public required bool ValidoAntiFraude { get; init; }
};

public interface ICadastroContaCommandHandler
{
    bool Handle(CadastroContaCommand request);
}

public class CadastroContaCommandHandler : ICadastroContaCommandHandler
{
    private readonly ILogger<CadastroContaCommandHandler> _log;
    private readonly IValidator<CadastroContaCommand> _validator;
    private readonly IHttpAntifraudeService _antifraudeService;
    private readonly IContaProducer _contaProducer;
    private readonly IContaRepository _contaRepository;

    public CadastroContaCommandHandler(
        IValidator<CadastroContaCommand> validator,
        IHttpAntifraudeService antifraudeService,
        IContaProducer contaProducer,
        IContaRepository contaRepository,
        ILogger<CadastroContaCommandHandler> log)
    {
        _validator = validator;
        _antifraudeService = antifraudeService;
        _contaProducer = contaProducer;
        _contaRepository = contaRepository;
        _log = log;
    }

    public bool Handle(CadastroContaCommand request)
    {
        try
        {
            var validation = _validator.Validate(request);

            if (validation.IsValid &&
                _antifraudeService.Get(request.Documento))
            {
                var conta = new Conta()
                {
                    Numero = "",
                    Documento = request.Documento,
                    Id = request.Id,
                    Status = ContaStatus.Cadastrado
                };
                _contaRepository.Insert(conta);

                var @event = new CadastroContaCommandEvent()
                {
                    Documento = request.Documento,
                    Id = request.Id,
                    ValidoAntiFraude = true
                };
                _contaProducer.Send(@event);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _log.LogInformation($"erro || {ex.Message}");
            return false;
        }
    }
}



public class CadastroContaCommandValidator : AbstractValidator<CadastroContaCommand>
{
}

public interface IHttpAntifraudeService
{
    bool Get(string documento);
}

public class HttpAntiFraudeService : IHttpAntifraudeService
{
    public bool Get(string documento)
    {
        return true;
    }
}

public interface IContaProducer
{
    void Send(CadastroContaCommandEvent @event);
}

public class ContaProducer : IContaProducer
{
    public void Send(CadastroContaCommandEvent @event)
    {
        //throw new NotImplementedException();
    }
}

public interface IContaRepository
{
    void Insert(Conta conta);
}

public class ContaRepository : IContaRepository
{
    public void Insert(Conta conta)
    {
        throw new NotImplementedException();
    }
}
