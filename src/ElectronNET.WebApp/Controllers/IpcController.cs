﻿using System.Linq;
using ElectronNET.API;
using Microsoft.AspNetCore.Mvc;

namespace ElectronNET.WebApp.Controllers;

public class IpcController : Controller
{
    public IActionResult Index()
    {
        if (HybridSupport.IsElectronActive)
        {
            Electron.IpcMain.On("async-msg", args =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                Electron.IpcMain.Send(mainWindow, "asynchronous-reply", "pong");
            });

            Electron.IpcMain.OnSync("sync-msg", args => { return "pong"; });
        }

        return View();
    }
}