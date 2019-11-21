using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestInputOutputProcessorWithDefaultOutput< TInputBodyType, TOutputBodyType > : AbstractRequestInputOutputProcessor< TInputBodyType, TOutputBodyType > where TInputBodyType : IMessageBody where TOutputBodyType : IMessageBody
    {
        #region Protected Fields
        protected NodeOutputConnection< TOutputBodyType > _defaultConnection;
        #endregion

        #region Public Members
        public void RegisterDefaultConnection( IInputNode< TOutputBodyType > node )
        {
            var connection = new NodeOutputConnection< TOutputBodyType >( node );
            RegisterConnection( connection );
            _defaultConnection = connection;
        }
        #endregion
    }
}
