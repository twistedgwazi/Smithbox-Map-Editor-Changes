﻿using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;

namespace StudioCore.Editors.TextureViewer;

public class TexTextureListView
{
    private TextureViewerScreen Screen;
    private TexViewSelection Selection;
    private TexFilters Filters;

    public TexTextureListView(TextureViewerScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        Filters = screen.Filters;
    }

    // <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Textures##TextureViewList");

        ImGui.Separator();

        Filters.DisplayTextureFilterSearch();

        ImGui.Separator();

        if (Selection._selectedTextureContainer != null && Selection._selectedTextureContainerKey != "")
        {
            TextureViewInfo data = Selection._selectedTextureContainer;

            ImGui.Text($"Textures");
            ImGui.Separator();

            if (data.Textures != null)
            {
                foreach (var tex in data.Textures)
                {
                    if (Filters.IsTextureFilterMatch(tex.Name))
                    {
                        // Texture row
                        if (ImGui.Selectable($@" {tex.Name}", tex.Name == Selection._selectedTextureKey))
                        {
                            Selection._selectedTextureKey = tex.Name;
                            Selection._selectedTexture = tex;
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Selection.SelectTexture)
                        {
                            Selection.SelectTexture = false;
                            Selection._selectedTextureKey = tex.Name;
                            Selection._selectedTexture = tex;
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Selection.SelectTexture = true;
                        }
                    }
                }
            }
        }

        ImGui.End();
    }
}