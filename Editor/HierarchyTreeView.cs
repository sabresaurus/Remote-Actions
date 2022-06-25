﻿using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Sabresaurus.RemoteActions
{
     class HierarchyTreeView : TreeView
    {
        public Action<IList<int>> OnSelectionChanged;

        List<TreeViewItem> displays = new List<TreeViewItem>();
        List<HierarchyNode> nodes = new List<HierarchyNode>();

        public HierarchyTreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
        }

        public void ClearDisplays()
        {
            SetDisplays(new List<TreeViewItem>(), new List<HierarchyNode>());
        }

        public void SetDisplays(List<TreeViewItem> displays, List<HierarchyNode> nodes)
        {
            this.displays = displays;
            this.nodes = nodes;

            Reload();
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            // Instead of drawing base.RowGUI() use our own label so we can recolor it
            Rect rect = args.rowRect;
            rect.xMin += GetContentIndent(args.item);
            GUIStyle style = new GUIStyle(GUI.skin.label);

            // Use .item.id instead of .row as the row index takes folding into account
            bool disabled = (nodes[args.item.id] != null && nodes[args.item.id].ActiveInHierarchy == false);
            // Make the name less opaque if disabled
            Color color = style.normal.textColor;
            color.a = disabled ? 0.5f : 1;
            style.normal.textColor = color;

            GUI.Label(rect, args.item.displayName, style);
        }

        protected override TreeViewItem BuildRoot()
        {
            // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
            // are created from data. Here we just create a fixed set of items, in a real world example
            // a data model should be passed into the TreeView and the items created from the model.

            // This section illustrates that IDs should be unique and that the root item is required to 
            // have a depth of -1 and the rest of the items increment from that.
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(root, displays);

            // Return root of the tree
            return root;
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            OnSelectionChanged(selectedIds);
        }
    }
}