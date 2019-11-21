using CrazyPanda.UnityCore.AssetsSystem.Caching;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class AssetsWithRefcountCacheControllerTests
    { 
        private AssetsWithRefcountCacheController _cacheController;

        private object testObject1 = new object();
        private object ownerObject1 = new object();
        private string testObject1Name = "testObject1";

        private object testObject2 = new object();
        private object ownerObject2 = new object();
        private string testObject2Name = "testObject2";

        [SetUp]
        public void Setup()
        {
            _cacheController = new AssetsWithRefcountCacheController(10);            
        }

        [Test]
        public void ContainsTest()
        {            
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);

            Assert.True(_cacheController.Contains(testObject1Name));
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void ContainsExceptionTest(string key)
        {            
            Assert.Throws<ArgumentNullException>(()=> { _cacheController.Contains(key); });
        }

        [Test]
        public void GetAllAssetsNamesTest()
        {            
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);
            var allAssetNames = _cacheController.GetAllAssetsNames();
            Assert.True(allAssetNames.Contains(testObject1Name));
            Assert.True(allAssetNames.Contains(testObject2Name));
            Assert.False(allAssetNames.Contains("noasset"));
        }

        [Test]
        public void GetGenericTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);

            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));
            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject2));            
        }

        [Test]
        public void GetGenericWrongTypeTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);            

            Assert.Throws<AssetMemoryCacheException>(()=> { _cacheController.Get<string>(testObject1Name, ownerObject1); });            
        }

        [Test]
        [TestCase("", "owner")]
        [TestCase(null, "owner")]
        [TestCase("key", null)]
        public void GetGenericExceptionTest(string key, string owner)
        {
            Assert.Throws<ArgumentNullException>(() => { _cacheController.Get<string>(key, owner); });
        }

        [Test]
        public void GetTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);

            Assert.AreEqual(testObject1, _cacheController.Get(testObject1Name, ownerObject1, typeof(object)));
            Assert.AreEqual(testObject2, _cacheController.Get(testObject2Name, ownerObject2, typeof(object)));
        }

        [Test]
        public void GetWrongTypeTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);

            Assert.Throws<AssetMemoryCacheException>(() => { _cacheController.Get(testObject1Name, ownerObject1, typeof(string)); });
        }

        [Test]
        [TestCase("", "owner")]
        [TestCase(null, "owner")]
        [TestCase("key", null)]
        public void GetExceptionTest(string key, string owner)
        {
            Assert.Throws<ArgumentNullException>(() => { _cacheController.Get(key, owner, typeof(string)); });
        }

        [Test]
        public void RemoveUsedObjectsTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);

            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));
            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject2));

            _cacheController.Remove(testObject1Name, false);
            _cacheController.Remove(testObject2Name, false);

            Assert.False(_cacheController.Contains(testObject1Name));
            Assert.False(_cacheController.Contains(testObject2Name));
        }

        [Test]
        public void RemoveIfUnusedObjectsFailToRemoveTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);

            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));
            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject2));

            _cacheController.Remove(testObject1Name, true);
            _cacheController.Remove(testObject2Name, true);

            Assert.True(_cacheController.Contains(testObject1Name));
            Assert.True(_cacheController.Contains(testObject2Name));
        }

        [Test]
        public void RemoveIfUnusedObjectsSuccessToRemoveTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);            

            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));            

            _cacheController.ReleaseReference(testObject1Name, ownerObject1);            

            _cacheController.Remove(testObject1Name, true);            

            Assert.False(_cacheController.Contains(testObject1Name));            
        }

        [Test]
        public void GetUnusedAssetNamesTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);

            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));
            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject2));

            _cacheController.ReleaseReference(testObject1Name, ownerObject1);

            var unusedAssets = _cacheController.GetUnusedAssetNames();

            Assert.True(unusedAssets.Contains(testObject1Name));
            Assert.False(unusedAssets.Contains(testObject2Name));
        }


        [Test]
        public void GetAssetsNamesWithReferenceTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject1);


            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));
            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject1));

            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject2));            

            var owner1Assets = _cacheController.GetAssetsNamesWithReference(ownerObject1);

            Assert.True(owner1Assets.Contains(testObject1Name));
            Assert.True(owner1Assets.Contains(testObject2Name));

            _cacheController.ReleaseReference(testObject1Name, ownerObject1);

            var owner1AssetsAfterRelease = _cacheController.GetAssetsNamesWithReference(ownerObject1);

            Assert.False(owner1AssetsAfterRelease.Contains(testObject1Name));
            Assert.True(owner1AssetsAfterRelease.Contains(testObject2Name));
        }

        [Test]
        public void ReleaseAllAssetReferencesTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);

            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));
            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject1));

            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject2));

            var owner1Assets = _cacheController.GetAssetsNamesWithReference(ownerObject1);

            Assert.True(owner1Assets.Contains(testObject1Name));
            Assert.True(owner1Assets.Contains(testObject2Name));

            var owner2Assets = _cacheController.GetAssetsNamesWithReference(ownerObject2);
            
            Assert.True(owner2Assets.Contains(testObject2Name));

            _cacheController.ReleaseAllAssetReferences(ownerObject2);

            var owner2AssetsAfterFirstRelease = _cacheController.GetAssetsNamesWithReference(ownerObject2);
            var owner1AssetsAfterFirstRelease = _cacheController.GetAssetsNamesWithReference(ownerObject1);
            Assert.False(owner2AssetsAfterFirstRelease.Contains(testObject2Name));
            Assert.AreEqual(0, owner2AssetsAfterFirstRelease.Count);

            Assert.True(owner1AssetsAfterFirstRelease.Contains(testObject1Name));
            Assert.True(owner1AssetsAfterFirstRelease.Contains(testObject2Name));

            _cacheController.ReleaseAllAssetReferences(ownerObject1);

            var owner2AssetsAfterSecondRelease = _cacheController.GetAssetsNamesWithReference(ownerObject2);
            var owner1AssetsAfterSecondRelease = _cacheController.GetAssetsNamesWithReference(ownerObject1);
            Assert.False(owner2AssetsAfterSecondRelease.Contains(testObject2Name));
            Assert.AreEqual(0, owner2AssetsAfterSecondRelease.Count);

            Assert.False(owner1AssetsAfterSecondRelease.Contains(testObject1Name));
            Assert.False(owner1AssetsAfterSecondRelease.Contains(testObject2Name));
            Assert.AreEqual(0, owner1AssetsAfterSecondRelease.Count);
        }

        [Test]
        public void RemoveUnusedAssetsTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);

            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));
            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject1));

            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject2));

            _cacheController.ReleaseAllAssetReferences(ownerObject1);
            _cacheController.RemoveUnusedAssets();

            Assert.False(_cacheController.Contains(testObject1Name));
            Assert.True(_cacheController.Contains(testObject2Name));

            _cacheController.ReleaseAllAssetReferences(ownerObject2);
            _cacheController.RemoveUnusedAssets();

            Assert.False(_cacheController.Contains(testObject1Name));
            Assert.False(_cacheController.Contains(testObject2Name));
        }

        [Test]
        public void RemoveAllAssetsTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject2);
            _cacheController.Add<object>(testObject2, testObject2Name, ownerObject1);

            Assert.AreEqual(testObject1, _cacheController.Get<object>(testObject1Name, ownerObject1));
            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject1));

            Assert.AreEqual(testObject2, _cacheController.Get<object>(testObject2Name, ownerObject2));
            
            _cacheController.RemoveAllAssets();

            Assert.False(_cacheController.Contains(testObject1Name));
            Assert.False(_cacheController.Contains(testObject2Name));
        }

        [Test]
        public void GetDoAddsNewReferenceTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);
            
            Assert.True(_cacheController.Contains(testObject1Name));
            Assert.True(_cacheController.GetAssetsNamesWithReference(ownerObject1).Contains(testObject1Name));
            var gotObject1 = _cacheController.Get(testObject1Name, ownerObject2, typeof(object));

            Assert.AreEqual(testObject1, gotObject1);
            Assert.True(_cacheController.GetAssetsNamesWithReference(ownerObject2).Contains(testObject1Name));
        }

        [Test]
        public void GetGenericDoAddsNewReferenceTest()
        {
            _cacheController.Add<object>(testObject1, testObject1Name, ownerObject1);

            Assert.True(_cacheController.Contains(testObject1Name));
            Assert.True(_cacheController.GetAssetsNamesWithReference(ownerObject1).Contains(testObject1Name));
            var gotObject1 = _cacheController.Get<object>(testObject1Name, ownerObject2);

            Assert.AreEqual(testObject1, gotObject1);
            Assert.True(_cacheController.GetAssetsNamesWithReference(ownerObject2).Contains(testObject1Name));
        }
    }
}