using System;
using UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestProcessor<TAsyncOperation, TInputBodyType, TOutputBodyType, TEceptionOutputBodyType > :
        AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< TInputBodyType, TOutputBodyType, TEceptionOutputBodyType > where TEceptionOutputBodyType : IMessageBody where TOutputBodyType : IMessageBody where TInputBodyType : IMessageBody where TAsyncOperation : AsyncOperation
    {
        #region Protected Members
        protected abstract void OnLoadingCompleted( RequestProcessorData data );
        protected abstract bool LoadingFinishedWithoutErrors( RequestProcessorData data );

        protected virtual void OnErrorLoading( RequestProcessorData data )
        {
            
        }

        protected virtual void OnLoadingStarted( MessageHeader header, TInputBodyType body )
        {
        }

        protected virtual void OnOperationCancelled( RequestProcessorData data)
        {
        }
        protected void ConfigureLoadingProcess( RequestProcessorData data )
        {
            OnLoadingStarted( data.Header, data.Body );
            var operationCancelled = false;
            data.Header.CancellationToken.Register( () =>
            {
                operationCancelled = true;
                data.RequestLoadingOperation.completed -= OnOperationFinished;
                OnOperationCancelled( data );
            } );

            data.RequestLoadingOperation.completed += OnOperationFinished;

            void OnOperationFinished( AsyncOperation o )
            {
                o.completed -= OnOperationFinished;

                try
                {
                    var invokeFinishOperationResultEvent = o.isDone && !operationCancelled && LoadingFinishedWithoutErrors( data );

                    if( !invokeFinishOperationResultEvent )
                    {
                        OnErrorLoading( data );
                        return;
                    }

                    OnLoadingCompleted( data );
                }
                catch( Exception e )
                {
                    OnErrorLoading( data );
                    base.ProcessException( data.Header, data.Body, e );
                }
            }
        }

        #endregion
        
        protected class RequestProcessorData
        {
            public TAsyncOperation RequestLoadingOperation { get; }
            public MessageHeader Header { get; }
            public TInputBodyType Body { get; }

            protected internal RequestProcessorData( TAsyncOperation requestLoadingOperation, MessageHeader header, TInputBodyType body )
            {
                RequestLoadingOperation = requestLoadingOperation ?? throw new ArgumentNullException(nameof(requestLoadingOperation));
                Header = header ?? throw new ArgumentNullException(nameof(header));
                Body = body;
            }
        }
    }
}
