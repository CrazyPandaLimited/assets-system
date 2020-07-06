using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< TInputBodyType, TOutputBodyType,TEceptionOutputBodyType > : AbstractRequestInputOutputProcessorWithDefaultOutput< TInputBodyType, TOutputBodyType > where TInputBodyType : IMessageBody where TOutputBodyType : IMessageBody where TEceptionOutputBodyType:IMessageBody
    {
        #region Protected Fields
        private NodeOutputConnection< TEceptionOutputBodyType > _exceptionConnection;
        #endregion

        #region Public Members
        public void RegisterExceptionConnection( IInputNode< TEceptionOutputBodyType > node )
        {
            var connection = new NodeOutputConnection< TEceptionOutputBodyType >( node );
             RegisterConnection( connection );
            _exceptionConnection = connection;
        }
        #endregion

        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        protected void ProcessMessageToExceptionConnection( MessageHeader header, TEceptionOutputBodyType body )
        {
            _exceptionConnection?.ProcessMessage( header, body );
        }
    }
}
