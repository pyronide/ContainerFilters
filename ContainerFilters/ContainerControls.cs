using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.ModAPI;
using VRage.Utils;

namespace ContainerFilters
{
    static class ContainerControls
    {
        internal static void CreateFilterMode(List<MyTerminalControlComboBoxItem> comboBoxItems)
        {
            comboBoxItems.Add(new MyTerminalControlComboBoxItem() { Key = 0, Value = MyStringId.GetOrCompute("Whitelist") });
            comboBoxItems.Add(new MyTerminalControlComboBoxItem() { Key = 1, Value = MyStringId.GetOrCompute("Blacklist") });
        }

        internal static void SetFilterMode(IMyTerminalBlock block, long value)
        {
            FilteredContainer component = block?.GameLogic?.GetAs<FilteredContainer>();
            var filterdata = new Filterdata();
            if (value == 0) filterdata.FilterMode = true;
            if (value == 1) filterdata.FilterMode = false;
            var Sendpacket = new MetaPacket
            {
                EntityId = component.Entity.EntityId,
                PacketType = PacketType.FILTER_MODE,
                MetaData = MyAPIGateway.Utilities.SerializeToBinary<Filterdata>(filterdata),
            };
            MyAPIGateway.Multiplayer.SendMessageToServer(31875, MyAPIGateway.Utilities.SerializeToBinary<MetaPacket>(Sendpacket));
            component.SetFilterMode(filterdata.FilterMode);
        }

        internal static long GetFilterMode(IMyTerminalBlock block)
        {
            FilteredContainer component = block?.GameLogic?.GetAs<FilteredContainer>();
            return component.FilterController.FilterMode ? 0 : 1;
        }

        internal static void ClearFilter(IMyTerminalBlock block)
        {
            FilteredContainer component = block?.GameLogic?.GetAs<FilteredContainer>();
            var Sendpacket = new MetaPacket
            {
                EntityId = component.Entity.EntityId,
                PacketType = PacketType.FILTER_CLEAR,
                MetaData = MyAPIGateway.Utilities.SerializeToBinary<Filterdata>(new Filterdata()),
            };
            MyAPIGateway.Multiplayer.SendMessageToServer(31875, MyAPIGateway.Utilities.SerializeToBinary<MetaPacket>(Sendpacket));
            component.ClearFilter();
            ContainerControls.updateVisual("CurrentList");
        }

        internal static void CreateCurrentList(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> listItems, List<MyTerminalControlListBoxItem> selectedItems)
        {
            FilteredContainer component = block?.GameLogic?.GetAs<FilteredContainer>();
            foreach (var item in component.FilterController.FilterList)
            {
                var listitem = new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(item.DisplayName), MyStringId.NullOrEmpty, item);
                listItems.Add(listitem);
            }
        }

        internal static void RemoveFromFilter(IMyTerminalBlock block)
        {
            FilteredContainer component = block?.GameLogic?.GetAs<FilteredContainer>();
            if (component.FilterController.ListToRemove.Count == 0) return;
            Filterdata filterdata = new Filterdata
            {
                id = component.Entity.EntityId,
                FilterMode = component.FilterController.FilterMode,
                FilterItems = component.FilterController.ListToRemove.ToArray(),
            };
            var metaData = MyAPIGateway.Utilities.SerializeToBinary<Filterdata>(filterdata);
            var SendPacket = new MetaPacket
            {
                EntityId = component.Entity.EntityId,
                PacketType = PacketType.FILTER_REMOVE,
                MetaData = metaData,
            };
            MyAPIGateway.Multiplayer.SendMessageToServer(31875, MyAPIGateway.Utilities.SerializeToBinary<MetaPacket>(SendPacket));
            component.RemoveFromFilter(filterdata);
            component.FilterController.ListToRemove.Clear();
            ContainerControls.updateVisual("CurrentList");
        }

        internal static void AddToFilter(IMyTerminalBlock block)
        {
            FilteredContainer component = block?.GameLogic?.GetAs<FilteredContainer>();
            if (component.FilterController.ListToAdd.Count == 0) return;
            Filterdata filterdata = new Filterdata
            {
                id = component.Entity.EntityId,
                FilterMode = component.FilterController.FilterMode,
                FilterItems = component.FilterController.ListToAdd.ToArray(),
            };
            var metaData = MyAPIGateway.Utilities.SerializeToBinary<Filterdata>(filterdata);
            var SendPacket = new MetaPacket
            {
                EntityId = component.Entity.EntityId,
                PacketType = PacketType.FILTER_ADD,
                MetaData = metaData,
            };
            MyAPIGateway.Multiplayer.SendMessageToServer(31875, MyAPIGateway.Utilities.SerializeToBinary<MetaPacket>(SendPacket));
            component.AddToFilter(filterdata);
            component.FilterController.ListToAdd.Clear();
            ContainerControls.updateVisual("CurrentList");
        }

        internal static void CreateCandidateList(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> listItems, List<MyTerminalControlListBoxItem> selectedItems)
        {
            foreach (var item in ContainerSession.CandidateList)
            {
                listItems.Add(new MyTerminalControlListBoxItem(MyStringId.GetOrCompute(item.DisplayName), MyStringId.NullOrEmpty, item));
            }
        }

        internal static void SetSelectedcurrentItem(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> listitems)
        {
            if (listitems.Count != 0)
            {
                FilteredContainer component = block?.GameLogic?.GetAs<FilteredContainer>();
                foreach (var item in listitems)
                {
                    var filteritem = (FilterItem)item.UserData;
                    if (!component.FilterController.ListToRemove.Contains(filteritem)) component.FilterController.ListToRemove.Add(filteritem);
                }
            }
        }

        internal static void SetSelectedCandidate(IMyTerminalBlock block, List<MyTerminalControlListBoxItem> listitems)
        {
            if (listitems.Count != 0)
            {
                FilteredContainer component = block?.GameLogic?.GetAs<FilteredContainer>();
                foreach (var item in listitems)
                {
                    var filteritem = (FilterItem)item.UserData;
                    if (!component.FilterController.ListToAdd.Contains(filteritem)) component.FilterController.ListToAdd.Add(filteritem);
                }
            }
        }

        internal static bool ControlVisibility(IMyTerminalBlock block)
        {
            if (block as IMyCargoContainer != null)
            {
                return true;
            }
            return false;
        }
        public static void updateVisual(string Id)
        {
            var controls = new List<IMyTerminalControl>();
            MyAPIGateway.TerminalControls.GetControls<IMyCargoContainer>(out controls);
            controls.Find(x => x.Id == Id).UpdateVisual();
        }
    }
}
