using System;
using System.IO;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestProcessor<TAsyncOperation, TInputBodyType, TOutputBodyType, TEceptionOutputBodyType > :
        AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< TInputBodyType, TOutputBodyType, TEceptionOutputBodyType >
        where TEceptionOutputBodyType : IMessageBody
        where TOutputBodyType : IMessageBody
        where TInputBodyType : IMessageBody
        where TAsyncOperation : AsyncOperation
    {
        protected const float InitialProgress = 0.0f;
        protected const float FinalProgress = 1.0f;

        protected abstract void OnLoadingStarted( MessageHeader header, TInputBodyType body );
        protected abstract void OnLoadingCompleted( RequestProcessorData data );
        protected abstract void OnLoadingProgressUpdated( TInputBodyType body, float currentProgress );

        protected abstract bool LoadingFinishedWithoutErrors( RequestProcessorData data );
        
        protected virtual void OnErrorLoading( RequestProcessorData data )
        {
            if( data.Body is TEceptionOutputBodyType exceptionOutputBodyType )
            {
                data.Header.AddException( Exception ?? new AsyncOperationFailedException( data.RequestLoadingOperation, this, data.Header, data.Body ) );
                SendException( data.Header, exceptionOutputBodyType );
            }
        }

        protected virtual void OnOperationCancelled( RequestProcessorData data)
        {
        }

        protected void ConfigureLoadingProcess( RequestProcessorData data )
        {
            OnLoadingStarted( data.Header, data.Body );
            var operationCancelled = false;

            if( data.Header.CancellationToken.CanBeCanceled )
            {
                data.Header.CancellationToken.Register( () =>
                {
                    operationCancelled = true;
                    data.RequestLoadingOperation.completed -= OnOperationFinished;
                    OnOperationCancelled( data );
                    OnErrorLoading( data );
                } );
            }

            StartToTrackLoadingProgress( data );
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
                    base.ProcessException( data.Header, data.Body, e );
                    OnErrorLoading( data );
                }
            }
        }

        private void StartToTrackLoadingProgress( RequestProcessorData data )
        {
            PandaTasksUtilitys.WaitWhile( () =>
            {
                if( data.RequestLoadingOperation.isDone )
                {
                    OnLoadingProgressUpdated( data.Body, FinalProgress );
                    return false;
                }

                if( data.Header.CancellationToken.IsCancellationRequested )
                {
                    return false;
                }

                OnLoadingProgressUpdated(data.Body, data.RequestLoadingOperation.progress );
                return true;
            } );
        }
        
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
