using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ConditionBasedProcessor< TBodyType > : AbstractRequestInputProcessor< TBodyType >
        where TBodyType : TrackProgressLoadingRequest
    {
        protected Func< MetaData, Exception, TBodyType, bool > _conditionDelegate;

        private BaseOutput< TBodyType > _passedOutput = new BaseOutput< TBodyType >( OutputHandlingType.Optional );
        private BaseOutput< TBodyType > _notPassedOutput = new BaseOutput< TBodyType >( OutputHandlingType.Optional );

        public IOutputNode< TBodyType > PassedOutput => _passedOutput;
        public IOutputNode< TBodyType > NotPassedOutput => _notPassedOutput;

        public ConditionBasedProcessor( Func< MetaData, Exception, TBodyType, bool > conditionDelegate )
        {
            _conditionDelegate = conditionDelegate ?? throw new ArgumentNullException( nameof( conditionDelegate ) );
        }

        protected override void InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            if( _conditionDelegate( header.MetaData, header.Exceptions, body ) )
                _passedOutput.ProcessMessage( header, body );
            else
                _notPassedOutput.ProcessMessage( header, body );
        }
    }
}
