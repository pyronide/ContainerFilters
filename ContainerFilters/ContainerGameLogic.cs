using System;
using System.IO;
using ProtoBuf;
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
using Sandbox.Game.Localization;
using Sandbox.Game.Weapons;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using SpaceEngineers.Game.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using VRage.Game.ObjectBuilders.ComponentSystem;

namespace ContainerFilters
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_CargoContainer), false)]
    public class FilteredContainer : MyGameLogicComponent
    {
        public MyObjectBuilder_EntityBase MyObjectBuilder_EntityBase = null;
        public MyCargoContainer MyCargoContainer;
        internal FilterController FilterController;
        internal bool Initialized = false;

        internal static readonly Guid FilterStateGUID = new Guid("BD5CF870-EDBD-4EB7-B298-700A918F411D");
        public static bool registered = false;



        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
           

            FilterController = new FilterController();
            if (!MyAPIGateway.Utilities.IsDedicated)
            {
                MyAPIGateway.TerminalControls.CustomControlGetter += CreateControlsNew;
            }

            base.Init(objectBuilder);
        }

        public override void OnAddedToContainer()
        {
            base.OnAddedToContainer();
            if (Entity.Storage == null)
            {
                Entity.Storage = new MyModStorageComponent()
                {
                    [FilterStateGUID] = ""
                };
            }
        }

        void CreateControlsNew(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (block as IMyCargoContainer != null)
            {
                ContainerTerminal.CreateControls(block, controls);
            }
        }


        public override void OnAddedToScene()
        {
            base.OnAddedToScene();
            // DebugLog.Write("Entering OnAddedToScene");
            LoadData();
        }

        public override bool IsSerialized()
        {
            if (MyAPIGateway.Multiplayer.IsServer)
            {
                SaveData();
            }
            return false;
        }

        internal void AddToFilter(Filterdata filterdata)
        {
            if (filterdata.FilterItems.Length == 0)
            {
                // DebugLog.Write("AddToFilter called with 0 items to add. ignoring...");
                return;
            }

            var inventory = (MyInventory)MyCargoContainer.GetInventory();
            for (int i = 0; i < filterdata.FilterItems.Length; i++)
            {
                if (filterdata.FilterItems[i].Type == FilterType.FILTER_TYPE)
                {
                    MyObjectBuilderType type;
                    if (MyObjectBuilderType.TryParse(filterdata.FilterItems[i].ParseItem, out type) == true)
                    {
                        if (!inventory.Constraint.ConstrainedTypes.Contains(type)) inventory.Constraint.AddObjectBuilderType(type);
                    }
                }
                if (filterdata.FilterItems[i].Type == FilterType.FILTER_ITEM)
                {
                    MyDefinitionId Id;
                    if (MyDefinitionId.TryParse(filterdata.FilterItems[i].ParseItem, out Id) == true)
                    {
                        if (!inventory.Constraint.ConstrainedIds.Contains(Id)) inventory.Constraint.Add(Id);
                    }
                }
                if (!FilterController.FilterList.Contains(filterdata.FilterItems[i]))
                {
                    FilterController.FilterList.Add(filterdata.FilterItems[i]);
                }
            }
            inventory.Constraint.Icon = null;
            SaveData();
        }

        internal void RemoveFromFilter(Filterdata filterdata)
        {
            if (filterdata.FilterItems.Length == 0)
            {
                // DebugLog.Write("RemoveFromFilter called with 0 items to add. ignoring...");
                return;
            }
            var inventory = (MyInventory)MyCargoContainer.GetInventory();
            for (int i = 0; i < filterdata.FilterItems.Length; i++)
            {
                if (filterdata.FilterItems[i].Type == FilterType.FILTER_TYPE)
                {
                    MyObjectBuilderType type;
                    if (MyObjectBuilderType.TryParse(filterdata.FilterItems[i].ParseItem, out type) == true)
                    {
                        if (inventory.Constraint.ConstrainedTypes.Contains(type)) inventory.Constraint.RemoveObjectBuilderType(type);
                    }
                }
                if (filterdata.FilterItems[i].Type == FilterType.FILTER_ITEM)
                {
                    MyDefinitionId Id;
                    if (MyDefinitionId.TryParse(filterdata.FilterItems[i].ParseItem, out Id) == true)
                    {
                        if (inventory.Constraint.ConstrainedIds.Contains(Id)) inventory.Constraint.Remove(Id);
                    }
                }
                if (FilterController.FilterList.Contains(filterdata.FilterItems[i]))
                {
                    FilterController.FilterList.Remove(filterdata.FilterItems[i]);
                }
            }
            inventory.Constraint.Icon = null;
            SaveData();
        }

        internal void SetFilterMode(bool FilterMode)
        {
            var inventory = (MyInventory)MyCargoContainer.GetInventory();
            inventory.Constraint.IsWhitelist = FilterMode;
            FilterController.FilterMode = FilterMode;
            inventory.Constraint.Icon = null;
            SaveData();
        }

        internal void ClearFilter()
        {
            var inventory = (MyInventory)MyCargoContainer.GetInventory();
            inventory.Constraint.Clear();
            FilterController.FilterList.Clear();
            inventory.Constraint.Icon = null;
            SaveData();
        }

        internal void SaveData()
        {
            if (Entity.Storage != null && MyCargoContainer.EntityId != 0)
            {

                Filterdata filterdata = new Filterdata
                {
                    id = MyCargoContainer.EntityId,
                    FilterMode = FilterController.FilterMode,
                    FilterItems = FilterController.FilterList.ToArray(),

                };
                // DebugLog.Write($"SaveData() called for id {filterdata.id}");
                var data = MyAPIGateway.Utilities.SerializeToBinary<Filterdata>(filterdata);
                var save = Convert.ToBase64String(data);
                DebugLog.Write(save);
                Entity.Storage[FilterStateGUID] = save;
            }
        }

        internal void LoadData()
        {

            if (Entity.Storage == null)
            {
                // DebugLog.Write("storage is null, exiting loadData().");
                return;
            }
            string Data;
            if (Entity.Storage.TryGetValue(FilterStateGUID, out Data))
            {
                // DebugLog.Write(Data);
                Filterdata loadedfilterdata = new Filterdata();

                var base64 = Convert.FromBase64String(Data);
                loadedfilterdata = MyAPIGateway.Utilities.SerializeFromBinary<Filterdata>(base64);
                // DebugLog.Write($"loaded id: {loadedfilterdata.id}");
                if (loadedfilterdata.id == MyCargoContainer.EntityId)
                {
                    // DebugLog.Write($"Saved state found (id: {loadedfilterdata.id})");
                    MyInventory inventory = (MyInventory)MyCargoContainer.GetInventory();
                    if (loadedfilterdata.FilterItems != null)
                    {
                        for (int i = 0; i < loadedfilterdata.FilterItems.Count(); i++)
                        {
                            // DebugLog.Write($"{loadedfilterdata.FilterItems[i].DisplayName}");
                            FilterController.FilterList.Add(loadedfilterdata.FilterItems[i]);
                            if (loadedfilterdata.FilterItems[i].Type == FilterType.FILTER_TYPE)
                            {
                                MyObjectBuilderType type;
                                if (MyObjectBuilderType.TryParse(loadedfilterdata.FilterItems[i].ParseItem, out type) == true)
                                    inventory.Constraint.AddObjectBuilderType(type);
                            }
                            else if (loadedfilterdata.FilterItems[i].Type == FilterType.FILTER_ITEM)
                            {
                                MyDefinitionId Id;
                                if (MyDefinitionId.TryParse(loadedfilterdata.FilterItems[i].ParseItem, out Id) == true)
                                    inventory.Constraint.Add(Id);
                            }
                        }
                    }
                    inventory.Constraint.IsWhitelist = loadedfilterdata.FilterMode;
                    FilterController.FilterMode = loadedfilterdata.FilterMode;
                    inventory.Constraint.Icon = null;
                }
                else
                {
                    // DebugLog.Write($"Id mismatch - Entity Id: {Entity.EntityId}  MyCargoContainerId: {MyCargoContainer.EntityId}");
                }
            }
        }
    }
}