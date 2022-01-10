using System;
using System.Collections;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NSubstitute;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public abstract class BaseProcessorModuleWithOneOutTest< TProcessorType, TInputNodeBodyType, TOutNodeBodyType >
        where TProcessorType : AbstractRequestInputOutputProcessor< TInputNodeBodyType, TOutNodeBodyType >
        where TInputNodeBodyType : IMessageBody
        where TOutNodeBodyType : IMessageBody
    {
        protected const float RemoteLoadingTimeoutSec = 1f;
        protected TProcessorType _workProcessor;
        protected IInputNode< TOutNodeBodyType > outProcessor;

        [ SetUp ]
        public void Setup()
        {
            InternalSetup();
            outProcessor = Substitute.For< IInputNode< TOutNodeBodyType > >();
            _workProcessor.DefaultOutput.LinkTo( outProcessor );
        }

        protected abstract void InternalSetup();

        protected IEnumerator WaitForTimeOut( TInputNodeBodyType sendedBody )
        {
            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                yield return null;
            }
        }

        protected IEnumerator WaitForTimeOut(Func<bool> needToBreakWaiting = null)
        {
            bool NeeedToBreak() => needToBreakWaiting?.Invoke() ?? false;
            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while(!NeeedToBreak() && DateTime.Now < timeoutTime )
            {
                yield return null;
            }
        }
    }
}