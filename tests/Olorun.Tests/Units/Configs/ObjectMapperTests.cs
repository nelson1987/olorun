using SharedDomain.Configs;

namespace Olorun.Tests.Units.Configs;
public class ObjectMapperTests
{
    [Fact]
    public void ValidateMappingConfigurationTest()
    {
        var mapper = Mappers.Mapper;

        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
