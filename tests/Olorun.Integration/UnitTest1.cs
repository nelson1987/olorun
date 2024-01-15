using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace Olorun.Integration
{
    public class OlorunApi : WebApplicationFactory<Program>
    {
        static OlorunApi()
            => Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Test");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.UseEnvironment("Test")
                      .ConfigureTestServices(services =>
                      {
                          services.AddScoped<ICadastroContaCommandHandler, CadastroContaCommandHandler>();
                          services.AddAuthentication(defaultScheme: "TestScheme")
                                  .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

                          //KafkaFixture.ConfigureKafkaServices(services);
                      });

        private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
                : base(options, logger, encoder, clock)
            { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var claims = new[] {
                new Claim(ClaimTypes.Name, "Test user"),
                new Claim("preferred_username", "user@stone.com.br")
            };
                var identity = new ClaimsIdentity(claims, "Test");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "TestScheme");

                var result = AuthenticateResult.Success(ticket);

                return Task.FromResult(result);
            }
        }
    }
    public class OlorunApiFixture
    {
        private static readonly OlorunApi _server;
        private static readonly HttpClient _client;

        public OlorunApi Server => _server;
        public HttpClient Client => _client;

        static OlorunApiFixture()
        {
            _server = new();
            _client = _server.CreateDefaultClient();
        }

        public OlorunApiFixture()
        {
            _client.DefaultRequestHeaders.Clear();

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(scheme: "TestScheme");
        }
    }
    public class IntegrationTests1 : IClassFixture<OlorunApiFixture>
    {
        private readonly OlorunApiFixture _olorunApiFixture;

        public IntegrationTests1(OlorunApiFixture olorunApiFixture)
            => _olorunApiFixture = olorunApiFixture;

        [Fact]
        public void Dado_Request_Valido_Deve_Retornar_True()
        {
            // Act
            var updateFinancialData = new CadastroContaCommand()
            {
                Documento = "Documento",
                Id = Guid.NewGuid()
            };
            var updateFinancialDataAsString = JsonConvert.SerializeObject(updateFinancialData);
            using var stringContent = new StringContent(updateFinancialDataAsString, Encoding.UTF8, "application/json");

            // Act
            var response = _olorunApiFixture.Client.PutAsync("/credit-titles/financial", stringContent);

            // Assert
            Assert.True(response.Result.IsSuccessStatusCode);
        }
    }
    public class UnitTest1
    {
        private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization { ConfigureMembers = true });

        private readonly CadastroContaCommandHandler _controller;
        private readonly CadastroContaCommand _request;
        private readonly Mock<IValidator<CadastroContaCommand>> _validator;
        private readonly Mock<IHttpAntifraudeService> _antifraudeService;
        private readonly Mock<IContaRepository> contaRepository;
        private readonly Mock<IContaProducer> contaProducer;

        public UnitTest1()
        {
            _request = _fixture.Build<CadastroContaCommand>()
                .Create();

            _validator = _fixture.Freeze<Mock<IValidator<CadastroContaCommand>>>();
            _validator
                .Setup(x => x.Validate(It.IsAny<CadastroContaCommand>()))
                .Returns(true);

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
            _validator
                .Setup(x => x.Validate(It.IsAny<CadastroContaCommand>()))
                .Returns(false);

            // Act
            var result = _controller.Handle(_request);

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
}