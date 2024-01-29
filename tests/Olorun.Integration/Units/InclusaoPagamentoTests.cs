using FluentValidation.Results;
using SharedDomain.Features.Pagamentos;
using SharedDomain.Features.Pagamentos.Events.Incluido;
using SharedDomain.Features.Pagamentos.Inclusao;
using SharedDomain.Shared;

namespace Olorun.Integration.Units;
public class InclusaoPagamentoTests
{
    //
    /*
    InclusaoPagamentoCommand -> CommandHandler -> (Insert) ReadRepository -> Event -> Producer(Fire And Forget)
    Consumer -> (insert) WriteRepository -> Next Event -> Producer -> (Update) ReadRepository
    Consumer -> (Update) WriteRepository -> Next Event -> Producer -> (Update) ReadRepository
    */
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });
    private CancellationToken _token => CancellationToken.None;
    private readonly InclusaoPagamentoCommandHandler _handler;
    private readonly InclusaoPagamentoCommand _request;
    private readonly Mock<IValidator<InclusaoPagamentoCommand>> _validator;
    private readonly Mock<EventProducer<PagamentoIncluidoEvent>> _eventProducer;
    private readonly Mock<IPagamentoReadRepository> _repository;

    public InclusaoPagamentoTests()
    {
        _request = _fixture.Build<InclusaoPagamentoCommand>()
            .Create();

        _validator = _fixture.Freeze<Mock<IValidator<InclusaoPagamentoCommand>>>();
        _repository = _fixture.Freeze<Mock<IPagamentoReadRepository>>();
        _eventProducer = _fixture.Freeze<Mock<EventProducer<PagamentoIncluidoEvent>>>();

        _validator
            .Setup(x => x.Validate(It.IsAny<InclusaoPagamentoCommand>()))
            .Returns(new ValidationResult());

        _repository
            .Setup(x => x.CreateAsync(It.IsAny<SharedDomain.Features.Pagamento>(), _token))
            .Returns(Task.CompletedTask);

        _eventProducer.Setup(x => x.Send(It.IsAny<PagamentoIncluidoEvent>(), _token))
            .Returns(Task.CompletedTask);

        _handler = _fixture.Build<InclusaoPagamentoCommandHandler>()
            .Create();
    }

    [Fact]
    public async Task Dado_Request_Valido_Deve_Retornar_Sucesso()
    {
        // Act
        var result = await _handler.Handle(_request, _token);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Dado_Request_Invalido_Deve_Retornar_Falha()
    {
        // Assert
        _fixture.Freeze<Mock<IValidator<InclusaoPagamentoCommand>>>()
                .Setup(x => x.Validate(_request))
                .Returns(new ValidationResult(new[] { new ValidationFailure() }));

        // Act
        var result = await _handler.Handle(_request, _token);

        // Assert
        Assert.False(result.IsSuccess);
        //_fixture.Freeze<Mock<IPagamentoReadRepository>>().VerifyNoOtherCalls();
        //_fixture.Freeze<Mock<IEventProducer<PagamentoIncluidoEvent>>>().VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Dado_Repositorio_Retorne_Erro_Deve_Retornar_Falha()
    {
        // Assert
        _repository
            .Setup(x => x.CreateAsync(It.IsAny<SharedDomain.Features.Pagamento>(), _token))
                .Returns(Task.FromException(new NotImplementedException()));
        //.ReturnsAsync(Result.Fail("some error"));

        // Act
        var result = await _handler.Handle(_request, _token);

        // Assert
        Assert.False(result.IsSuccess);
        //_fixture.Freeze<Mock<IEventProducer<PagamentoIncluidoEvent>>>().VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Dado_Produtor_Retorne_Erro_Deve_Retornar_Falha()
    {
        // Assert
        _eventProducer
            .Setup(x => x.Send(It.IsAny<PagamentoIncluidoEvent>(), _token))
            .Returns(Task.FromException(new NotImplementedException()));

        // Act
        var result = await _handler.Handle(_request, _token);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
