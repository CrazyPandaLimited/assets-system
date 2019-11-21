using System;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AddExceptionToMessageProcessor< TBodyType > : AbstractRequestInputOutputProcessorWithDefaultOutput< TBodyType, TBodyType > where TBodyType : TrackProgressLoadingRequest
    {
        #region Protected Fields
        protected Exception _exception;
        #endregion

        #region Constructors
        public AddExceptionToMessageProcessor( Exception exception )
        {
            _exception = exception ?? throw new ArgumentNullException( $"{nameof(exception)} == null" );
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            header.AddException( _exception );
            _defaultConnection.ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
