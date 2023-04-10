using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public abstract class BaseCacheTests< TCache > where TCache : ICache, new()
    {
        protected TCache _memoryCache { get; private set; }

        protected readonly object testObject1 = new object();
        protected string testObjectName => nameof(testObject1);        
        
        [SetUp]
        public void Setup()
        {
            _memoryCache = new TCache();
        }

        [Test]
        public void AddAndContainsTest()
        {            
            _memoryCache.Add(testObjectName, testObject1);            
            Assert.True(_memoryCache.Contains(testObjectName));
        }

        [Test]
        public void ContainsFalseTest()
        {
            _memoryCache.Add(testObjectName, testObject1);
            Assert.False(_memoryCache.Contains("noname"));
        }

        [Test]
        public void ContainsEmptyTest()
        {            
            Assert.Throws<ArgumentNullException>(()=> { _memoryCache.Contains(""); });
        }

        [Test]
        public void ContainsNullTest()
        {
            Assert.Throws<ArgumentNullException>(() => { _memoryCache.Contains(null); });
        }

        [Test]
        public void AddTest()
        {
            _memoryCache.Add(testObjectName, testObject1);
            Assert.True(_memoryCache.Contains(testObjectName));
        }

        [Test]
        [TestCase("", "asset")]
        [TestCase(null, "asset")]
        [TestCase("key", null)]
        public void AddNullOrEmptyTest(string key, object asset)
        {
            Assert.Throws<ArgumentNullException>(()=> { _memoryCache.Add(key, asset); });
        }

        [Test]
        public void GetTest()
        {
            _memoryCache.Add(testObjectName, testObject1);
            Assert.AreEqual(testObject1, _memoryCache.Get(testObjectName));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]        
        public void GetNullOrEmptyTest(string key)
        {
            Assert.Throws<ArgumentNullException>(() => { _memoryCache.Get(key); });
        }

        [ Test ]
        public abstract void GetNotExistedElementTest();

        [Test]
        public void RemoveTest()
        {
            _memoryCache.Add(testObjectName, testObject1);
            Assert.AreEqual(testObject1, _memoryCache.Get(testObjectName));

            _memoryCache.Remove(testObjectName);
            Assert.False(_memoryCache.Contains(testObjectName));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void RemoveNullorEmptyKeyTest(string key)
        {
            Assert.Throws<ArgumentNullException>(() => { _memoryCache.Remove(key); });
        }

        [Test]        
        public void RemoveNotExistedTest()
        {
            Assert.Throws<KeyNotFoundException>(() => { _memoryCache.Remove("none"); });
        }

        [Test]
        public void GetAllAssetsNamesTest()
        {
            var emptyNames = _memoryCache.GetAllAssetsNames();
            Assert.AreEqual(0, emptyNames.Count);

            _memoryCache.Add("1", 1);
            _memoryCache.Add("2", 1);
            var someNames1 = _memoryCache.GetAllAssetsNames();
            Assert.AreEqual(2, someNames1.Count);
            Assert.True(someNames1.Contains("1"));
            Assert.True(someNames1.Contains("2"));

            _memoryCache.Remove("2");

            var someNames2 = _memoryCache.GetAllAssetsNames();
            Assert.AreEqual(1, someNames2.Count);
            Assert.True(someNames2.Contains("1"));
            Assert.False(someNames2.Contains("2"));
        }


        [Test]
        public void ClearCacheTest()
        {
            _memoryCache.Add("1", 1);
            _memoryCache.Add("2", 1);
            var someNames1 = _memoryCache.GetAllAssetsNames();
            Assert.AreEqual(2, someNames1.Count);

            _memoryCache.ClearCache();

            var someNames2 = _memoryCache.GetAllAssetsNames();
            Assert.AreEqual(0, someNames2.Count);
            Assert.False(someNames2.Contains("1"));
            Assert.False(someNames2.Contains("2"));
        }

    }
}