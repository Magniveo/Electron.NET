using System;
using System.Collections.Generic;
using ElectronNET.API.Entities;

namespace ElectronNET.API.Extensions;

internal static class ThumbarButtonExtensions
{
    public static ThumbarButton[] AddThumbarButtonsId(this ThumbarButton[] thumbarButtons)
    {
        for (var index = 0; index < thumbarButtons.Length; index++)
        {
            var thumbarButton = thumbarButtons[index];

            if (string.IsNullOrEmpty(thumbarButton.Id)) thumbarButton.Id = Guid.NewGuid().ToString();
        }

        return thumbarButtons;
    }

    public static ThumbarButton GetThumbarButton(this List<ThumbarButton> thumbarButtons, string id)
    {
        var result = new ThumbarButton("");

        foreach (var item in thumbarButtons)
            if (item.Id == id)
                result = item;

        return result;
    }
}