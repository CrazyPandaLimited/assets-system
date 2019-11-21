using System;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestInputProcessor< TInputBodyType > : IInputNode< TInputBodyType > where TInputBodyType : IMessageBody
    {
        #region Private Fields
        private FlowNodeStatus _status;
        #endregion

        #region Properties
        public FlowNodeStatus Status { get => _status; protected set { _status = value; } }

        public AggregateException Exception { get; protected set; }
        #endregion

        #region Events
        public event EventHandler< FlowNodeStatusChangedEventArgs > OnStatusChanged = delegate { };
        public event EventHandler< MessageConsumedEventArgs > OnMessageConsumed = delegate { };
        #endregion

        #region Public Members
        public FlowMessageStatus ProcessMessage( MessageHeader header, TInputBodyType body )
        {
            if( Status == FlowNodeStatus.Failed )
            {
                return FlowMessageStatus.Rejected;
            }

            try
            {
                OnMessageConsumed.Invoke( this, new MessageConsumedEventArgs(header, body) );

                if( header.CancellationToken.IsCancellationRequested )
                {
                    return FlowMessageStatus.Accepted;
                }

                return InternalProcessMessage( header, body );
            }
            catch( Exception e )
            {
                ProcessException( header, body, e );
                return FlowMessageStatus.Rejected;
            }
        }

        public void Restore()
        {
            InternalRestore();
            OnStatusChanged.Invoke( this, new FlowNodeStatusChangedEventArgs( this, Status ) );
        }

        public void Dispose()
        {
            InternalDispose();
        }
        #endregion

        #region Protected Members
        protected void ProcessException( MessageHeader header, IMessageBody body, Exception ex )
        {
            Exception = new AggregateException( new Exception($"HeaderInfo:{header.ToString()} BodyInfo:{body.ToString()}"), ex );
            Status = FlowNodeStatus.Failed;
            OnStatusChanged.Invoke( this, new FlowNodeStatusChangedEventArgs( this, header, body, Status ) );
        }

        protected abstract FlowMessageStatus InternalProcessMessage( MessageHeader header, TInputBodyType body );

        protected virtual void InternalDispose()
        {
        }

        protected virtual void InternalRestore()
        {
            Status = FlowNodeStatus.Working;
            Exception = null;
        }
        #endregion
    }
}
