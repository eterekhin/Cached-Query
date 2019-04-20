using System.IO;
using Microsoft.Extensions.Configuration;

namespace UnitTests
{
    public class BaseTest
    {
        public BaseTest()
        {
            var builder = new ConfigurationBuilder();
            var configuration = builder.Build();

            var startup = new Startup();
        }
    }
}