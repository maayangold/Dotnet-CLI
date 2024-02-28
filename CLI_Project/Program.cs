using System.CommandLine;



string[] allLanguages = { "c#", "c++", "java", "html", "js", "python", "sql" };
//---option
var languageOption = new Option<string>("--language", "File Languages that must be one of the values of a static list.")
{
    IsRequired = true,
    AllowMultipleArgumentsPerToken = true
};
//.FromAmong("c#", "c++", "java", "html", "js", "python", "sql");

languageOption.AddAlias("-l");

var outputOption = new Option<FileInfo>("--output", "File path or file name")
{ IsRequired = true };
outputOption.AddAlias("-o");

var noteOption = new Option<bool>("--note", "add code sorce in bundle ");
noteOption.AddAlias("-n");
noteOption.SetDefaultValue(false);
var sortOption = new Option<string>(name: "--sort",
       description: "Sort file by AB of the file name or file type",
    getDefaultValue: () => "name");
sortOption.AddAlias("-s");

var removeEmptyLinesOption = new Option<bool>("--remove-empty-lines", "Sort file by AB of the file name or file type");
removeEmptyLinesOption.AddAlias("-re");

var authorOption = new Option<string>("--author", "add  author's name in bundle");
authorOption.AddAlias("-a");

//---subcommand
var bundleCommand = new Command("bundle", "Bundle code files to a signle file");
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);


bundleCommand.SetHandler((language, output, note, sort, removeEmptyLines, author) =>
{
    try
    {
        List<string> languages = LanguageForBundle(language, allLanguages);
        string currentPath = Directory.GetCurrentDirectory();
        List<string> files = Directory.GetFiles(currentPath, "", SearchOption.AllDirectories).Where(file => languages.Contains(Path.GetExtension(file)) &&
        !file.Contains("bin") && !file.Contains("Debug") && !file.Contains("obj") && !file.Contains("vs") && !file.Contains("node_modules") && !file.Contains(".git")).ToList();
        if (sort == null || sort == "name")
            files.Sort();
        if (sort == "type")
            files.OrderBy(file => Path.GetExtension(file));

        // The using statement also closes the StreamReader.

        using (StreamWriter bundleFile = new StreamWriter(output.FullName))
        {
            if (author != null)
                bundleFile.WriteLine("Author of file: " + author);
            if (note)
                bundleFile.WriteLine("File sorce: " + currentPath + " Name of File: " + new DirectoryInfo(currentPath).Name);

            foreach (var file in files)
            {
                using (StreamReader sr = new StreamReader(file))
                {
                    string line = sr.ReadLine();
                    while (line != null)
                    {
                        if (!removeEmptyLines || line.Length > 0)
                            bundleFile.WriteLine(line);
                        line = sr.ReadLine();
                    }

                }
            }
        }
        WriteSuccess("Success Bundling file");

    }



    catch (DirectoryNotFoundException e)
    {
        Console.WriteLine("Error: The file or directory cannot be found.");
    }


    catch (Exception e)
    {

        WriteError("Error: The file could not be read:");
        Console.WriteLine(e.Message);
    }



}, languageOption, outputOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);

var createRspCommand = new Command("create-rsp", "create response file in order to save time in writing commands and arguments");

createRspCommand.SetHandler(() =>
{
    string name = "ResponceFile", language, output, note, sort, removeEmptyLines, author;
    bool isRequierd = false;
    String[] yayOrNay = { "y", "Y","n", "N" };

    name = WriteQuestion("Enter response-file name: ");
    do
    {
        if (isRequierd)//print just in the second entering
            Console.WriteLine("Required field!");
        language = WriteQuestion("Enter file language you want to bundle or enter all to include all languages");
        isRequierd = true;
    }
    while (language == null);
    isRequierd = false;
    do
    {
        if (isRequierd)//print just in the second entering
            Console.WriteLine("Required field!");
        output = WriteQuestion("Enter path for bundle to: ");
        isRequierd = true;
    }
    while (output != null);
    do { note = WriteQuestion("Do you want to add note in the file? "); }
    while (!yayOrNay.Contains(note)) ;
    do
    {
        sort = WriteQuestion("Do you want to sort bundle file by type? (y/n)");
    }

    while (!yayOrNay.Contains(sort));
    do
    {
        removeEmptyLines = WriteQuestion("Do you want to remove empty lines? (y/n)");
    }

    while (!yayOrNay.Contains(removeEmptyLines));
  
  

    author = WriteQuestion("Autor name? ");
    using (StreamWriter rsp = new StreamWriter("@" + name + ".rsp"))
    {
        rsp.WriteLine("bundle");
        rsp.WriteLine($"-l {language}");
        rsp.WriteLine($"-o {output}");
        if (note == "y" || note == "Y")
            rsp.WriteLine("-n");
        if (sort == "y" || sort == "Y")
            rsp.WriteLine($"-s type");
        else
            rsp.WriteLine($"-s name");
        if (removeEmptyLines == "y" || removeEmptyLines == "Y")
            rsp.WriteLine($"-re");
        if (author != null)
            rsp.WriteLine($"-a {author}");
    }
    WriteSuccess($"Response file @{name}.rsp created succesfully");


});


//---RootCommand
var rootCommand = new RootCommand("Root Command for file bundle");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args);


static List<string> LanguageForBundle(string language, string[] Languages)
{
    List<string> FilesExtensions = new List<string>();
    foreach (var l in language.Split(' '))
    {
        if (!Languages.Contains(l))
            Console.WriteLine($" Unrecognized  language '{l}'. ");
        else
            if (!FilesExtensions.Contains(l))
            switch (l)
            {
                case "all": FilesExtensions.AddRange(new[] { ".cs", ".cpp", ".java", ".html", ".js", ".py", ".sql" }); break;
                case "c#": FilesExtensions.Add(".cs"); break;
                case "c": FilesExtensions.Add(".cpp"); break;
                case "python": FilesExtensions.Add(".py"); break;
                default: FilesExtensions.Add('.' + l); break;
            }


    }
    return FilesExtensions;
}
static void WriteError(string error)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(error);
    Console.ForegroundColor = ConsoleColor.White;
}
static void WriteSuccess(string success)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine(success);
    Console.ForegroundColor = ConsoleColor.White;
}
static string WriteQuestion(string qustion)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write(qustion);
    Console.ForegroundColor = ConsoleColor.White;

    string answer = Console.ReadLine();
    Console.WriteLine();
    return answer;

}
