﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ElectronNET.API.Entities;
using ElectronNET.API.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ElectronNET.API;

/// <summary>
///     Create native application menus and context menus.
/// </summary>
public sealed class Menu
{
    private static Menu _menu;
    private static readonly object _syncRoot = new();
    private readonly Dictionary<int, List<MenuItem>> _contextMenuItems = new();

    private readonly JsonSerializer _jsonSerializer = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly List<MenuItem> _menuItems = new();

    internal Menu()
    {
    }

    internal static Menu Instance
    {
        get
        {
            if (_menu == null)
                lock (_syncRoot)
                {
                    if (_menu == null) _menu = new Menu();
                }

            return _menu;
        }
    }

    /// <summary>
    ///     Gets the menu items.
    /// </summary>
    /// <value>
    ///     The menu items.
    /// </value>
    public IReadOnlyCollection<MenuItem> MenuItems => _menuItems.AsReadOnly();

    /// <summary>
    ///     Gets the context menu items.
    /// </summary>
    /// <value>
    ///     The context menu items.
    /// </value>
    public IReadOnlyDictionary<int, ReadOnlyCollection<MenuItem>> ContextMenuItems { get; internal set; }

    /// <summary>
    ///     Sets the application menu.
    /// </summary>
    /// <param name="menuItems">The menu items.</param>
    public void SetApplicationMenu(MenuItem[] menuItems)
    {
        _menuItems.Clear();

        menuItems.AddMenuItemsId();
        menuItems.AddSubmenuTypes();

        BridgeConnector.Socket.Emit("menu-setApplicationMenu", JArray.FromObject(menuItems, _jsonSerializer));
        _menuItems.AddRange(menuItems);

        BridgeConnector.Socket.Off("menuItemClicked");
        BridgeConnector.Socket.On("menuItemClicked", id =>
        {
            var menuItem = _menuItems.GetMenuItem(id.ToString());
            menuItem.Click?.Invoke();
        });
    }

    /// <summary>
    ///     Sets the context menu.
    /// </summary>
    /// <param name="browserWindow">The browser window.</param>
    /// <param name="menuItems">The menu items.</param>
    public void SetContextMenu(BrowserWindow browserWindow, MenuItem[] menuItems)
    {
        menuItems.AddMenuItemsId();
        menuItems.AddSubmenuTypes();

        BridgeConnector.Socket.Emit("menu-setContextMenu", browserWindow.Id,
            JArray.FromObject(menuItems, _jsonSerializer));

        if (!_contextMenuItems.ContainsKey(browserWindow.Id))
        {
            _contextMenuItems.Add(browserWindow.Id, menuItems.ToList());
            var x = _contextMenuItems.ToDictionary(kv => kv.Key, kv => kv.Value.AsReadOnly());
            ContextMenuItems = new ReadOnlyDictionary<int, ReadOnlyCollection<MenuItem>>(x);
        }

        BridgeConnector.Socket.Off("contextMenuItemClicked");
        BridgeConnector.Socket.On("contextMenuItemClicked", results =>
        {
            var id = ((JArray)results).First.ToString();
            var browserWindowId = (int)((JArray)results).Last;

            var menuItem = _contextMenuItems[browserWindowId].GetMenuItem(id);
            menuItem.Click?.Invoke();
        });
    }

    /// <summary>
    ///     Contexts the menu popup.
    /// </summary>
    /// <param name="browserWindow">The browser window.</param>
    public void ContextMenuPopup(BrowserWindow browserWindow)
    {
        BridgeConnector.Socket.Emit("menu-contextMenuPopup", browserWindow.Id);
    }
}