using System;
using System.Linq;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NSubstitute;
using NUnit.Framework;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
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

            processor.RegisterConditionPassedOutConnection( outProcessorTrue );
            processor.RegisterConditionFailedOutConnection( outProcessorFalse );

            processor.GetOutputs().ElementAt( 0 ).OnMessageSended += ( sender, args ) => { trueConnectionMessagesCount++; };
            processor.GetOutputs().ElementAt( 1 ).OnMessageSended += ( sender, args ) => { falseConnectionMessagesCount++; };

            processor.ProcessMessage( _header, _LoadingRequest );

            Assert.AreEqual( expectedResult ? 1 : 0, trueConnectionMessagesCount );
            Assert.AreEqual( expectedResult ? 0 : 1, falseConnectionMessagesCount );
        }

        [ Test ]
        public void FailRequestWithExceptionConditionTest()
        {
            var processor = new ConditionBasedProcessor< UrlLoadingRequest >( ( url, exception, data ) => throw new Exception() );
            int trueConnectionMessagesCount = 0;
            int falseConnectionMessagesCount = 0;

            processor.RegisterConditionPassedOutConnection( outProcessorTrue );
            processor.RegisterConditionFailedOutConnection( outProcessorFalse );

            processor.GetOutputs().ElementAt( 0 ).OnMessageSended += ( sender, args ) => { trueConnectionMessagesCount++; };
            processor.GetOutputs().ElementAt( 1 ).OnMessageSended += ( sender, args ) => { falseConnectionMessagesCount++; };

            processor.ProcessMessage( _header, _LoadingRequest );

            Assert.AreEqual( 0, trueConnectionMessagesCount );
            Assert.AreEqual( 0, falseConnectionMessagesCount );

            Assert.AreEqual( FlowNodeStatus.Failed, processor.Status );
            Assert.NotNull( processor.Exception );
        }
    }
}