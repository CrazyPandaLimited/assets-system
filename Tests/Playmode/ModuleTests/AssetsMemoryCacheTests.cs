using CrazyPanda.UnityCore.AssetsSystem.Caching;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class AssetsMemoryCacheTests
    { 
        private AssetsMemoryCache _assetsMemoryCache;

        private object testObject1 = new object();
        private string testObjectName => nameof(testObject1);        
        

        [SetUp]
        public void Setup()
        {
            _assetsMemoryCache = new AssetsMemoryCache();
        }

        [Test]
        public void AddAndContainsTest()
        {            
            _assetsMemoryCache.Add(testObjectName, testObject1);            
            Assert.True(_assetsMemoryCache.Contains(testObjectName));
        }

        [Test]
        public void ContainsFalseTest()
        {
            _assetsMemoryCache.Add(testObjectName, testObject1);
            Assert.False(_assetsMemoryCache.Contains("noname"));
        }

        [Test]
        public void ContainsEmptyTest()
        {            
            Assert.Throws<ArgumentNullException>(()=> { _assetsMemoryCache.Contains(""); });
        }

        [Test]
        public void ContainsNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => { _assetsMemoryCache.Contains(null); });
        }

        [Test]
        public void AddTest()
        {
            _assetsMemoryCache.Add(testObjectName, testObject1);
            Assert.True(_assetsMemoryCache.Contains(testObjectName));
        }

        [Test]
        [TestCase("", "asset")]
        [TestCase(null, "asset")]
        [TestCase("key", null)]
        public void AddNullOrEmptyTest(string key, object asset)
        {
            Assert.Throws<ArgumentNullException>(()=> { _assetsMemoryCache.Add(key, asset); });
        }

        [Test]
        public void GetTest()
        {
            _assetsMemoryCache.Add(testObjectName, testObject1);
            Assert.AreEqual(testObject1, _assetsMemoryCache.Get(testObjectName));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]        
        public void GetNullOrEmptyTest(string key)
        {
            Assert.Throws<ArgumentNullException>(() => { _assetsMemoryCache.Get(key); });
        }

        [Test]
        public void GetNotExistedElementTest()
        {
            Assert.Throws<AssetMemoryCacheException>(() => { _assetsMemoryCache.Get("any"); });
        }

        [Test]
        public void RemoveTest()
        {
            _assetsMemoryCache.Add(testObjectName, testObject1);
            Assert.AreEqual(testObject1, _assetsMemoryCache.Get(testObjectName));

            _assetsMemoryCache.Remove(testObjectName);
            Assert.False(_assetsMemoryCache.Contains(testObjectName));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void RemoveNullorEmptyKeyTest(string key)
        {
            Assert.Throws<ArgumentNullException>(() => { _assetsMemoryCache.Remove(key); });
        }

        [Test]        
        public void RemoveNotExistedTest()
        {
            Assert.Throws<KeyNotFoundException>(() => { _assetsMemoryCache.Remove("none"); });
        }

        [Test]
        public void GetAllAssetsNamesTest()
        {
            var emptyNames = _assetsMemoryCache.GetAllAssetsNames();
            Assert.AreEqual(0, emptyNames.Count);

            _assetsMemoryCache.Add("1", 1);
            _assetsMemoryCache.Add("2", 1);
            var someNames1 = _assetsMemoryCache.GetAllAssetsNames();
            Assert.AreEqual(2, someNames1.Count);
            Assert.True(someNames1.Contains("1"));
            Assert.True(someNames1.Contains("2"));

            _assetsMemoryCache.Remove("2");

            var someNames2 = _assetsMemoryCache.GetAllAssetsNames();
            Assert.AreEqual(1, someNames2.Count);
            Assert.True(someNames2.Contains("1"));
            Assert.False(someNames2.Contains("2"));
        }


        [Test]
        public void ClearCacheTest()
        {
            _assetsMemoryCache.Add("1", 1);
            _assetsMemoryCache.Add("2", 1);
            var someNames1 = _assetsMemoryCache.GetAllAssetsNames();
            Assert.AreEqual(2, someNames1.Count);

            _assetsMemoryCache.ClearCache();

            var someNames2 = _assetsMemoryCache.GetAllAssetsNames();
            Assert.AreEqual(0, someNames2.Count);
            Assert.False(someNames2.Contains("1"));
            Assert.False(someNames2.Contains("2"));
        }

    }
}