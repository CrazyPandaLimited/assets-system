using System;
using System.Collections;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NSubstitute;
using NUnit.Framework;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public abstract class BaseProcessorModuleWithOneOutTest< ProcessorType, InputNodeBodyType, OutNodeBodyType > where ProcessorType : AbstractRequestInputOutputProcessorWithDefaultOutput< InputNodeBodyType, OutNodeBodyType > where InputNodeBodyType : IMessageBody where OutNodeBodyType : IMessageBody
    {
        protected const float RemoteLoadingTimeoutSec = 5f;
        protected ProcessorType _workProcessor;
        protected IInputNode< OutNodeBodyType > outProcessor;

        [ SetUp ]
        public void Setup()
        {
            InternalSetup();
            outProcessor = Substitute.For< IInputNode< OutNodeBodyType > >();
            _workProcessor.RegisterDefaultConnection( outProcessor );
        }

        protected abstract void InternalSetup();

        protected IEnumerator WaitForTimeOut( InputNodeBodyType sendedBody )
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