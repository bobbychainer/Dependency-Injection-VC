using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using VContainer.Diagnostics;
using VContainer.Unity;

namespace VContainer.Editor.Diagnostics
{
    public sealed class DiagnosticsInfoTreeViewItem : TreeViewItem
    {
        public string ScopeName;
        public IRegistration Registration;
        public IReadOnlyList<ResolveInfo> Resolves;

        public string RegistrationSummary => Registration != null
            ? Registration.GetType().Name
            : "";

        public string ContractTypesSummary => Registration?.InterfaceTypes != null
            ? string.Join(", ", Registration.InterfaceTypes.Select(x => x.Name))
            : "";

        public DiagnosticsInfoTreeViewItem(int id) : base(id)
        {
        }
    }

    public sealed class VContainerDiagnosticsInfoTreeView : TreeView
    {
        const string SortedColumnIndexStateKey = "VContainer.Editor.DiagnosticsInfoTreeView_sortedColumnIndex";

        public static int NextId() => ++idSeed;
        static int idSeed;

        public IReadOnlyList<TreeViewItem> CurrentBindingItems;
        readonly Dictionary<IObjectResolver, int> usedTrackIds = new Dictionary<IObjectResolver, int>();

        public VContainerDiagnosticsInfoTreeView()
            : this(new TreeViewState(), new MultiColumnHeader(new MultiColumnHeaderState(new[]
            {
                new MultiColumnHeaderState.Column { headerContent = new GUIContent("Scope"), width = 60f },
                new MultiColumnHeaderState.Column { headerContent = new GUIContent("Type") },
                new MultiColumnHeaderState.Column { headerContent = new GUIContent("ContractTypes") },
                new MultiColumnHeaderState.Column { headerContent = new GUIContent("Lifetime") },
                new MultiColumnHeaderState.Column { headerContent = new GUIContent("Registration") },
                new MultiColumnHeaderState.Column { headerContent = new GUIContent("ResolveCount"), width = 5f },
            })))
        {
        }

        VContainerDiagnosticsInfoTreeView(TreeViewState state, MultiColumnHeader header)
            : base(state, header)
        {
            rowHeight = 20;
            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            extraSpaceBeforeIconAndLabel = 20;
            showBorder = true;
            header.sortingChanged += OnSortedChanged;

            header.ResizeToFit();
            Reload();

            header.sortedColumnIndex = SessionState.GetInt(SortedColumnIndexStateKey, 1);
        }

        public void ReloadAndSort()
        {
            var currentSelected = state.selectedIDs;
            Reload();
            OnSortedChanged(multiColumnHeader);
            state.selectedIDs = currentSelected;
        }

        void OnSortedChanged(MultiColumnHeader multiColumnHeader)
        {
            // SessionState.SetInt(SortedColumnIndexStateKey, multiColumnHeader.sortedColumnIndex);
            // var index = multiColumnHeader.sortedColumnIndex;
            // var ascending = multiColumnHeader.IsSortedAscending(multiColumnHeader.sortedColumnIndex);
            //
            // var items = rootItem.children.Cast<DiagnosticsInfoTreeViewItem>();
            //
            // IOrderedEnumerable<DiagnosticsInfoTreeViewItem> orderedEnumerable;
            // switch (index)
            // {
            //     case 0:
            //         orderedEnumerable = ascending ? items.OrderBy(item => item.Head) : items.OrderByDescending(item => item.Head);
            //         break;
            //     case 1:
            //         orderedEnumerable = ascending ? items.OrderBy(item => item.Elapsed) : items.OrderByDescending(item => item.Elapsed);
            //         break;
            //     case 2:
            //         orderedEnumerable = ascending ? items.OrderBy(item => item.Count) : items.OrderByDescending(item => item.Count);
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException(nameof(index), index, null);
            // }
            //
            // CurrentBindingItems = rootItem.children = orderedEnumerable.Cast<TreeViewItem>().ToList();
            BuildRows(rootItem);
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { depth = -1 };
            var children = new List<TreeViewItem>();

            if (LifetimeScope.DiagnosticsCollector != null)
            {
                if (VContainerDiagnosticsWindow.EnableCollapse)
                {
                    var grouped = LifetimeScope.DiagnosticsCollector.GetGroupedDiagnosticsInfos();
                    foreach (var scope in grouped)
                    {
                        var parentItem = new DiagnosticsInfoTreeViewItem(NextId())
                        {
                            displayName = scope.Key,
                            ScopeName = scope.Key
                        };
                        children.Add(parentItem);
                        SetExpanded(parentItem.id, true);

                        foreach (var info in scope)
                        {
                            parentItem.AddChild(new DiagnosticsInfoTreeViewItem(NextId())
                            {
                                ScopeName = scope.Key,
                                Registration = info.Registration,
                                Resolves = info.Resolves
                            });
                        }
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }

            CurrentBindingItems = children;
            root.children = CurrentBindingItems as List<TreeViewItem>;

            return root;
        }

        protected override bool CanMultiSelect(TreeViewItem item) => false;

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as DiagnosticsInfoTreeViewItem;

            for (var visibleColumnIndex = 0; visibleColumnIndex < args.GetNumVisibleColumns(); visibleColumnIndex++)
            {
                var rect = args.GetCellRect(visibleColumnIndex);
                var columnIndex = args.GetColumn(visibleColumnIndex);

                var labelStyle = args.selected ? EditorStyles.whiteLabel : EditorStyles.label;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                switch (columnIndex)
                {
                    case 0:
                        // EditorGUI.LabelField(rect, item.ScopeName, labelStyle);
                        base.RowGUI(args);
                        break;
                    case 1:
                        EditorGUI.LabelField(rect, item.Registration?.ImplementationType?.Name ?? "", labelStyle);
                        break;
                    case 2:
                        EditorGUI.LabelField(rect, item.ContractTypesSummary, labelStyle);
                        break;
                    case 3:
                        EditorGUI.LabelField(rect, item.Registration?.Lifetime.ToString(), labelStyle);
                        break;
                    case 4:
                        EditorGUI.LabelField(rect, item.RegistrationSummary, labelStyle);
                        break;
                    case 5:
                        EditorGUI.LabelField(rect, item.Resolves?.Count.ToString(), labelStyle);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnIndex), columnIndex, null);
                }
            }
        }
    }

}

