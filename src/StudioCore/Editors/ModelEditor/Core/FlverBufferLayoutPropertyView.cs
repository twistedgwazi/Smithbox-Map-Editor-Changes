﻿using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class FlverBufferLayoutPropertyView
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    public FlverBufferLayoutPropertyView(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        var index = Selection._selectedBufferLayout;

        if (index == -1)
            return;

        if (Screen.ResManager.GetCurrentFLVER().BufferLayouts.Count < index)
            return;

        if (Selection.BufferLayoutMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Buffer Layouts are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        var entry = Screen.ResManager.GetCurrentFLVER().BufferLayouts[index];

        for (int i = 0; i < entry.Count; i++)
        {
            DosplayBufferLayoutMember(entry[i], i);
        }
    }

    private void DosplayBufferLayoutMember(FLVER.LayoutMember layout, int index)
    {
        ImGui.Separator();
        if (ImGui.Selectable($"Layout Member {index}##LayoutMember{index}", Selection._subSelectedBufferLayoutMember == index))
        {
            Selection._subSelectedBufferLayoutMember = index;
        }
        ImGui.Separator();

        if (Selection._subSelectedBufferLayoutMember == index)
        {
            ContextMenu.BufferLayoutMemberHeaderContextMenu(index);
        }

        var unk00 = layout.Unk00;
        var type = (int)layout.Type;
        var semantic = (int)layout.Semantic;
        var layoutIndex = layout.Index;

        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Unk00:");
        UIHelper.ShowHoverTooltip("Unknown; 0, 1, or 2.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Layout Type:");
        UIHelper.ShowHoverTooltip("Format used to store this member.");
        ImGui.Text($"");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Layout Semantic:");
        UIHelper.ShowHoverTooltip("Vertex property being stored.");
        ImGui.Text($"");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Index:");
        UIHelper.ShowHoverTooltip("For semantics that may appear more than once such as UVs, which one this member is.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Unk00##unk00{index}", ref unk00);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (layout.Unk00 != unk00)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERBufferLayout_LayoutMember_Unk00(layout, layout.Unk00, unk00));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Type##type{index}", ref type);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)layout.Type != type)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERBufferLayout_LayoutMember_Type(layout, (int)layout.Type, type));
        }

        Decorator.LayoutTypeDecorator(type);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Semantic##semantic{index}", ref semantic);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)layout.Semantic != semantic)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERBufferLayout_LayoutMember_Semantic(layout, (int)layout.Semantic, semantic));
        }

        Decorator.LayoutSemanticDecorator(semantic);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Index##index{index}", ref layoutIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (layout.Index != layoutIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERBufferLayout_LayoutMember_Index(layout, layout.Index, layoutIndex));
        }

        if (layout.Size == -1)
        {
            UIHelper.WrappedTextColored(UI.Default.ImGui_Warning_Text_Color, "Invalid Layout Type. Size cannot be determined.");
        }

        ImGui.Columns(1);

    }
}

