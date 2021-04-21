using System;
using System.IO;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestProcessor<TAsyncOperation, TInputBodyType, TOutputBodyType > :
        AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< TInputBodyType, TOutputBodyType, TInputBodyType > 
        where TOutputBodyType : IMessageBody
        where TInputBodyType : IMessageBody
        where TAsyncOperation : AsyncOperation
    {
        #region Protected Members
        protected const float InitialProgress = 0.0f;
        protected const float FinalProgress = 1.0f;

        protected abstract void OnLoadingStarted( MessageHeader header, TInputBodyType body );
        protected abstract void OnLoadingCompleted( RequestProcessorData data );
        protected abstract void OnLoadingProgressUpdated( TInputBodyType body, float currentProgress );

        protected abstract bool LoadingFinishedWithoutErrors( RequestProcessorData data );
        
        
        protected virtual void OnErrorLoading( RequestProcessorData data )
        {
            data.Header.AddException( Exception ?? new AsyncOperationFailedException( data.RequestLoadingOperation, this, data.Header, data.Body ) );
            SendException( data.Header,data.Body );
        }

        protected virtual void OnOperationCancelled( RequestProcessorData data)
        {
            data.Header.AddException( new AsyncOperationCancelledException( data.RequestLoadingOperation, this, data.Header, data.Body ) );
            SendException( data.Header,data.Body );
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
        #endregion

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
