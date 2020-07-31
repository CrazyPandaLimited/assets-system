using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestProcessor : IFlowNode
    {
        private IReadOnlyCollection< IInputNode > _inputs;
        private IReadOnlyCollection< IOutputNode > _outputs;

        public FlowNodeStatus Status { get; protected set; }

        public AbstractProcessorException Exception { get; protected set; }
        Exception IFlowNode.Exception => Exception;

        public IReadOnlyCollection< IInputNode > Inputs
        {
            get
            {
                if( _inputs == null )
                    CollectInputsAndOutputs();

                return _inputs;
            }
        }

        public IReadOnlyCollection< IOutputNode > Outputs
        {
            get
            {
                if( _outputs == null )
                    CollectInputsAndOutputs();

                return _outputs;
            }
        }

        public event EventHandler< FlowNodeStatusChangedEventArgs > OnStatusChanged;

        public void Restore()
        {
            Status = FlowNodeStatus.Working;
            Exception = null;

            OnStatusChanged?.Invoke( this, new FlowNodeStatusChangedEventArgs( this, Status ) );
        }

        protected void ProcessException( MessageHeader header, IMessageBody body, Exception ex )
        {
            if( ex is AbstractProcessorException processorException )
            {
                Exception = processorException;
            }
            else
            {
                Exception = new UnhandledProcessorException( this, header, body, ex );
            }

            Status = FlowNodeStatus.Failed;
            OnStatusChanged?.Invoke( this, new FlowNodeStatusChangedEventArgs( this, Status , header, body) );
        }

        private void CollectInputsAndOutputs()
        {
            if( _inputs != null && _outputs != null )
                return;

            var props = GetType().GetProperties();

            List< IInputNode > inputs = null;
            List< IOutputNode > outputs = null;

            foreach( var prop in props )
            {
                var propType = prop.PropertyType;

                if( propType.IsGenericType )
                {
                    if( propType.GetGenericTypeDefinition() == typeof( IInputNode<> ) )
                    {
                        if( inputs == null )
                            inputs = new List< IInputNode >();

                        inputs.Add( prop.GetValue( this ) as IInputNode );
                    }

                    if( propType.GetGenericTypeDefinition() == typeof( IOutputNode<> ) )
                    {
                        if( outputs == null )
                            outputs = new List< IOutputNode >();

                        outputs.Add( prop.GetValue( this ) as IOutputNode );
                    }
                }
            }

            _inputs = inputs as IReadOnlyCollection< IInputNode > ?? Array.Empty< IInputNode >();
            _outputs = outputs as IReadOnlyCollection< IOutputNode > ?? Array.Empty< IOutputNode >();
        }
    }
}
