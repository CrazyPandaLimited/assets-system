﻿using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class LoggingProcessor< TBodyType > : AbstractRequestInputOutputProcessor< TBodyType, TBodyType > where TBodyType : TrackProgressLoadingRequest
    {
        #region Protected Fields
        protected Action< MetaData, TBodyType, AggregateException > _logHandler;
        #endregion

        #region Constructors
        /// <summary>
        /// Handler parameters: URI-Metadata
        /// </summary>
        /// <param name="logHandler"></param>
        public LoggingProcessor( Action< MetaData, TBodyType, AggregateException > logHandler )
        {
            _logHandler = logHandler ?? throw new ArgumentNullException( $"{nameof(logHandler)} == null" );
        }
        #endregion

        #region Protected Members
        protected override void InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            _logHandler( header.MetaData, body, header.Exceptions );
            SendOutput( header, body );
        }
        #endregion
    }
}
