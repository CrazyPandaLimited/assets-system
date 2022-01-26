using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public sealed class WeakRefCacheTests : BaseCacheTests< WeakCache >
    {
        private const int TestsTimeoutSeconds = 30;

        [ Test ]
        public override void GetNotExistedElementTest()
        {
            Assert.Throws< KeyNotFoundException >( () => { _memoryCache.Get( "any" ); } );
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
            _memoryCache.Add( testObjectName, new object() );
            CheckThatCacheContainsTestValue();
            await RunFullGCCollectAsync();
            Assert.False( _memoryCache.Contains( testObjectName ) );
        }

        [Test]
        public void Add_Should_Failed_ToAdd_UnityObjectType_Twice_When_ObjectInstance_Exists()
        {
            var go = new GameObject();
            _memoryCache.Add( testObjectName, go );
            CheckThatCacheContainsTestValue();
            Assert.Throws< ArgumentException >( () => _memoryCache.Add( testObjectName, go ) );
            Object.DestroyImmediate( go );
        }

        [Test]
        public void Add_Should_Succeed_Add_UnityObjectType_MultipleTimes()
        {
            for( int i = 0; i < 5; i++ )
            {
                Assert.DoesNotThrow( () =>
                {
                    var go = new GameObject();
                    _memoryCache.Add( testObjectName, go );
                    CheckThatCacheContainsTestValue();
                    Object.DestroyImmediate( go );
                } );
            }
        }

        [ AsyncTest( TestsTimeoutSeconds ) ]
        public async Task Add_Should_Succeed_Add_Same_Key_Twice_After_Full_GC_Collect()
        {
            _memoryCache.Add( testObjectName, new object() );
            await RunFullGCCollectAsync();
            Assert.DoesNotThrow( () => _memoryCache.Add( testObjectName, new object() ) );
            CheckThatCacheContainsTestValue();
        }

        [ Test ]
        public void Get_Should_Throw_KeyNotFoundException_When_Unity_Object_Was_Destroyed()
        {
            var go = new GameObject();
            _memoryCache.Add( testObjectName, go );
            Object.DestroyImmediate( go );
            Assert.Throws< KeyNotFoundException >( () => _memoryCache.Get( testObjectName ) );
        }
        
        [ AsyncTest (TestsTimeoutSeconds)]
        public async Task Get_Should_Throw_KeyNotFoundException_After_Full_GC_Collect()
        {
            _memoryCache.Add( testObjectName, new object() );
            CheckThatCacheContainsTestValue();

            Assert.That( _memoryCache.Get( testObjectName ), Is.Not.Null );
            await RunFullGCCollectAsync();
            Assert.Throws<KeyNotFoundException>( ()=> _memoryCache.Get( testObjectName ) );
        }

        [ AsyncTest( TestsTimeoutSeconds ) ]
        public async Task Type_With_Strong_Reference_Should_Succeed_Keep_Alive_In_Cache()
        {
            var strongReference = new object();
            _memoryCache.Add( testObjectName, strongReference );
            CheckThatCacheContainsTestValue();
            await RunFullGCCollectAsync();
            CheckThatCacheContainsTestValue();
        }

        [ Test ]
        public void Contains_Should_Return_False_When_Unity_Object_Was_Destroyed()
        {
            var go = new GameObject();
            _memoryCache.Add( testObjectName, go );
            Object.DestroyImmediate( go );
            Assert.That( _memoryCache.Contains( testObjectName ), Is.False );
        }

        [ Test ]
        public void GetAllAssetsNames_Should_Return_EmptyList_When_Unity_Object_Was_Destroyed()
        {
            var go = new GameObject();
            _memoryCache.Add( testObjectName, go );
            Object.DestroyImmediate( go );
            Assert.That( _memoryCache.GetAllAssetsNames(), Is.Empty );
        }
        
        private async Task RunFullGCCollectAsync()
        {
            var lastGcCount = GC.CollectionCount( 2 );
            GC.Collect();
            while( lastGcCount == GC.CollectionCount( 2 ) )
            {
                await Task.Yield();
            }
        }
        
        private void CheckThatCacheContainsTestValue() => Assert.True( _memoryCache.Contains( testObjectName ) );
        
        private void AddValueToCache() => _memoryCache.Add( testObjectName, testObject1 );
    }
}