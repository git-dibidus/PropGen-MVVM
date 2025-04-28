using PropGen.Core.Models;

namespace PropGen.Core.Services
{
    public interface ITextParserService
    {
        PropertyParserResult ParseText(string input);
    }
}
