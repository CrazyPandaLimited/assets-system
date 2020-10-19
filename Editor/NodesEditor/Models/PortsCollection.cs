using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public interface IPortsCollection : IEnumerable<PortInfo>
    {
    }

    public sealed class PortsCollection : List< PortInfo >, IPortsCollection
    {
        
    }

    public readonly struct PortInfo
    {
        public string ID { get; }
        public Type Type { get; }

        public bool Exists => !string.IsNullOrEmpty( ID );
        
        public PortInfo( string id, Type type )
        {
            ID = id;
            Type = type;
        }
    }
}