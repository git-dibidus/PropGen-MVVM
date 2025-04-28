using PropGen.Core.Models;

namespace PropGen.Core.Services
{
    public interface ISourceParserService
    {
        PropertyParserResult ParseFile(string filePath);
        PropertyParserResult ParseSource(string sourceCode);
    }
}
