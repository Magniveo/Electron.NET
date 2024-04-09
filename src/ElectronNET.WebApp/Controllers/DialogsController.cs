using System.Linq;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ElectronNET.WebApp.Controllers;

public class DialogsController : Controller
{
    public IActionResult Index()
    {
        if (HybridSupport.IsElectronActive)
        {
            Electron.IpcMain.On("select-directory", async args =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new OpenDialogOptions
                {
                    Properties = new[]
                    {
                        OpenDialogProperty.openFile,
                        OpenDialogProperty.openDirectory
                    }
                };

                var files = await Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
                Electron.IpcMain.Send(mainWindow, "select-directory-reply", files);
            });

            Electron.IpcMain.On("error-dialog",
                args => { Electron.Dialog.ShowErrorBox("An Error Message", "Demonstrating an error message."); });

            Electron.IpcMain.On("information-dialog", async args =>
            {
                var options = new MessageBoxOptions("This is an information dialog. Isn't it nice?")
                {
                    Type = MessageBoxType.info,
                    Title = "Information",
                    Buttons = new[] { "Yes", "No" }
                };

                var result = await Electron.Dialog.ShowMessageBoxAsync(options);

                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                Electron.IpcMain.Send(mainWindow, "information-dialog-reply", result.Response);
            });

            Electron.IpcMain.On("save-dialog", async args =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new SaveDialogOptions
                {
                    Title = "Save an Image",
                    Filters = new FileFilter[]
                    {
                        new() { Name = "Images", Extensions = new[] { "jpg", "png", "gif" } }
                    }
                };

                var result = await Electron.Dialog.ShowSaveDialogAsync(mainWindow, options);
                Electron.IpcMain.Send(mainWindow, "save-dialog-reply", result);
            });
        }

        return View();
    }
}