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
    }
}