using SharedDomain.Configs;

namespace Olorun.Integration.Units;

public class ObjectMapperTests
{
    [Fact]
    public void ValidateMappingConfigurationTest()
    {
        var mapper = Mappers.Mapper;

        mapper.ConfigurationProvider.AssertConfigurationIsValid();
    }
}
