using BurnSystems.CommandLine.ByAttributes;

namespace logviewer
{
    public class ProgramArguments
    {
        [NamedArgument(IsRequired = false, ShortName = 'k', HelpText = "App key to encrypt")]
        public string ProjectKey { get; set; }

        [NamedArgument(IsRequired = false, ShortName = 'r', HelpText = "Result file")]
        public string ResultFile { get; set; }
    }
}