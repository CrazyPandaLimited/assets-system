using System;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class SyncLoadException : AssetsSystemException
    {
        #region Properties
        public string Url { get; }
        public MetaData MetaData { get; }
        #endregion

        #region Constructors
        public SyncLoadException( string url, MetaData metaData, Exception innerException )
            : base( $"Operation could not be completed synchronously!!! InputInfo: url:{url} {metaData}", innerException )
        {
            Url = url;
            MetaData = metaData;
        }

        public SyncLoadException( string url, MetaData metaData )
            : this( url, metaData, null )
        {
        }
        #endregion
    }
}
