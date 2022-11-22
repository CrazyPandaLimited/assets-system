using System;
using System.Linq;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NSubstitute;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
#if !UNITY_EDITOR
    [Ignore("")]
#endif
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
    public class ConditionBasedProcessorTests
    {
        private MessageHeader _header;
        UrlLoadingRequest _LoadingRequest;

        IInputNode< UrlLoadingRequest > outProcessorTrue;
        IInputNode< UrlLoadingRequest > outProcessorFalse;

        [ SetUp ]
        public void Setup()
        {
            _header = new MessageHeader( new MetaData(), CancellationToken.None );
            _LoadingRequest = new UrlLoadingRequest( "", typeof( UnityEngine.Object ), null );

            outProcessorTrue = Substitute.For< IInputNode< UrlLoadingRequest > >();
            outProcessorFalse = Substitute.For< IInputNode< UrlLoadingRequest > >();
        }

        [ TestCase( true ) ]
        [ TestCase( false ) ]
        [ Test ]
        public void PassConditionTest( bool expectedResult )
        {
            var processor = new ConditionBasedProcessor< UrlLoadingRequest >( ( url,exception, data ) => expectedResult );

            int trueConnectionMessagesCount = 0;
            int falseConnectionMessagesCount = 0;

            processor.PassedOutput.LinkTo( outProcessorTrue );
            processor.NotPassedOutput.LinkTo( outProcessorFalse );

            processor.PassedOutput.MessageSent += ( sender, args ) => { trueConnectionMessagesCount++; };
            processor.NotPassedOutput.MessageSent += ( sender, args ) => { falseConnectionMessagesCount++; };

            processor.DefaultInput.ProcessMessage( _header, _LoadingRequest );

            Assert.AreEqual( expectedResult ? 1 : 0, trueConnectionMessagesCount );
            Assert.AreEqual( expectedResult ? 0 : 1, falseConnectionMessagesCount );
        }

        [ Test ]
        public void FailRequestWithExceptionConditionTest()
        {
            var processor = new ConditionBasedProcessor< UrlLoadingRequest >( ( url, exception, data ) => throw new Exception() );
            int trueConnectionMessagesCount = 0;
            int falseConnectionMessagesCount = 0;

            processor.PassedOutput.LinkTo( outProcessorTrue );
            processor.NotPassedOutput.LinkTo( outProcessorFalse );

            processor.PassedOutput.MessageSent += ( sender, args ) => { trueConnectionMessagesCount++; };
            processor.NotPassedOutput.MessageSent += ( sender, args ) => { falseConnectionMessagesCount++; };

            processor.DefaultInput.ProcessMessage( _header, _LoadingRequest );

            Assert.AreEqual( 0, trueConnectionMessagesCount );
            Assert.AreEqual( 0, falseConnectionMessagesCount );

            Assert.AreEqual( FlowNodeStatus.Failed, processor.Status );
            Assert.NotNull( processor.Exception );
        }
    }
}