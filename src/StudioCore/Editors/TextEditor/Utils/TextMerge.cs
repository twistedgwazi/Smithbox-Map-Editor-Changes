﻿using ImGuiNET;
using Octokit;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Utils;

public static class TextMerge
{
    public static string TargetProjectDir = "";

    public static bool ReplaceModifiedRows = true;

    public static void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        UIHelper.WrappedText("Use this to merge a target project's text files into your current project.");
        UIHelper.WrappedText("");
        UIHelper.WrappedText("Merging will bring all unique text from the target project into your project.\nIncludes modified text if enabled.");
        UIHelper.WrappedText("");

        if (ImGui.BeginTable($"textMergeTable", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // Row 1
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Target Project");
            UIHelper.ShowHoverTooltip("The project you want to merge text from.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(ImGui.GetColumnWidth() * 0.75f); 
            ImGui.InputText("##targetProjectDir", ref TargetProjectDir, 255);
            ImGui.SameLine();
            if (ImGui.Button($@"{ForkAwesome.FileO}"))
            {
                if (PlatformUtils.Instance.OpenFolderDialog("Select project directory...", out var path))
                {
                    TargetProjectDir = path;
                }
            }

            // Row 2
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Replace Modified Entries");

            ImGui.TableSetColumnIndex(1);

            ImGui.Checkbox("##replaceModified", ref ReplaceModifiedRows);
            UIHelper.ShowHoverTooltip("If enabled, then modified rows from the target will overwrite existing rows in our project. If not, then they will be ignored, and only unique rows will be merged.");

            ImGui.EndTable();
        }

        if(ImGui.Button("Merge", defaultButtonSize))
        {
            ApplyMerge();
        }
        UIHelper.ShowHoverTooltip("May hang whilst processing the merge.");
    }

    private static void ApplyMerge()
    {
        if(TargetProjectDir == "")
        {
            TaskLogs.AddLog("Specified target directory is invalid!");
            return;
        }

        // Load target project text files in
        if (Directory.Exists(TargetProjectDir))
        {
            TextBank.LoadTargetTextFiles(TargetProjectDir);
        }

        StartFmgMerge();
    }

    private static void StartFmgMerge()
    {
        /// Filter through containers, only process FMGs for each if they match
        foreach (var entry in TextBank.FmgBank)
        {
            var primaryKey = Path.GetFileName(entry.Key);
            var currentContainer = entry.Value;

            foreach (var pEntry in TextBank.TargetFmgBank)
            {
                var targetKey = Path.GetFileName(pEntry.Key);
                var targetContainer = entry.Value;

                // Same category
                if (currentContainer.ContainerDisplayCategory == targetContainer.ContainerDisplayCategory)
                {
                    // Same file
                    if (primaryKey == targetKey)
                    {
                        // Get the container wrapper from the target bank
                        var targetWrapper = TextBank.TargetFmgBank.Where(
                            e => e.Value.ContainerDisplayCategory == targetContainer.ContainerDisplayCategory &&
                            e.Value.Filename == targetKey).FirstOrDefault().Value;

                        if (targetWrapper != null)
                        {
                            foreach (var curWrapper in entry.Value.FmgWrappers)
                            {
                                foreach (var tarWrapper in targetWrapper.FmgWrappers)
                                {
                                    if (curWrapper.ID == tarWrapper.ID)
                                    {
                                        ProcessFmg(curWrapper, tarWrapper);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        TaskLogs.AddLog($"Applied Text Merge.");
    }

    private static void ProcessFmg(TextFmgWrapper sourceWrapper, TextFmgWrapper targetWrapper)
    {
        List<FMG.Entry> missingEntries = new();
        List<FMG.Entry> modifiedEntries = new();

        foreach (var entry in targetWrapper.File.Entries)
        {
            // Entry ID not present in source, therefore add to missing entries
            if(!sourceWrapper.File.Entries.Any(e => e.ID == entry.ID))
            {
                missingEntries.Add(entry);
            }

            // Entry ID is present in source,
            // Entry Text not present in source, therefore add to modified entries
            if (sourceWrapper.File.Entries.Any(e => e.ID == entry.ID && e.Text != entry.Text))
            {
                modifiedEntries.Add(entry);
            }
        }

        // Add Missing
        foreach(var entry in missingEntries)
        {
            //TaskLogs.AddLog($"{entry.ID} {entry.Text}");
            sourceWrapper.File.Entries.Add(entry);
        }

        if (ReplaceModifiedRows)
        {
            // Change Modified
            foreach (var entry in modifiedEntries)
            {
                //TaskLogs.AddLog($"{entry.ID} {entry.Text}");

                if (sourceWrapper.File.Entries.Any(e => e.ID == entry.ID))
                {
                    var targetEntry = sourceWrapper.File.Entries.Where(e => e.ID == entry.ID).FirstOrDefault();
                    if (targetEntry != null)
                    {
                        targetEntry.Text = entry.Text;
                    }
                }
            }
        }

        //TaskLogs.AddLog($"Modified {sourceWrapper.Name} Text File");
    }
}
