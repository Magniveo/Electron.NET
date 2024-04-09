using System.Linq;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ElectronNET.WebApp.Controllers;

public class PdfController : Controller
{
    public IActionResult Index()
    {
        if (HybridSupport.IsElectronActive)
            Electron.IpcMain.On("print-pdf", async args =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();

                var saveOptions = new SaveDialogOptions
                {
                    Title = "Save an PDF-File",
                    DefaultPath = await Electron.App.GetPathAsync(PathName.Documents),
                    Filters = new FileFilter[]
                    {
                        new() { Name = "PDF", Extensions = new[] { "pdf" } }
                    }
                };
                var path = await Electron.Dialog.ShowSaveDialogAsync(mainWindow, saveOptions);

                if (await mainWindow.WebContents.PrintToPDFAsync(path))
                    await Electron.Shell.OpenExternalAsync("file://" + path);
                else
                    Electron.Dialog.ShowErrorBox("Error", "Failed to create pdf file.");
            });

        return View();
    }
}