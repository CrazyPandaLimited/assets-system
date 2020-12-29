using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public sealed class WeakRefCacheTests : ICacheTests< WeakCache >
    {
        private const int TestsTimeoutSeconds = 20;

        [ Test ]
        public override void GetNotExistedElementTest()
        {
            Assert.Throws< KeyNotFoundException >( () => { _assetsMemoryCache.Get( "any" ); } );
        }

        [ Test ]
        public void Add_Should_Throw_ArgumentException_When_Same_Key_Added_Twice()
        {
            AddValueToCache();
            Assert.Throws< ArgumentException >( AddValueToCache );
        }

        [ AsyncTest( TestsTimeoutSeconds ) ]
        public async Task Contains_Should_Return_False_After_Full_GC_Collect()
        {
            _assetsMemoryCache.Add( testObjectName, new object() );
            CheckThatCacheContainsTestValue();
            await RunFullGCCollectAsync();
            Assert.False( _assetsMemoryCache.Contains( testObjectName ) );
        }
        
        [ AsyncTest( TestsTimeoutSeconds ) ]
        public async Task Add_Should_Succeed_Add_Same_Key_Twice_After_Full_GC_Collect()
        {
            _assetsMemoryCache.Add( testObjectName, new object() );
            await RunFullGCCollectAsync();
            Assert.DoesNotThrow( () => _assetsMemoryCache.Add( testObjectName, new object() ) );
            CheckThatCacheContainsTestValue();
        }

        [ AsyncTest (TestsTimeoutSeconds)]
        public async Task Get_Should_Throw_KeyNotFoundException_After_Full_GC_Collect()
        {
            _assetsMemoryCache.Add( testObjectName, new object() );
            CheckThatCacheContainsTestValue();

            Assert.That( _assetsMemoryCache.Get( testObjectName ), Is.Not.Null );
            await RunFullGCCollectAsync();
            Assert.Throws<KeyNotFoundException>( ()=> _assetsMemoryCache.Get( testObjectName ) );
        }

        [ Test]
        public void Type_With_Strong_Reference_Should_Succeed_Keep_Alive_In_Cache()
        {
            AddValueToCache();
            CheckThatCacheContainsTestValue();
            CheckThatCacheContainsTestValue();
        }
        
        private async Task RunFullGCCollectAsync()
        {
            Stopwatch stopwatch = new Stopwatch();

            GC.KeepAlive( stopwatch );
            GC.Collect();

            stopwatch.Start();
            
            const int maxTimeoutSeconds = 10;
            while( _assetsMemoryCache.Contains( testObjectName ) && stopwatch.Elapsed.Seconds <= maxTimeoutSeconds )
            {
                await Task.Yield();
            }
            
            stopwatch.Stop();
        }
        
        private void CheckThatCacheContainsTestValue() => Assert.True( _assetsMemoryCache.Contains( testObjectName ) );
        
        private void AddValueToCache() => _assetsMemoryCache.Add( testObjectName, testObject1 );
    }
}