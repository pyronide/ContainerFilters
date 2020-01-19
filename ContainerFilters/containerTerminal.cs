using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace ContainerFilters
{
    public static class ContainerTerminal
    {
        public static bool controlsCreated = false;
        public static bool actionsCreated = false;



        public static void CreateControls(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block as IMyCargoContainer == null || controlsCreated == true)
            {
                return;
            }

            controlsCreated = true;

            //seperator and label
            var separatorA = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyCargoContainer>("FilterSectionSeparator");
            separatorA.Enabled = Block => true;
            separatorA.SupportsMultipleBlocks = false;
            separatorA.Visible = Block => ContainerControls.ControlVisibility(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(separatorA);
            controls.Add(separatorA);

            var labelA = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlLabel, IMyCargoContainer>("FilterSectionLabel");
            labelA.Enabled = Block => true;
            labelA.SupportsMultipleBlocks = false;
            labelA.Visible = Block => ContainerControls.ControlVisibility(Block);
            labelA.Label = MyStringId.GetOrCompute("Container Filter Controls");
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(labelA);
            controls.Add(labelA);

            IMyTerminalControlSeparator separatorb = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, IMyCargoContainer>("FilterSectionSeparatorlower");
            separatorb.Enabled = Block => true;
            separatorb.SupportsMultipleBlocks = false;
            separatorb.Visible = Block => ContainerControls.ControlVisibility(Block);
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(separatorb);
            controls.Add(separatorb);

            // BlackList/WhiteList comboBox

            var filterMode = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, IMyCargoContainer>("filterMode");
            filterMode.Enabled = Block => true;
            filterMode.Visible = Block => ContainerControls.ControlVisibility(Block);
            filterMode.Title = MyStringId.GetOrCompute("Filter Mode:");
            filterMode.Tooltip = MyStringId.GetOrCompute("");
            filterMode.SupportsMultipleBlocks = false;
            filterMode.ComboBoxContent = ContainerControls.CreateFilterMode;
            filterMode.Setter = ContainerControls.SetFilterMode;
            filterMode.Getter = ContainerControls.GetFilterMode;
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(filterMode);
            controls.Add(filterMode);

            // "Clear Filter" Button

            var clearFilter = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyCargoContainer>("ClearFilterButton");
            clearFilter.Enabled = Block => true;
            clearFilter.Visible = Block => ContainerControls.ControlVisibility(block);
            clearFilter.Title = MyStringId.GetOrCompute("Clear Filter");
            clearFilter.Tooltip = MyStringId.GetOrCompute("Removes all items and types from Filter");
            clearFilter.SupportsMultipleBlocks = false;
            clearFilter.Action = ContainerControls.ClearFilter;
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(clearFilter);
            controls.Add(clearFilter);
                
            // Current filter list

            var Currentlist = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyCargoContainer>("CurrentList");
            Currentlist.Enabled = Block => true;
            Currentlist.SupportsMultipleBlocks = false;
            Currentlist.Visible = Block => ContainerControls.ControlVisibility(Block);
            Currentlist.Title = MyStringId.GetOrCompute("Current Filter:");
            Currentlist.VisibleRowsCount = 6;
            Currentlist.Multiselect = true;
            Currentlist.ListContent = ContainerControls.CreateCurrentList;
            Currentlist.ItemSelected = ContainerControls.SetSelectedcurrentItem;
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(Currentlist);
            controls.Add(Currentlist);

            // "Remove" button

            var removeButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyCargoContainer>("RemoveButton");
            removeButton.Enabled = Block => true;
            removeButton.SupportsMultipleBlocks = false;
            removeButton.Visible = Block => ContainerControls.ControlVisibility(block);
            removeButton.Title = MyStringId.GetOrCompute("Remove");
            removeButton.Tooltip = MyStringId.GetOrCompute("Removes the item selected in Current list from filter");
            removeButton.Action = ContainerControls.RemoveFromFilter;
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(removeButton);
            controls.Add(removeButton);

            // Filter Candidates list

            var CandidatesList = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlListbox, IMyCargoContainer>("CandidatesList");
            CandidatesList.Enabled = Block => true;
            CandidatesList.SupportsMultipleBlocks = false;
            CandidatesList.Visible = Block => ContainerControls.ControlVisibility(Block);
            CandidatesList.ListContent = ContainerControls.CreateCandidateList;
            CandidatesList.Title = MyStringId.GetOrCompute("Filter Candidates:");
            CandidatesList.VisibleRowsCount = 8;
            CandidatesList.Multiselect = true;
            CandidatesList.ItemSelected = ContainerControls.SetSelectedCandidate;
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(CandidatesList);
            controls.Add(CandidatesList);

            // "Add" button

            var addButton = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, IMyCargoContainer>("AddButton");
            addButton.Enabled = Block => true;
            addButton.SupportsMultipleBlocks = false;
            addButton.Visible = Block => ContainerControls.ControlVisibility(Block);
            addButton.Title = MyStringId.GetOrCompute("Add");
            addButton.Tooltip = MyStringId.GetOrCompute("Adds the selected candidate to filter");
            addButton.Action = ContainerControls.AddToFilter;
            MyAPIGateway.TerminalControls.AddControl<IMyCargoContainer>(addButton);
            controls.Add(addButton);
        }
    }
}
