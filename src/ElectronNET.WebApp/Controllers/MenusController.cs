using System.Linq;
using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ElectronNET.WebApp.Controllers;

public class MenusController : Controller
{
    public IActionResult Index()
    {
        if (HybridSupport.IsElectronActive)
        {
            Electron.App.Ready += () => CreateContextMenu();

            var menu = new MenuItem[]
            {
                new()
                {
                    Label = "Edit", Submenu = new MenuItem[]
                    {
                        new() { Label = "Undo", Accelerator = "CmdOrCtrl+Z", Role = MenuRole.undo },
                        new() { Label = "Redo", Accelerator = "Shift+CmdOrCtrl+Z", Role = MenuRole.redo },
                        new() { Type = MenuType.separator },
                        new() { Label = "Cut", Accelerator = "CmdOrCtrl+X", Role = MenuRole.cut },
                        new() { Label = "Copy", Accelerator = "CmdOrCtrl+C", Role = MenuRole.copy },
                        new() { Label = "Paste", Accelerator = "CmdOrCtrl+V", Role = MenuRole.paste },
                        new() { Label = "Select All", Accelerator = "CmdOrCtrl+A", Role = MenuRole.selectall }
                    }
                },
                new()
                {
                    Label = "View", Submenu = new MenuItem[]
                    {
                        new()
                        {
                            Label = "Reload",
                            Accelerator = "CmdOrCtrl+R",
                            Click = () =>
                            {
                                // on reload, start fresh and close any old
                                // open secondary windows
                                var mainWindowId = Electron.WindowManager.BrowserWindows.ToList().First().Id;
                                Electron.WindowManager.BrowserWindows.ToList().ForEach(browserWindow =>
                                {
                                    if (browserWindow.Id != mainWindowId)
                                        browserWindow.Close();
                                    else
                                        browserWindow.Reload();
                                });
                            }
                        },
                        new()
                        {
                            Label = "Toggle Full Screen",
                            Accelerator = "CmdOrCtrl+F",
                            Click = async () =>
                            {
                                var isFullScreen =
                                    await Electron.WindowManager.BrowserWindows.First().IsFullScreenAsync();
                                Electron.WindowManager.BrowserWindows.First().SetFullScreen(!isFullScreen);
                            }
                        },
                        new()
                        {
                            Label = "Open Developer Tools",
                            Accelerator = "CmdOrCtrl+I",
                            Click = () => Electron.WindowManager.BrowserWindows.First().WebContents.OpenDevTools()
                        },
                        new()
                        {
                            Type = MenuType.separator
                        },
                        new()
                        {
                            Label = "App Menu Demo",
                            Click = async () =>
                            {
                                var options = new MessageBoxOptions(
                                    "This demo is for the Menu section, showing how to create a clickable menu item in the application menu.");
                                options.Type = MessageBoxType.info;
                                options.Title = "Application Menu Demo";
                                await Electron.Dialog.ShowMessageBoxAsync(options);
                            }
                        }
                    }
                },
                new()
                {
                    Label = "Window", Role = MenuRole.window, Submenu = new MenuItem[]
                    {
                        new() { Label = "Minimize", Accelerator = "CmdOrCtrl+M", Role = MenuRole.minimize },
                        new() { Label = "Close", Accelerator = "CmdOrCtrl+W", Role = MenuRole.close }
                    }
                },
                new()
                {
                    Label = "Help", Role = MenuRole.help, Submenu = new MenuItem[]
                    {
                        new()
                        {
                            Label = "Learn More",
                            Click = async () => await Electron.Shell.OpenExternalAsync("https://github.com/ElectronNET")
                        }
                    }
                }
            };

            Electron.Menu.SetApplicationMenu(menu);
        }

        return View();
    }

    private void CreateContextMenu()
    {
        var menu = new MenuItem[]
        {
            new()
            {
                Label = "Hello",
                Click = async () => await Electron.Dialog.ShowMessageBoxAsync("Electron.NET rocks!")
            },
            new() { Type = MenuType.separator },
            new() { Label = "Electron.NET", Type = MenuType.checkbox, Checked = true }
        };

        var mainWindow = Electron.WindowManager.BrowserWindows.FirstOrDefault();
        Electron.Menu.SetContextMenu(mainWindow, menu);

        Electron.IpcMain.On("show-context-menu", args =>
        {
            var mainWindow = Electron.WindowManager.BrowserWindows.FirstOrDefault();
            Electron.Menu.ContextMenuPopup(mainWindow);
        });
    }
}