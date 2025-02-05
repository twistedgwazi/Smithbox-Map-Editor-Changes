﻿using ImGuiNET;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public class MassEditHandler
{
    private MapEditorScreen Screen;

    private MapListType MapTarget;

    public List<string> MapInputs = new List<string>() { "" };

    public SelectionConditionLogic MapSelectionLogic;

    public List<string> SelectionInputs = new List<string>() { "" };

    public SelectionConditionLogic MapObjectSelectionLogic;

    public List<string> EditInputs = new List<string>() { "" };

    public MassEditLog EditLog;
    public MassEditHints Hints;
    public MassEditTools Tools;

    public MassEditHandler(MapEditorScreen screen)
    {
        Screen = screen;
        EditLog = new MassEditLog(screen, this);
        Hints = new MassEditHints(screen, this);
        Tools = new MassEditTools(screen, this);
    }

    /// <summary>
    /// Handles the initial setup for the Mass Edit tool panel
    /// </summary>
    public void Display()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);

        UIHelper.WrappedText("Map Target");
        UIHelper.ShowHoverTooltip("Determine which maps will be affected by the mass edit.");
        ImGui.Separator();

        ConfigureMapTarget();

        ImGui.Separator();
        UIHelper.WrappedText("Selection Criteria");
        UIHelper.ShowHoverTooltip("Determine which map objects will be affected by the mass edit.");
        ImGui.Separator();

        ConfigureSelection();

        ImGui.Separator();
        UIHelper.WrappedText("Edit Commands");
        UIHelper.ShowHoverTooltip("Determine which property to affect and the value change to apply for this mass edit.");
        ImGui.Separator();

        ConfigureEdit();

        if (ImGui.Button("Apply", buttonSize))
        {
            ProcessMassEdit();
        }

        ImGui.Separator();

        EditLog.DisplayButton();
        ImGui.SameLine();
        Tools.DisplayButton();

        EditLog.Display();
        Tools.Display();

        Hints.DisplayHintPopups();
    }

    /// <summary>
    /// Handles the map target section
    /// </summary>
    private void ConfigureMapTarget()
    {
        var width = ImGui.GetWindowWidth();

        //--------------
        // Actions
        //--------------
        // Documentation
        if (ImGui.Button($"{ForkAwesome.QuestionCircle}##mapTargetHintButton"))
        {
            ImGui.OpenPopup("mapTargetHint");
        }
        UIHelper.ShowHoverTooltip("View the documentation on map target commands.");

        ImGui.SameLine();

        // Add
        if (ImGui.Button($"{ForkAwesome.Plus}##mapSelectionAdd"))
        {
            MapInputs.Add("");
        }
        UIHelper.ShowHoverTooltip("Add new map selection input row.");

        ImGui.SameLine();

        // Remove
        if (MapInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{ForkAwesome.Minus}##mapSelectionRemoveDisabled"))
            {
                MapInputs.RemoveAt(MapInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added map selection input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{ForkAwesome.Minus}##mapSelectionRemove"))
            {
                MapInputs.RemoveAt(MapInputs.Count - 1);
                UIHelper.ShowHoverTooltip("Remove last added map selection input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##resetMapSelectionInput"))
        {
            MapInputs = new List<string>() { "" };
        }
        UIHelper.ShowHoverTooltip("Reset map selection input rows.");

        ImGui.SameLine();

        // Conditional Logic
        ImGui.SetNextItemWidth(width * 0.3f);
        if (ImGui.BeginCombo($"##mapSelectionCommandLogic", MapSelectionLogic.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SelectionConditionLogic)))
            {
                var curEnum = (SelectionConditionLogic)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", MapSelectionLogic == curEnum))
                {
                    MapSelectionLogic = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.ShowHoverTooltip("The logic with which to handle the selection inputs." +
            "\n\nAll must match means all the selection criteria must be true for the map object to be included." +
            "\n\nOne must match means only one of the selection criteria must be true for the map object to be included.");

        ImGui.SameLine();

        // Map List Type
        ImGui.SetNextItemWidth(width * 0.3f);
        if (ImGui.BeginCombo("##mapTargetCombo", MapTarget.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(MapListType)))
            {
                var curEnum = (MapListType)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", MapTarget == curEnum))
                {
                    MapTarget = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.ShowHoverTooltip("Determines how the map list is obtained." +
            "\n\nLocal means only currently loaded maps will be edited (that match the map selection criteria)." +
            "\n\nGlobal means all maps will be edited (that match the map selection criteria).\nWARNING: editing a large amounts of maps will cause Smithbox to hang until it is finished, which may be several minutes.");

        //--------------
        // Map Inputs
        //--------------
        for (int i = 0; i < MapInputs.Count; i++)
        {
            var curCommand = MapInputs[i];
            var curText = curCommand;

            ImGui.SetNextItemWidth(width);
            if (ImGui.InputText($"##mapSelectionInput{i}", ref curText, 255))
            {
                MapInputs[i] = curText;
            }
            UIHelper.ShowHoverTooltip("The map selection command to process.");
        }
    }

    /// <summary>
    /// Handles the selection criteria section
    /// </summary>
    private void ConfigureSelection()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width * 0.32f, 24);

        //--------------
        // Actions
        //--------------
        // Documentation
        if (ImGui.Button($"{ForkAwesome.QuestionCircle}##selectionHintButton"))
        {
            ImGui.OpenPopup("selectionInputHint");
        }
        UIHelper.ShowHoverTooltip("View documentation on selection commands.");

        ImGui.SameLine();

        // Add
        if (ImGui.Button($"{ForkAwesome.Plus}##selectionAdd"))
        {
            SelectionInputs.Add("");
        }
        UIHelper.ShowHoverTooltip("Add new selection input row.");

        ImGui.SameLine();

        // Remove
        if (SelectionInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{ForkAwesome.Minus}##selectionRemoveDisabled"))
            {
                SelectionInputs.RemoveAt(SelectionInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added selection input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{ForkAwesome.Minus}##selectionRemove"))
            {
                SelectionInputs.RemoveAt(SelectionInputs.Count - 1);
                UIHelper.ShowHoverTooltip("Remove last added selection input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##resetSelectionInput"))
        {
            SelectionInputs = new List<string>() { "" };
        }
        UIHelper.ShowHoverTooltip("Reset selection input rows.");

        ImGui.SameLine();

        // Conditional Logic
        ImGui.SetNextItemWidth(width * 0.3f);
        if (ImGui.BeginCombo($"##selectionCommandLogic", MapObjectSelectionLogic.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SelectionConditionLogic)))
            {
                var curEnum = (SelectionConditionLogic)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", MapObjectSelectionLogic == curEnum))
                {
                    MapObjectSelectionLogic = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.ShowHoverTooltip("The logic with which to handle the selection inputs." +
            "\n\nAll must match means all the selection criteria must be true for the map object to be included." +
            "\n\nOne must match means only one of the selection criteria must be true for the map object to be included.");

        //--------------
        // Selection Inputs
        //--------------
        for (int i = 0; i < SelectionInputs.Count; i++)
        {
            var curCommand = SelectionInputs[i];
            var curText = curCommand;

            ImGui.SetNextItemWidth(width);
            if (ImGui.InputText($"##selectionInput{i}", ref curText, 255))
            {
                SelectionInputs[i] = curText;
            }
            UIHelper.ShowHoverTooltip("The selection command to process.");
        }

    }

    /// <summary>
    /// Handles the edit section
    /// </summary>
    private void ConfigureEdit()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width * 0.32f, 24);

        //--------------
        // Actions Inputs
        //--------------
        // Documentation
        if (ImGui.Button($"{ForkAwesome.QuestionCircle}##editHintButton"))
        {
            ImGui.OpenPopup("editInputHint");
        }
        UIHelper.ShowHoverTooltip("View documentation on edit commands.");

        ImGui.SameLine();

        // Add
        if (ImGui.Button($"{ForkAwesome.Plus}##editAdd"))
        {
            EditInputs.Add("");
        }
        UIHelper.ShowHoverTooltip("Add edit input row.");

        ImGui.SameLine();

        // Remove
        if (EditInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{ForkAwesome.Minus}##editRemoveDisabled"))
            {
                EditInputs.RemoveAt(EditInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added edit input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{ForkAwesome.Minus}##editRemove"))
            {
                EditInputs.RemoveAt(EditInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added edit input row.");
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##resetEditInputs"))
        {
            EditInputs = new List<string>() { "" };
        }
        UIHelper.ShowHoverTooltip("Reset edit input rows.");

        //--------------
        // Edit Inputs
        //--------------
        for (int i = 0; i < EditInputs.Count; i++)
        {
            var curCommand = EditInputs[i];
            var curText = curCommand;

            ImGui.SetNextItemWidth(width);
            if (ImGui.InputText($"##editInput{i}", ref curText, 255))
            {
                EditInputs[i] = curText;
            }
            UIHelper.ShowHoverTooltip("The edit command to process.");
        }
    }

    
    /// <summary>
    /// Handles the map-level part of the Mass Edit
    /// </summary>
    private void ProcessMassEdit()
    {
        var selection = Smithbox.EditorHandler.MapEditor.Selection;
        var listView = Smithbox.EditorHandler.MapEditor.MapListView;
        var universe = Smithbox.EditorHandler.MapEditor.Universe;

        List<MapActionGroup> actionGroups = new List<MapActionGroup>();

        // Clear selection before applying edits, to ensure the properties view doesn't interfere.
        selection.ClearSelection();

        // Get filtered list of maps
        var mapList = MapLocator.GetFullMapList();
        var availableList = new List<string>();
        foreach (var entry in mapList)
        {
            if (IsValidMap(entry))
            {
                availableList.Add(entry);
            }
        }

        // Local
        if (MapTarget is MapListType.Local)
        {
            if (universe.LoadedObjectContainers.Count > 0)
            {
                var maps = universe.LoadedObjectContainers
                        .Where(k => k.Key is not null)
                        .OrderBy(k => k.Key);

                foreach (var entry in maps)
                {
                    if (availableList.Contains(entry.Key))
                    {
                        if (listView.ContentViews.ContainsKey(entry.Key))
                        {
                            var curView = listView.ContentViews[entry.Key];

                            var actionList = ProcessSelectionCriteria(curView);

                            if (actionList.Count > 0)
                                actionGroups.Add(new MapActionGroup(entry.Key, actionList));
                        }
                    }
                }
            }

            if (actionGroups.Count > 0)
            {
                EditLog.UpdateLogSource(actionGroups);
                var compoundAction = new MapActionGroupCompoundAction(actionGroups);
                Smithbox.EditorHandler.MapEditor.EditorActionManager.ExecuteAction(compoundAction);
            }
            else
            {
                TaskLogs.AddLog("MSB mass edit could not be applied.");
            }
        }

        // Global
        if (MapTarget is MapListType.Global)
        {
            Universe.IsRendering = false;
            Universe.IgnoreExceptions = true;

            // Load all maps
            foreach (var entry in availableList)
            {
                universe.LoadMap(entry, false, true);
                Smithbox.EditorHandler.MapEditor.MapListView.SignalLoad(entry);
            }

            // Process each map
            if (universe.LoadedObjectContainers.Count > 0)
            {
                var maps = universe.LoadedObjectContainers
                        .Where(k => k.Key is not null)
                        .OrderBy(k => k.Key);

                foreach (var entry in maps)
                {
                    if (listView.ContentViews.ContainsKey(entry.Key))
                    {
                        var curView = listView.ContentViews[entry.Key];

                        var actionList = ProcessSelectionCriteria(curView);

                        if (actionList.Count > 0)
                            actionGroups.Add(new MapActionGroup(entry.Key, actionList));
                    }
                }
            }

            if (actionGroups.Count > 0)
            {
                EditLog.UpdateLogSource(actionGroups);
                var compoundAction = new MapActionGroupCompoundAction(actionGroups);
                Smithbox.EditorHandler.MapEditor.EditorActionManager.ExecuteAction(compoundAction);
            }
            else
            {
                TaskLogs.AddLog("MSB mass edit could not be applied.");
            }

            universe.SaveAllMaps();

            //universe.UnloadAllMaps();
            foreach (var entry in availableList)
            {
                Smithbox.EditorHandler.MapEditor.MapListView.SignalUnload(entry);
            }

            Universe.IsRendering = true;
            Universe.IgnoreExceptions = false;
        }
    }
    /// <summary>
    /// Handles the selection filtering for map objects
    /// </summary>
    private bool IsValidMap(string mapID)
    {
        var isValid = true;

        if (MapSelectionLogic is SelectionConditionLogic.OR)
        {
            isValid = false;
        }

        bool[] partTruth = new bool[MapInputs.Count];

        for (int i = 0; i < MapInputs.Count; i++)
        {
            var cmd = MapInputs[i];

            // Blank will match for everything
            if (cmd == "")
                partTruth[i] = true;

            if (cmd.Contains("exclude:"))
            {
                var input = cmd.Replace("exclude:", "").Trim().ToLower();

                if (mapID.Contains(input))
                {
                    partTruth[i] = false;
                }
            }
            // Default to name filter if no explicit command is used
            else
            {
                var input = cmd.Trim().ToLower();

                if (mapID.Contains(input))
                {
                    partTruth[i] = true;
                }
            }
        }

        foreach (bool entry in partTruth)
        {
            if (MapSelectionLogic is SelectionConditionLogic.AND)
            {
                if (!entry)
                    isValid = false;
            }
            else if (MapSelectionLogic is SelectionConditionLogic.OR)
            {
                if (entry)
                    isValid = true;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Handles the selection criteria process
    /// </summary>
    private List<ViewportAction> ProcessSelectionCriteria(MapContentView curView)
    {
        List<ViewportAction> actions = new List<ViewportAction>();

        if (curView.Container != null)
        {
            foreach (var entry in curView.Container.Objects)
            {
                if (entry is MsbEntity mEnt)
                {
                    if (IsValidMapObject(curView, mEnt))
                    {
                        var actionList = ProcessEditCommands(curView, mEnt);
                        foreach (var actionEntry in actionList)
                        {
                            actions.Add(actionEntry);
                        }
                    }
                }
            }
        }

        return actions;
    }

    /// <summary>
    /// Handles the selection filtering for map objects
    /// </summary>
    private bool IsValidMapObject(MapContentView curView, MsbEntity mEnt)
    {
        var isValid = true;

        if (MapObjectSelectionLogic is SelectionConditionLogic.OR)
        {
            isValid = false;
        }

        bool[] partTruth = new bool[SelectionInputs.Count];

        for (int i = 0; i < SelectionInputs.Count; i++)
        {
            var cmd = SelectionInputs[i];

            if (cmd.Contains("prop:"))
            {
                partTruth[i] = PropertyValueFilter(curView, mEnt, cmd);
            }
            // Default to name filter if no explicit command is used
            else
            {
                partTruth[i] = PropertyNameFilter(curView, mEnt, cmd);
            }
        }

        foreach (bool entry in partTruth)
        {
            if (MapObjectSelectionLogic is SelectionConditionLogic.AND)
            {
                if (!entry)
                    isValid = false;
            }
            else if (MapObjectSelectionLogic is SelectionConditionLogic.OR)
            {
                if (entry)
                    isValid = true;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Handles the selection filtering for map objects based on name
    /// </summary>
    private bool PropertyNameFilter(MapContentView view, Entity curEnt, string cmd)
    {
        bool isValid = false;

        if (curEnt == null)
            return isValid;

        if (curEnt.Name == null)
            return isValid;

        if (curEnt.CachedAliasName == null)
            return isValid;

        var entName = curEnt.Name.Trim().ToLower();
        var aliasName = curEnt.CachedAliasName.Trim().ToLower();

        var input = cmd.Replace("name:", "").Trim().ToLower();

        if (entName != null)
        {
            if (entName != "" && entName.Contains(input))
                isValid = true;
        }

        if (aliasName != null)
        {
            if (aliasName != "" && aliasName.Contains(input))
                isValid = true;
        }

        return isValid;
    }

    /// <summary>
    /// Handles the selection filtering for map objects based on property value
    /// </summary>
    private bool PropertyValueFilter(MapContentView view, Entity curEnt, string cmd)
    {
        bool isValid = false;

        var input = cmd.Replace("prop:", "");

        var segments = input.Split(" ");
        if (segments.Length >= 3)
        {
            var prop = segments[0];
            var compare = segments[1].Trim().ToLower();
            var targetValue = segments[2].Trim().ToLower();

            var index = -1;

            if (prop.Contains("[") && prop.Contains("]"))
            {
                var match = new Regex(@"\[(.*?)\]").Match(prop);

                if (match.Success)
                {
                    var val = match.Value.Replace("[", "").Replace("]", "");

                    int.TryParse(val, out index);
                    prop = prop.Replace($"{match.Value}", "");
                }
            }

            Type targetObj = curEnt.WrappedObject.GetType();

            PropertyInfo targetProp = null;
            object targetProp_Value = null;

            // Get the actual property from within the array
            if (index != -1)
            {
                targetProp = targetObj.GetProperty(prop);

                if (targetProp != null)
                {
                    object collection = targetProp.GetValue(curEnt.WrappedObject);

                    if (collection is Array arr && index >= 0 && index < arr.Length)
                    {
                        targetProp_Value = arr.GetValue(index);
                    }
                    else if (collection is IList list && index >= 0 && index < list.Count)
                    {
                        targetProp_Value = list[index];
                    }
                }
            }
            else
            {
                targetProp = curEnt.GetProperty(prop);
                targetProp_Value = curEnt.GetPropertyValue(prop);
            }

            if (targetProp != null && targetProp_Value != null)
            {
                var valueType = targetProp_Value.GetType();

                // Do numeric comparison if compare str is < or >
                if (MassEditUtils.IsNumericType(valueType) && compare != "=")
                {
                    isValid = PerformNumericComparison(compare, targetValue, targetProp_Value, valueType);
                }
                // Otherwise do string comparison
                else
                {
                    if (targetValue == $"{targetProp_Value}")
                    {
                        isValid = true;
                    }
                }
            }
        }


        return isValid;
    }


    /// <summary>
    /// Performs the mathematical condition check
    /// </summary>
    private bool PerformNumericComparison(string comparator, string targetVal, object propValue, Type valueType)
    {
        // LONG
        if (valueType == typeof(long))
        {
            var tPropValue = (long)propValue;
            var tTargetValue = (long)propValue;

            var res = long.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // UINT
        if (valueType == typeof(uint))
        {
            var tPropValue = (uint)propValue;
            var tTargetValue = (uint)propValue;

            var res = uint.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // INT
        if (valueType == typeof(int))
        {
            var tPropValue = (int)propValue;
            var tTargetValue = (int)propValue;

            var res = int.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // USHORT
        if (valueType == typeof(ushort))
        {
            var tPropValue = (ushort)propValue;
            var tTargetValue = (ushort)propValue;

            var res = ushort.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // SHORT
        if (valueType == typeof(short))
        {
            var tPropValue = (short)propValue;
            var tTargetValue = (short)propValue;

            var res = short.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // SBYTE
        if (valueType == typeof(sbyte))
        {
            var tPropValue = (sbyte)propValue;
            var tTargetValue = (sbyte)propValue;

            var res = sbyte.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // BYTE
        if (valueType == typeof(byte))
        {
            var tPropValue = (byte)propValue;
            var tTargetValue = (byte)propValue;

            var res = byte.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // FLOAT
        if (valueType == typeof(float))
        {
            var tPropValue = (float)propValue;
            var tTargetValue = (float)propValue;

            var res = float.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // VECTOR 3
        if (valueType == typeof(Vector3))
        {
            var tPropValue = (Vector3)propValue;
            var tTargetValue = (Vector3)propValue;

            var parts = targetVal.Split(",");
            if (parts.Length >= 2)
            {
                var x = parts[0];
                var y = parts[1];
                var z = parts[2];

                float tX = 0.0f;
                float tY = 0.0f;
                float tZ = 0.0f;

                var resX = float.TryParse(x, out tX);
                var resY = float.TryParse(y, out tY);
                var resZ = float.TryParse(z, out tZ);

                if (resX && resY && resZ)
                {
                    if (comparator == "<")
                    {
                        if (tPropValue.X < tX && tPropValue.Y < tY && tPropValue.Z < tZ)
                            return true;
                    }
                    if (comparator == ">")
                    {
                        if (tPropValue.X > tX && tPropValue.Y > tY && tPropValue.Z > tZ)
                            return true;
                    }
                }
            }
        }


        return false;
    }

    /// <summary>
    /// Handles the edit command process
    /// </summary>
    private List<ViewportAction> ProcessEditCommands(MapContentView curView, MsbEntity mEnt)
    {
        var editCommands = EditInputs;

        List<ViewportAction> actions = new();

        for (int i = 0; i < editCommands.Count; i++)
        {
            var cmd = editCommands[i];

            if (cmd.Contains("random:"))
            {

            }
            // Default to <prop> <operation> <value>
            else
            {
                var action = PropertyValueOperation(curView, mEnt, cmd);
                if (action != null)
                    actions.Add(action);
            }
        }

        return actions;
    }

    /// <summary>
    /// Handles the property value operation edits
    /// TODO: adjust how this is done so we don't need to duplicate the operation logic so much
    /// </summary>
    private ViewportAction PropertyValueOperation(MapContentView curView, MsbEntity curEnt, string cmd)
    {
        var input = cmd.Replace("prop:", "");

        var segments = input.Split(" ");
        if (segments.Length >= 3)
        {
            var prop = segments[0];
            var compare = segments[1].Trim().ToLower();
            var newValue = segments[2].Trim().ToLower();

            var index = -1;

            if (prop.Contains("[") && prop.Contains("]"))
            {
                var match = new Regex(@"\[(.*?)\]").Match(prop);

                if (match.Success)
                {
                    var val = match.Value.Replace("[", "").Replace("]", "");

                    int.TryParse(val, out index);
                    prop = prop.Replace($"{match.Value}", "");
                }
            }

            Type targetObj = curEnt.WrappedObject.GetType();

            PropertyInfo targetProp = null;
            object targetProp_Value = null;

            // Get the actual property from within the array
            if (index != -1)
            {
                targetProp = targetObj.GetProperty(prop);

                if (targetProp != null)
                {
                    object collection = targetProp.GetValue(curEnt.WrappedObject);

                    if (collection is Array arr && index >= 0 && index < arr.Length)
                    {
                        targetProp_Value = arr.GetValue(index);

                        if (targetProp_Value == null)
                            return null;

                        var valueType = targetProp_Value.GetType();

                        // If numeric operation is not supported, force set operation
                        if (!MassEditUtils.IsNumericType(valueType))
                        {
                            compare = "=";
                        }

                        // LONG
                        if (valueType == typeof(long))
                        {
                            long tNewValue = 0;
                            long tExistingValue = (long)targetProp_Value;

                            var res = long.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;
                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // UINT
                        if (valueType == typeof(uint))
                        {
                            uint tNewValue = 0;
                            uint tExistingValue = (uint)targetProp_Value;

                            var res = uint.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // INT
                        if (valueType == typeof(int))
                        {
                            int tNewValue = 0;
                            int tExistingValue = (int)targetProp_Value;

                            var res = int.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // USHORT
                        if (valueType == typeof(ushort))
                        {
                            ushort tNewValue = 0;
                            ushort tExistingValue = (ushort)targetProp_Value;

                            var res = ushort.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // SHORT
                        if (valueType == typeof(short))
                        {
                            short tNewValue = 0;
                            short tExistingValue = (short)targetProp_Value;

                            var res = short.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // SBYTE
                        if (valueType == typeof(sbyte))
                        {
                            sbyte tNewValue = 0;
                            sbyte tExistingValue = (sbyte)targetProp_Value;

                            var res = sbyte.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // BYTE
                        if (valueType == typeof(byte))
                        {
                            byte tNewValue = 0;
                            byte tExistingValue = (byte)targetProp_Value;

                            var res = byte.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // FLOAT
                        if (valueType == typeof(float))
                        {
                            float tNewValue = 0;
                            float tExistingValue = (float)targetProp_Value;

                            var res = float.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                    }
                }
            }
            else
            {
                targetProp = curEnt.GetProperty(prop);
                targetProp_Value = curEnt.GetPropertyValue(prop);

                if (targetProp_Value == null)
                    return null;

                var valueType = targetProp_Value.GetType();

                // If numeric operation is not supported, force set operation
                if (!MassEditUtils.IsNumericType(valueType))
                {
                    compare = "=";
                }

                // LONG
                if (valueType == typeof(long))
                {
                    long tNewValue = 0;
                    long tExistingValue = (long)targetProp_Value;

                    var res = long.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;
                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // UINT
                if (valueType == typeof(uint))
                {
                    uint tNewValue = 0;
                    uint tExistingValue = (uint)targetProp_Value;

                    var res = uint.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // INT
                if (valueType == typeof(int))
                {
                    int tNewValue = 0;
                    int tExistingValue = (int)targetProp_Value;

                    var res = int.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // USHORT
                if (valueType == typeof(ushort))
                {
                    ushort tNewValue = 0;
                    ushort tExistingValue = (ushort)targetProp_Value;

                    var res = ushort.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // SHORT
                if (valueType == typeof(short))
                {
                    short tNewValue = 0;
                    short tExistingValue = (short)targetProp_Value;

                    var res = short.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // SBYTE
                if (valueType == typeof(sbyte))
                {
                    sbyte tNewValue = 0;
                    sbyte tExistingValue = (sbyte)targetProp_Value;

                    var res = sbyte.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // BYTE
                if (valueType == typeof(byte))
                {
                    byte tNewValue = 0;
                    byte tExistingValue = (byte)targetProp_Value;

                    var res = byte.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // FLOAT
                if (valueType == typeof(float))
                {
                    float tNewValue = 0;
                    float tExistingValue = (float)targetProp_Value;

                    var res = float.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
            }
        }

        return null;
    }

}

