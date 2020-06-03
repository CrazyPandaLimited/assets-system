using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ConnectionAlreadyExistException : AssetsSystemException
    {
        #region Properties
        public IFlowNode FlowNode { get; }
        public IBaseOutputConnection Connection { get; }
        #endregion

        #region Constructors
        public ConnectionAlreadyExistException( IFlowNode flowNode, IBaseOutputConnection connection )
            : base( $"Connection {connection} already exists in node '{flowNode}'" )
        {
            FlowNode = flowNode;
            Connection = connection;
        }
        #endregion
    }
}
