using System;
using System.Linq;

namespace ElectronNET.CLI.Commands;

public class AddPackageJsonCommand{

    public static void WritePackageJson(SimpleCommandLineParser parser, string _manifest, string tempPath)
    {
         /// TODO: Make the same thing easer with native c# - we can save a tmp file in production code :)
            Console.WriteLine("Create electron-builder configuration file...");
            string _paramVersion = "Version";
            string version = null;
            string _paramElectronParams = "electron-params";
            if (parser.Arguments.ContainsKey(_paramVersion))
                version = parser.Arguments[_paramVersion][0];
            var electronParams = "";
            if (parser.Arguments.ContainsKey(_paramElectronParams))
                electronParams = parser.Arguments[_paramElectronParams][0];
            var manifestFileName = "electron.manifest.json";

            if (parser.Arguments.ContainsKey(_manifest)) manifestFileName = parser.Arguments[_manifest].First();

            ProcessHelper.CmdExecute(
                string.IsNullOrWhiteSpace(version)
                    ? $"node build-helper.js {manifestFileName}"
                    : $"node build-helper.js {manifestFileName} {version}", tempPath);

            Console.WriteLine("... done");
    }
}