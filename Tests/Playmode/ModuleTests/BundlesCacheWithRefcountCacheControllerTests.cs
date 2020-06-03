using CrazyPanda.UnityCore.AssetsSystem.Caching;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
    public class BundlesCacheWithRefcountCacheControllerTests
    { 
        private BundlesCacheWithRefcountCacheController _cacheController;

        private AssetBundle _assetBundle2;
        private AssetBundle _assetBundle3;
        //private object _assetBundle2 = new object();
        private object ownerObject1 = new object();
        //private string bundleName2 = "_assetBundle2";

        //private object _assetBundle3 = new object();
        private object ownerObject2 = new object();
        //private string bundleName3 = "_assetBundle3";

        string bundleName2 = "bundletest_0.bundle";
        string bundleName3 = "bundletest_1.bundle";

        [SetUp]
        public void Setup()
        {
            _cacheController = new BundlesCacheWithRefcountCacheController();
            AssetBundle.UnloadAllAssetBundles(false);

            var bundlePath = $"{Application.dataPath}/UnityCoreSystems/Systems/Tests/ResourcesSystem/Bundle/";
            AssetBundle.UnloadAllAssetBundles(false);
            _assetBundle2 = AssetBundle.LoadFromFile(bundlePath + bundleName2);
            _assetBundle3 = AssetBundle.LoadFromFile(bundlePath + bundleName3);
        }

        [Test]
        public void ContainsTest()
        {            
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);

            Assert.True(_cacheController.Contains(bundleName2));
        }

        [Test]
        public void GetAllAssetsNamesTest()
        {            
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);
            var allAssetNames = _cacheController.GetAllAssetsNames();
            Assert.True(allAssetNames.Contains(bundleName2));
            Assert.True(allAssetNames.Contains(bundleName3));
            Assert.False(allAssetNames.Contains("noasset"));
        }

        [Test]
        public void GetGenericTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);

            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));
            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject2));            
        }

        [Test]
        public void GetGenericExceptionTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);            

            Assert.Throws<AssetTypeMismatchException>(() => { _cacheController.Get<string>(bundleName2, ownerObject1); });            
        }

        [Test]
        public void GetTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);

            Assert.AreEqual(_assetBundle2, _cacheController.Get(bundleName2, ownerObject1, typeof(AssetBundle)));
            Assert.AreEqual(_assetBundle3, _cacheController.Get(bundleName3, ownerObject2, typeof(AssetBundle)));
        }

        [Test]
        public void GetExceptionTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);

            Assert.Throws<AssetTypeMismatchException>(() => { _cacheController.Get(bundleName2, ownerObject1, typeof(string)); });
        }

        [Test]
        public void GetSimpleTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);

            Assert.AreEqual(_assetBundle2, _cacheController.Get(bundleName2, ownerObject1));
        }

        [Test]
        public void RemoveUsedObjectsTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);

            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));
            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject2));

            _cacheController.Remove(bundleName2, false);
            _cacheController.Remove(bundleName3, false);

            Assert.False(_cacheController.Contains(bundleName2));
            Assert.False(_cacheController.Contains(bundleName3));
        }

        [Test]
        public void RemoveIfUnusedObjectsFailToRemoveTest()
        {
            _cacheController.Add<AssetBundle>(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add<AssetBundle>(_assetBundle3, bundleName3, ownerObject2);

            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));
            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject2));

            _cacheController.Remove(bundleName2, true);
            _cacheController.Remove(bundleName3, true);

            Assert.True(_cacheController.Contains(bundleName2));
            Assert.True(_cacheController.Contains(bundleName3));
        }

        [Test]
        public void RemoveIfUnusedObjectsSuccessToRemoveTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);            

            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));            

            _cacheController.ReleaseReference(bundleName2, ownerObject1);            

            _cacheController.Remove(bundleName2, true);            

            Assert.False(_cacheController.Contains(bundleName2));            
        }

        [Test]
        public void GetUnusedAssetNamesTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);

            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));
            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject2));

            _cacheController.ReleaseReference(bundleName2, ownerObject1);

            var unusedAssets = _cacheController.GetUnusedAssetNames();

            Assert.True(unusedAssets.Contains(bundleName2));
            Assert.False(unusedAssets.Contains(bundleName3));
        }


        [Test]
        public void GetAssetsNamesWithReferenceTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject1);


            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));
            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject1));

            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject2));            

            var owner1Assets = _cacheController.GetAssetsNamesWithReference(ownerObject1);

            Assert.True(owner1Assets.Contains(bundleName2));
            Assert.True(owner1Assets.Contains(bundleName3));

            _cacheController.ReleaseReference(bundleName2, ownerObject1);

            var owner1AssetsAfterRelease = _cacheController.GetAssetsNamesWithReference(ownerObject1);

            Assert.False(owner1AssetsAfterRelease.Contains(bundleName2));
            Assert.True(owner1AssetsAfterRelease.Contains(bundleName3));
        }

        [Test]
        public void ReleaseAllAssetReferencesTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);

            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));
            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject1));

            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject2));

            var owner1Assets = _cacheController.GetAssetsNamesWithReference(ownerObject1);

            Assert.True(owner1Assets.Contains(bundleName2));
            Assert.True(owner1Assets.Contains(bundleName3));

            var owner2Assets = _cacheController.GetAssetsNamesWithReference(ownerObject2);
            
            Assert.True(owner2Assets.Contains(bundleName3));

            _cacheController.ReleaseAllAssetReferences(ownerObject2);

            var owner2AssetsAfterFirstRelease = _cacheController.GetAssetsNamesWithReference(ownerObject2);
            var owner1AssetsAfterFirstRelease = _cacheController.GetAssetsNamesWithReference(ownerObject1);
            Assert.False(owner2AssetsAfterFirstRelease.Contains(bundleName3));
            Assert.AreEqual(0, owner2AssetsAfterFirstRelease.Count);

            Assert.True(owner1AssetsAfterFirstRelease.Contains(bundleName2));
            Assert.True(owner1AssetsAfterFirstRelease.Contains(bundleName3));

            _cacheController.ReleaseAllAssetReferences(ownerObject1);

            var owner2AssetsAfterSecondRelease = _cacheController.GetAssetsNamesWithReference(ownerObject2);
            var owner1AssetsAfterSecondRelease = _cacheController.GetAssetsNamesWithReference(ownerObject1);
            Assert.False(owner2AssetsAfterSecondRelease.Contains(bundleName3));
            Assert.AreEqual(0, owner2AssetsAfterSecondRelease.Count);

            Assert.False(owner1AssetsAfterSecondRelease.Contains(bundleName2));
            Assert.False(owner1AssetsAfterSecondRelease.Contains(bundleName3));
            Assert.AreEqual(0, owner1AssetsAfterSecondRelease.Count);
        }

        [Test]
        public void RemoveUnusedAssetsTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1); 
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);

            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));
            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject1));

            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject2));

            _cacheController.ReleaseAllAssetReferences(ownerObject1);
            _cacheController.RemoveUnusedAssets();

            Assert.False(_cacheController.Contains(bundleName2));
            Assert.True(_cacheController.Contains(bundleName3));

            _cacheController.ReleaseAllAssetReferences(ownerObject2);
            _cacheController.RemoveUnusedAssets();

            Assert.False(_cacheController.Contains(bundleName2));
            Assert.False(_cacheController.Contains(bundleName3));
        }

        [Test]
        public void RemoveAllAssetsTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject2);
            _cacheController.Add(_assetBundle3, bundleName3, ownerObject1);

            Assert.AreEqual(_assetBundle2, _cacheController.Get<AssetBundle>(bundleName2, ownerObject1));
            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject1));

            Assert.AreEqual(_assetBundle3, _cacheController.Get<AssetBundle>(bundleName3, ownerObject2));
            
            _cacheController.RemoveAllAssets();

            Assert.False(_cacheController.Contains(bundleName2));
            Assert.False(_cacheController.Contains(bundleName3));
        }

        [Test]
        public void GetDoAddsNewReferenceTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);
            
            Assert.True(_cacheController.Contains(bundleName2));
            Assert.True(_cacheController.GetAssetsNamesWithReference(ownerObject1).Contains(bundleName2));
            var gotObject1 = _cacheController.Get(bundleName2, ownerObject2, typeof(AssetBundle));

            Assert.AreEqual(_assetBundle2, gotObject1);
            Assert.True(_cacheController.GetAssetsNamesWithReference(ownerObject2).Contains(bundleName2));
        }

        [Test]
        public void GetGenericDoAddsNewReferenceTest()
        {
            _cacheController.Add(_assetBundle2, bundleName2, ownerObject1);

            Assert.True(_cacheController.Contains(bundleName2));
            Assert.True(_cacheController.GetAssetsNamesWithReference(ownerObject1).Contains(bundleName2));
            var gotObject1 = _cacheController.Get<AssetBundle>(bundleName2, ownerObject2);

            Assert.AreEqual(_assetBundle2, gotObject1);
            Assert.True(_cacheController.GetAssetsNamesWithReference(ownerObject2).Contains(bundleName2));
        }
    }
}