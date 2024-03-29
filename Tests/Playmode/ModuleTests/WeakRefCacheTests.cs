using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public sealed class WeakRefCacheTests : BaseCacheTests< WeakCache >
    {
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

        [ Test ]
        public void Get_Should_Throw_KeyNotFoundException_When_Unity_Object_Was_Destroyed()
        {
            var go = new GameObject();
            _memoryCache.Add( testObjectName, go );
            Object.DestroyImmediate( go );
            Assert.Throws< KeyNotFoundException >( () => _memoryCache.Get( testObjectName ) );
        }
        
        [ Test ]
        public void Type_With_Strong_Reference_Should_Succeed_Keep_Alive_In_Cache()
        {
            var strongReference = new object();
            _memoryCache.Add( testObjectName, strongReference );
            CheckThatCacheContainsTestValue();

            GC.Collect();
            
            CheckThatCacheContainsTestValue();
            strongReference.ToString();
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
        
        private void CheckThatCacheContainsTestValue() => Assert.True( _memoryCache.Contains( testObjectName ) );
        
        private void AddValueToCache() => _memoryCache.Add( testObjectName, testObject1 );
    }
}