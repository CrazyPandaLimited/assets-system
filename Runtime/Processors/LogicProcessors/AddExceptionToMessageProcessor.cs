using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AddExceptionToMessageProcessor< TBodyType > : AbstractRequestInputOutputProcessor< TBodyType, TBodyType > where TBodyType : TrackProgressLoadingRequest
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
        protected override void InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            header.AddException( _exception );
            SendOutput( header, body );
        }
        #endregion
    }
}
