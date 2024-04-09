using System.Linq;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ElectronNET.WebApp.Controllers;

public class AppSysInformationController : Controller
{
    public IActionResult Index()
    {
        if (HybridSupport.IsElectronActive)
        {
            Electron.IpcMain.On("app-info", async args =>
            {
                var appPath = await Electron.App.GetAppPathAsync();

                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                Electron.IpcMain.Send(mainWindow, "got-app-path", appPath);
            });

            Electron.IpcMain.On("sys-info", async args =>
            {
                var homePath = await Electron.App.GetPathAsync(PathName.Home);

                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                Electron.IpcMain.Send(mainWindow, "got-sys-info", homePath);
            });

            Electron.IpcMain.On("screen-info", async args =>
            {
                var display = await Electron.Screen.GetPrimaryDisplayAsync();

                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                Electron.IpcMain.Send(mainWindow, "got-screen-info", display.Size);
            });
        }

        return View();
    }
}