using System;
using System.Collections.Generic;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ConditionBasedProcessor< TBodyType > : AbstractRequestInputOutputProcessor< TBodyType, TBodyType > where TBodyType : TrackProgressLoadingRequest
    {
        #region Protected Fields
        protected Func< MetaData, Exception, TBodyType, bool > _conditionDelegate;

        protected Dictionary< bool, NodeOutputConnection< TBodyType > > _conditionOutConnections;
        #endregion

        #region Constructors
        public ConditionBasedProcessor( Func< MetaData,Exception, TBodyType, bool > conditionDelegate )
        {
            _conditionDelegate = conditionDelegate ?? throw new ArgumentNullException( $"{nameof(conditionDelegate)} can not be null!" );
            _conditionOutConnections = new Dictionary< bool, NodeOutputConnection< TBodyType > >( 2 );
        }
        #endregion

        #region Public Members
        public void RegisterConditionPassedOutConnection( IInputNode< TBodyType > node )
        {
            var connection = new NodeOutputConnection< TBodyType >( node );
            RegisterConnection( connection );
            _conditionOutConnections.Add( true, connection );
        }

        public void RegisterConditionFailedOutConnection( IInputNode< TBodyType > node )
        {
            var connection = new NodeOutputConnection< TBodyType >( node );
            RegisterConnection( connection );
            _conditionOutConnections.Add( false, connection );
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            _conditionOutConnections[ _conditionDelegate( header.MetaData, header.Exceptions, body ) ].ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
