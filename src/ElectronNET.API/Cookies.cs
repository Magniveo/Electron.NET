﻿using System;
using System.Threading.Tasks;
using ElectronNET.API.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace ElectronNET.API;

/// <summary>
///     Query and modify a session's cookies.
/// </summary>
public class Cookies
{
    private readonly JsonSerializer _jsonSerializer = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore
    };

    internal Cookies(int id)
    {
        Id = id;
    }

    /// <summary>
    ///     Gets the identifier.
    /// </summary>
    /// <value>
    ///     The identifier.
    /// </value>
    public int Id { get; }

    /// <summary>
    ///     Emitted when a cookie is changed because it was added, edited, removed, or expired.
    /// </summary>
    public event Action<Cookie, CookieChangedCause, bool> OnChanged
    {
        add
        {
            if (_changed == null)
            {
                BridgeConnector.Socket.On("webContents-session-cookies-changed" + Id, args =>
                {
                    var cookie = ((JArray)args)[0].ToObject<Cookie>();
                    var cause = ((JArray)args)[1].ToObject<CookieChangedCause>();
                    var removed = ((JArray)args)[2].ToObject<bool>();
                    _changed(cookie, cause, removed);
                });

                BridgeConnector.Socket.Emit("register-webContents-session-cookies-changed", Id);
            }

            _changed += value;
        }
        remove
        {
            _changed -= value;

            if (_changed == null)
                BridgeConnector.Socket.Off("webContents-session-cookies-changed" + Id);
        }
    }

    private event Action<Cookie, CookieChangedCause, bool> _changed;

    /// <summary>
    ///     Sends a request to get all cookies matching filter, and resolves a callack with the response.
    /// </summary>
    /// <param name="filter">
    /// </param>
    /// <returns>A task which resolves an array of cookie objects.</returns>
    public Task<Cookie[]> GetAsync(CookieFilter filter)
    {
        var taskCompletionSource = new TaskCompletionSource<Cookie[]>();
        var guid = Guid.NewGuid().ToString();

        BridgeConnector.Socket.On("webContents-session-cookies-get-completed" + guid, cookies =>
        {
            var result = ((JArray)cookies).ToObject<Cookie[]>();

            BridgeConnector.Socket.Off("webContents-session-cookies-get-completed" + guid);
            taskCompletionSource.SetResult(result);
        });

        BridgeConnector.Socket.Emit("webContents-session-cookies-get", Id, JObject.FromObject(filter, _jsonSerializer),
            guid);

        return taskCompletionSource.Task;
    }

    /// <summary>
    /// </summary>
    /// <param name="details"></param>
    /// <returns></returns>
    public Task SetAsync(CookieDetails details)
    {
        var taskCompletionSource = new TaskCompletionSource<object>();
        var guid = Guid.NewGuid().ToString();

        BridgeConnector.Socket.On("webContents-session-cookies-set-completed" + guid, () =>
        {
            BridgeConnector.Socket.Off("webContents-session-cookies-set-completed" + guid);
            taskCompletionSource.SetResult(null);
        });

        BridgeConnector.Socket.Emit("webContents-session-cookies-set", Id, JObject.FromObject(details, _jsonSerializer),
            guid);

        return taskCompletionSource.Task;
    }

    /// <summary>
    ///     Removes the cookies matching url and name
    /// </summary>
    /// <param name="url">The URL associated with the cookie.</param>
    /// <param name="name">The name of cookie to remove.</param>
    /// <returns>A task which resolves when the cookie has been removed</returns>
    public Task RemoveAsync(string url, string name)
    {
        var taskCompletionSource = new TaskCompletionSource<object>();
        var guid = Guid.NewGuid().ToString();

        BridgeConnector.Socket.On("webContents-session-cookies-remove-completed" + guid, () =>
        {
            BridgeConnector.Socket.Off("webContents-session-cookies-remove-completed" + guid);
            taskCompletionSource.SetResult(null);
        });

        BridgeConnector.Socket.Emit("webContents-session-cookies-remove", Id, url, name, guid);

        return taskCompletionSource.Task;
    }

    /// <summary>
    ///     Writes any unwritten cookies data to disk.
    /// </summary>
    /// <returns>A task which resolves when the cookie store has been flushed</returns>
    public Task FlushStoreAsync()
    {
        var taskCompletionSource = new TaskCompletionSource<object>();
        var guid = Guid.NewGuid().ToString();

        BridgeConnector.Socket.On("webContents-session-cookies-flushStore-completed" + guid, () =>
        {
            BridgeConnector.Socket.Off("webContents-session-cookies-flushStore-completed" + guid);
            taskCompletionSource.SetResult(null);
        });

        BridgeConnector.Socket.Emit("webContents-session-cookies-flushStore", Id, guid);

        return taskCompletionSource.Task;
    }
}