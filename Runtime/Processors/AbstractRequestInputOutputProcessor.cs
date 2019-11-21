using System;
using System.Collections.Generic;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestInputOutputProcessor< TInputBodyType, TOutputBodyType > : AbstractRequestInputProcessor< TInputBodyType >, IOutputNode< TOutputBodyType > where TInputBodyType : IMessageBody where TOutputBodyType : IMessageBody
    {
        #region Protected Fields
        protected List< IBaseOutputConnection > _connections = new List<IBaseOutputConnection >();
        #endregion

        #region Events
        public event EventHandler< MessageSendedOutEventArgs > OnMessageSended;
        #endregion

        #region Public Members
        public virtual IEnumerable< IBaseOutputConnection > GetOutputs()
        {
            return _connections;
        }
        #endregion

        #region Protected Members
        protected void RegisterConnection( IBaseOutputConnection connection )
        {
            if( _connections.Contains( connection ) )
            {
                throw new ConnectionAlreadyExistException( $"Try register other default connection for node: {GetType()}" );
            }

            _connections.Add( connection );
            connection.OnMessageSended += ( sender, args ) => { OnMessageSended?.Invoke( this, args ); };
        }
        #endregion
    }
}
