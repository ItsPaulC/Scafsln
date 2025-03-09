using Spectre.Console.Cli;
using Scafsln.Cli.CliCommands;

CommandApp app = new();
app.Configure(config =>
{
    config.AddCommand<InitCommand>("init-sln")
        .WithDescription("Initialize solution with common files and configurations");
    
    config.AddCommand<ConfigCommand>("config")
        .WithDescription("Show or change configuration files");
});

return app.Run(args);
