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
namespace ContainerFilters
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

    class ContainerSession : MySessionComponentBase
    {
        internal static List<FilterItem> CandidateList;
        internal static List<List<FilterItem>> CustomGroupList;

        internal readonly Guid FilterState = new Guid("BD5CF870-EDBD-4EB7-B298-700A918F411D");

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            base.Init(sessionComponent);
            MyAPIGateway.Multiplayer.RegisterMessageHandler(31875, ParseData);
            DebugLog.Init("CargoContainerFilters.log");
            MyEntities.OnEntityCreate += OnEntityCreate;
        }

        public void OnEntityCreate(IMyEntity entity)
        {
                var Cargo = entity as MyCargoContainer;
                if (Cargo == null)
                {
                    return;
                }
                FilteredContainer logic = entity?.GameLogic?.GetAs<FilteredContainer>();
                logic.MyCargoContainer = entity as MyCargoContainer;
                var inventory = (MyInventory)logic.MyCargoContainer.GetInventory();
                if (inventory.Constraint == null)
                {
                    inventory.Constraint = new MyInventoryConstraint(MyStringId.NullOrEmpty, null, false);
                }
        }

        public override void BeforeStart()
        {
            base.BeforeStart();
            var PhysicalItemList = MyDefinitionManager.Static.GetPhysicalItemDefinitions().OrderBy(x => { if (x != null) return x.DisplayNameText; else return null; });
            List<MyObjectBuilderType> _ItemTypes = new List<MyObjectBuilderType>();
            List<MyDefinitionId> _ItemIds = new List<MyDefinitionId>();
            foreach (var item in PhysicalItemList)
            {
                if ((item.CanSpawnFromScreen == true) && (item.Public == true))
                {
                    if (_ItemTypes.Contains(item.Id.TypeId) == false)
                    {
                        _ItemTypes.Add(item.Id.TypeId);
                    }
                    if (_ItemIds.Contains(item.Id) == false)
                    {
                        _ItemIds.Add(item.Id);
                    }
                }
            }
            List<FilterItem> Candidates = new List<FilterItem>();
            foreach (MyObjectBuilderType type in _ItemTypes)
            {
                FilterItem newItem = new FilterItem
                {
                    Type = FilterType.FILTER_TYPE,
                    ParseItem = type.ToString(),
                    DisplayName = "Type: " + type.ToString().Substring(16),
                };
                Candidates.Add(newItem);
            }
            foreach (MyDefinitionId Id in _ItemIds)
            {
                FilterItem newId = new FilterItem
                {
                    Type = FilterType.FILTER_ITEM,
                    ParseItem = Id.ToString(),
                    DisplayName = MyDefinitionManager.Static.GetPhysicalItemDefinition(Id).DisplayNameText,
                };
                Candidates.Add(newId);
            }
            CandidateList = Candidates;
        }

        internal void ParseData(byte[] data)
        {
            MetaPacket metaPacket = MyAPIGateway.Utilities.SerializeFromBinary<MetaPacket>(data);

            if (MyAPIGateway.Multiplayer.IsServer)
            {
                var BaseEntity = MyEntities.GetEntityById(metaPacket.EntityId);
                if (BaseEntity == null)
                {
                    return;
                }
                var filteredContainer = BaseEntity?.GameLogic?.GetAs<FilteredContainer>();
                if (filteredContainer == null)
                {
                    return;
                }

                var filterdata = MyAPIGateway.Utilities.SerializeFromBinary<Filterdata>(metaPacket.MetaData);
                switch (metaPacket.PacketType)
                {
                    case PacketType.FILTER_ADD:
                        {
                            filteredContainer.AddToFilter(filterdata);
                            break;
                        }
                    case PacketType.FILTER_REMOVE:
                        {
                            filteredContainer.RemoveFromFilter(filterdata);
                            break;
                        }
                    case PacketType.FILTER_MODE:
                        {
                            filteredContainer.SetFilterMode(filterdata.FilterMode);
                            break;
                        }
                    case PacketType.FILTER_CLEAR:
                        {
                            filteredContainer.ClearFilter();
                            break;
                        }
                }
            }
        }

        private static int CandidateListSorter(FilterItem filteritem)
        {
            if (filteritem.ParseItem != null)
            {
                return CandidateList.IndexOf(filteritem);
            }
            return -1;
        }

        protected override void UnloadData()
        {
            DebugLog.Close();
        }
    }
}
