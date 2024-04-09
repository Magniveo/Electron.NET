using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ElectronNET.CLI.Commands.Actions;

namespace ElectronNET.CLI.Commands;

public class StartElectronCommand : ICommand
{
    public const string COMMAND_NAME = "start";

    public const string COMMAND_DESCRIPTION =
        "Start your ASP.NET Core Application with Electron, without package it as a single exe. Faster for development.";

    public const string COMMAND_ARGUMENTS = "<Path> from ASP.NET Core Project.";

    private readonly string[] _args;
    private readonly string _arguments = "args";

    private readonly string _aspCoreProjectPath = "project-path";
    private readonly string _clearCache = "clear-cache";
    private readonly string _manifest = "manifest";
    private readonly string _paramDotNetConfig = "dotnet-configuration";
    private readonly string _paramPublishReadyToRun = "PublishReadyToRun";
    private readonly string _paramPublishSingleFile = "PublishSingleFile";
    private readonly string _paramTarget = "target";

    public StartElectronCommand(string[] args)
    {
        _args = args;
    }

    public static IList<CommandOption> CommandOptions { get; set; } = new List<CommandOption>();

    public Task<bool> ExecuteAsync()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("Start Electron Desktop Application...");

            var parser = new SimpleCommandLineParser();
            parser.Parse(_args);

            var aspCoreProjectPath = "";

            if (parser.Arguments.ContainsKey(_aspCoreProjectPath))
            {
                var projectPath = parser.Arguments[_aspCoreProjectPath].First();
                if (Directory.Exists(projectPath)) aspCoreProjectPath = projectPath;
            }
            else
            {
                aspCoreProjectPath = Directory.GetCurrentDirectory();
            }

            var tempPath = Path.Combine(aspCoreProjectPath, "obj", "Host");
            if (Directory.Exists(tempPath) == false) Directory.CreateDirectory(tempPath);

            var tempBinPath = Path.Combine(tempPath, "bin");
            var resultCode = 0;

            var publishReadyToRun = "/p:PublishReadyToRun=";
            if (parser.Arguments.ContainsKey(_paramPublishReadyToRun))
                publishReadyToRun += parser.Arguments[_paramPublishReadyToRun][0];
            else
                publishReadyToRun += "true";

            var publishSingleFile = "/p:PublishSingleFile=";
            if (parser.Arguments.ContainsKey(_paramPublishSingleFile))
                publishSingleFile += parser.Arguments[_paramPublishSingleFile][0];
            else
                publishSingleFile += "true";

            // If target is specified as a command line argument, use it.
            // Format is the same as the build command.
            // If target is not specified, autodetect it.
            var platformInfo = GetTargetPlatformInformation.Do(string.Empty, string.Empty);
            if (parser.Arguments.ContainsKey(_paramTarget))
            {
                var desiredPlatform = parser.Arguments[_paramTarget][0];
                var specifiedFromCustom = string.Empty;
                if (desiredPlatform == "custom" && parser.Arguments[_paramTarget].Length > 1)
                    specifiedFromCustom = parser.Arguments[_paramTarget][1];
                platformInfo = GetTargetPlatformInformation.Do(desiredPlatform, specifiedFromCustom);
            }

            var configuration = "Debug";
            if (parser.Arguments.ContainsKey(_paramDotNetConfig))
                configuration = parser.Arguments[_paramDotNetConfig][0];

            if (parser != null && !parser.Arguments.ContainsKey("watch"))
                resultCode = ProcessHelper.CmdExecute(
                    $"dotnet publish -r {platformInfo.NetCorePublishRid} -c \"{configuration}\" --output \"{tempBinPath}\" {publishReadyToRun} {publishSingleFile} --no-self-contained",
                    aspCoreProjectPath);

            if (resultCode != 0)
            {
                Console.WriteLine("Error occurred during dotnet publish: " + resultCode);
                return false;
            }

            DeployEmbeddedElectronFiles.Do(tempPath);

            var nodeModulesDirPath = Path.Combine(tempPath, "node_modules");

            Console.WriteLine("node_modules missing in: " + nodeModulesDirPath);

            Console.WriteLine("Start npm install...");
            ProcessHelper.CmdExecute("npm install", tempPath);

            Console.WriteLine("ElectronHostHook handling started...");

            var electronhosthookDir = Path.Combine(Directory.GetCurrentDirectory(), "ElectronHostHook");

            if (Directory.Exists(electronhosthookDir))
            {
                var hosthookDir = Path.Combine(tempPath, "ElectronHostHook");
                DirectoryCopy.Do(electronhosthookDir, hosthookDir, true, new List<string> { "node_modules" });

                Console.WriteLine("Start npm install for typescript & hosthooks...");
                ProcessHelper.CmdExecute("npm install", hosthookDir);

                // ToDo: Not sure if this runs under linux/macos
                ProcessHelper.CmdExecute(@"npx tsc -p ../../ElectronHostHook", tempPath);
            }

            var arguments = "";

            if (parser.Arguments.ContainsKey(_arguments)) arguments = string.Join(' ', parser.Arguments[_arguments]);

            if (parser.Arguments.ContainsKey(_manifest))
                arguments += " --manifest=" + parser.Arguments[_manifest].First();

            if (parser.Arguments.ContainsKey(_clearCache)) arguments += " --clear-cache=true";

            if (parser.Arguments.ContainsKey("watch")) arguments += " --watch=true";

            var path = Path.Combine(tempPath, "node_modules", ".bin");
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            if (isWindows)
            {
                Console.WriteLine("Invoke electron.cmd - in dir: " + path);
                ProcessHelper.CmdExecute(@"electron.cmd ""..\..\main.js"" " + arguments, path);
            }
            else
            {
                Console.WriteLine("Invoke electron - in dir: " + path);
                ProcessHelper.CmdExecute(@"./electron ""../../main.js"" " + arguments, path);
            }

            return true;
        });
    }
}