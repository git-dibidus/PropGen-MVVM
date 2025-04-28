using PropGen.Core.Models;
using PropGen.Core.Services;

var parser = new SimplePropertyParser();


//while (true)
//{
//    Console.WriteLine("Enter properties in the format 'Name, Type' (or type 'exit' to quit):");


string input = @"CustId, int
CustName, string
string Address1
string Address2


string City
City, string
string State";    




var result = parser.GenerateProperties(input);
    if (result.Classes[0].Properties.Count > 0)
    {
        foreach (var prop in result.Classes[0].Properties)
        {
            Console.WriteLine($"Property: {prop.Name}, Type: {prop.Type}");
        }
    }
    if (result.Issues.Count > 0)
    {
        foreach (var issue in result.Issues)
        {
            Console.WriteLine($"Error on Line {issue.LineNumber}: {issue.Message}");
        }
    }
//}



//var parserResult = parser.ParseFile("D:\\Temp\\PresenterModel.cs");

//if (parserResult.HasIssues)
//{
//    foreach (var issue in parserResult.Issues)
//    {
//        Console.WriteLine($"Issue: {issue.Severity} - {issue.Message}");
//    }
//    return;
//}


//foreach (var classInfo in parserResult.Classes)
//{
//    Console.WriteLine($"Class: {classInfo.ClassName}");
//    Console.WriteLine($"Namespace: {classInfo.Namespace}");
//    foreach (var prop in classInfo.Properties)
//    {
//         Console.WriteLine($"Property: {prop.Name}, Type: {prop.Type}");
//    }
//}

//Console.WriteLine("\nParsing completed successfully. Here is the output: \n");

var generator = new PropertyCodeGenerator();

//parserResult.Classes[0].Namespace = "";

//parserResult.Classes[0].ClassName = "";

//GeneratedFile generatedFile = generator.GenerateFile(parserResult.Classes[0], new CodeGenerationOptions 
//{ 
//    MakeClassPartial = false,
//    WrapInRegions = true,
//    IsCompactStyle = false,
//    IsMvvmToolkitStyle = false,
//    ImplementNotificationInterface = true
//});

//Console.WriteLine(generatedFile.Content);

//Console.WriteLine("\nProcessing completed.");