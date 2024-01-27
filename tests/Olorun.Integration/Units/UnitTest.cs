using FluentValidation.Results;
using Olorun.Integration.Configs;
using SharedDomain.Configs;

namespace Olorun.Integration.Units;
//[UnitTest]
public class ObjectMapperTests
{
    [Fact]
    public void ValidateMappingConfigurationTest()
    {
        var mapper = Mappers.Mapper;

        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
public class UnitTest
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

    private readonly CadastroContaCommandHandler _controller;
    private readonly CadastroContaCommand _request;
    private readonly Mock<IValidator<CadastroContaCommand>> _validator;
    private readonly Mock<IHttpAntifraudeService> _antifraudeService;
    private readonly Mock<IContaRepository> contaRepository;
    private readonly Mock<IContaProducer> contaProducer;

    public UnitTest()
    {
        _request = _fixture.Build<CadastroContaCommand>()
            .Create();

        _validator = _fixture.Freeze<Mock<IValidator<CadastroContaCommand>>>();
        _validator
            .Setup(x => x.Validate(It.IsAny<CadastroContaCommand>()))
            .Returns(new ValidationResult());

        _antifraudeService = _fixture.Freeze<Mock<IHttpAntifraudeService>>();
        _antifraudeService
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(true);

        contaRepository = _fixture.Freeze<Mock<IContaRepository>>();
        contaProducer = _fixture.Freeze<Mock<IContaProducer>>();

        _controller = _fixture.Build<CadastroContaCommandHandler>()
            .Create();
    }

    [Fact]
    public void Dado_Request_Valido_Deve_Retornar_True()
    {
        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.True(result);
    }
    [Fact]
    public void Dado_Request_Invalido_Deve_Retornar_False()
    {
        var invalidRequest = _request with { Id = Guid.Empty };

        // Act
        var result = _controller.Handle(invalidRequest);

        // Assert
        Assert.False(result);
    }
    [Fact]
    public void Dado_Documento_Invalido_Antifraude_Deve_Retornar_False()
    {
        _antifraudeService
            .Setup(x => x.Get(It.IsAny<string>()))
            .Returns(false);

        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.False(result);
    }
    [Fact]
    public void Dado_Insercao_Persistencia_Erro_Deve_Retornar_False()
    {
        contaRepository
         .Setup(x => x.Insert(It.IsAny<Conta>()))
         .Throws<Exception>();

        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.False(result);
    }
    [Fact]
    public void Dado_Publicacao_Evento_Erro_Deve_Retornar_False()
    {
        contaProducer
         .Setup(x => x.Send(It.IsAny<CadastroContaCommandEvent>()))
         .Throws<Exception>();
        // Act
        var result = _controller.Handle(_request);

        // Assert
        Assert.False(result);
    }
}
