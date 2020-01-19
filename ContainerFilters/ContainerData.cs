using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContainerFilters
{
    public enum PacketType
    {
        ERROR = 0,
        FILTER_MODE,
        FILTER_ADD,
        FILTER_REMOVE,
        FILTER_CLEAR,
    }

    public enum FilterType
    {
        ERROR = 0,
        FILTER_TYPE,
        FILTER_ITEM,
        FILTER_CUSTOMGROUP,
    }

    [ProtoContract]
    public struct MetaPacket
    {
        [ProtoMember(1, IsRequired = true)] public PacketType PacketType;
        [ProtoMember(2)] public long EntityId;
        [ProtoMember(3)] public byte[] MetaData;
    }

    [ProtoContract]
    public struct Filterdata
    {
        [ProtoMember(1, DataFormat = DataFormat.Default)]
        internal long id;
        [ProtoMember(2, DataFormat = DataFormat.Default)]
        internal bool FilterMode;
        [ProtoMember(3)] internal FilterItem[] FilterItems;

        public override string ToString()
        {
            return $"";
        }
    }

    [ProtoContract]
    public struct FilterItem
    {
        [ProtoMember(1)] public FilterType Type; 
        [ProtoMember(2)] public string ParseItem;
        [ProtoMember(3)] public string DisplayName;
    }

    internal class FilterController
    {
        internal bool FilterMode = false;
        internal List<FilterItem> FilterList = new List<FilterItem>();
        internal List<FilterItem> ListToAdd = new List<FilterItem>();
        internal List<FilterItem> ListToRemove = new List<FilterItem>();
    }

}
