﻿using ImGuiNET;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Configuration;
using StudioCore.CutsceneEditor;
using StudioCore.Editor;
using StudioCore.Editors;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.EmevdEditor;
using StudioCore.Graphics;
using StudioCore.GraphicsEditor;
using StudioCore.HavokEditor;
using StudioCore.Interface;
using StudioCore.MaterialEditor;
using StudioCore.ParticleEditor;
using StudioCore.Settings;
using StudioCore.TalkEditor;
using StudioCore.TextEditor;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core;

/// <summary>
/// Handler class that holds all of the editors and related editor state for access elsewhere.
/// </summary>
public class EditorHandler
{
    public List<EditorScreen> EditorList;
    public EditorScreen FocusedEditor;

    public MapEditorScreen MapEditor;
    public ModelEditorScreen ModelEditor;
    public TextEditorScreen TextEditor;
    public ParamEditorScreen ParamEditor;
    public TimeActEditorScreen TimeActEditor;
    public CutsceneEditorScreen CutsceneEditor;
    public GparamEditorScreen GparamEditor;
    public MaterialEditorScreen MaterialEditor;
    public ParticleEditorScreen ParticleEditor;
    public EmevdEditorScreen EmevdEditor;
    public EsdEditorScreen EsdEditor;
    public TextureViewerScreen TextureViewer;
    public HavokEditorScreen HavokEditor;

    public EditorHandler(IGraphicsContext _context)
    {
        EditorList = new();

        // Editors
        MapEditor = new MapEditorScreen(_context.Window, _context.Device);
        ModelEditor = new ModelEditorScreen(_context.Window, _context.Device);
        ParamEditor = new ParamEditorScreen(_context.Window, _context.Device);
        TextEditor = new TextEditorScreen(_context.Window, _context.Device);
        GparamEditor = new GparamEditorScreen(_context.Window, _context.Device);
        TimeActEditor = new TimeActEditorScreen(_context.Window, _context.Device);
        TextureViewer = new TextureViewerScreen(_context.Window, _context.Device);
        EmevdEditor = new EmevdEditorScreen(_context.Window, _context.Device);
        EsdEditor = new EsdEditorScreen(_context.Window, _context.Device);

        // WIP
        CutsceneEditor = new CutsceneEditorScreen(_context.Window, _context.Device);
        MaterialEditor = new MaterialEditorScreen(_context.Window, _context.Device);
        ParticleEditor = new ParticleEditorScreen(_context.Window, _context.Device);
        HavokEditor = new HavokEditorScreen(_context.Window, _context.Device);

        // Editors to Display
        if (CFG.Current.EnableMapEditor)
        {
            EditorList.Add(MapEditor);
        }

        if (CFG.Current.EnableModelEditor)
        {
            EditorList.Add(ModelEditor);
        }

        if (CFG.Current.EnableParamEditor)
        {
            EditorList.Add(ParamEditor);
        }

        if (CFG.Current.EnableTextEditor)
        {
            EditorList.Add(TextEditor);
        }

        if (CFG.Current.EnableTextureViewer)
        {
            EditorList.Add(TextureViewer);
        }

        if (CFG.Current.EnableGparamEditor)
        {
            EditorList.Add(GparamEditor);
        }

        if (CFG.Current.EnableTimeActEditor)
        {
            EditorList.Add(TimeActEditor);
        }

        if (CFG.Current.EnableEmevdEditor)
        {
            EditorList.Add(EmevdEditor);
        }

        if (CFG.Current.EnableEsdEditor)
        {
            EditorList.Add(EsdEditor);
        }

        // WIP
        if (CFG.Current.EnableCutsceneEditor)
        {
            EditorList.Add(CutsceneEditor);
        }

        if (CFG.Current.EnableHavokEditor)
        {
            EditorList.Add(HavokEditor);
        }

        if (CFG.Current.EnableMaterialEditor)
        {
            EditorList.Add(MaterialEditor);
        }

        if (CFG.Current.EnableParticleEditor)
        {
            EditorList.Add(ParticleEditor);
        }

        if (EditorList.Count > 0)
        {
            FocusedEditor = EditorList.First();
        }
    }

    public void UpdateEditors()
    {
        foreach (EditorScreen editor in EditorList)
        {
            editor.OnProjectChanged();
        }
    }

    public void SaveAllFocusedEditor()
    {
        FocusedEditor.SaveAll();
    }

    public void SaveFocusedEditor()
    {
        FocusedEditor.Save();
    }

    public void HandleEditorShortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_Save))
        {
            Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
            SaveFocusedEditor();
        }

        if (InputTracker.GetKeyDown(KeyBindings.Current.CORE_SaveAll))
        {
            Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
            SaveAllFocusedEditor();
        }
    }

    public void FileDropdown()
    {
        if (ImGui.BeginMenu("File"))
        {
            // Save
            if (ImGui.MenuItem($"Save Selected {FocusedEditor.SaveType}", KeyBindings.Current.CORE_Save.HintText))
            {
                Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                SaveFocusedEditor();
            }

            // Save All
            if (ImGui.MenuItem($"Save All Modified {FocusedEditor.SaveType}", KeyBindings.Current.CORE_SaveAll.HintText))
            {
                Smithbox.ProjectHandler.WriteProjectConfig(Smithbox.ProjectHandler.CurrentProject);
                SaveAllFocusedEditor();
            }

            ImGui.EndMenu();
        }

        ImGui.Separator();
    }
}
