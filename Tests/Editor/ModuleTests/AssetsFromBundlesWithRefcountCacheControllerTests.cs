using CrazyPanda.UnityCore.AssetsSystem.Caching;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class AssetsFromBundlesWithRefcountCacheControllerTests
    {
        [Test]
        public void ConstructorExceptionTest()
        {           
            Assert.Throws<ArgumentNullException>(()=> { new AssetsFromBundlesWithRefcountCacheController(null, new BundlesCacheWithRefcountCacheController()); });
            Assert.Throws<ArgumentNullException>(() => { new AssetsFromBundlesWithRefcountCacheController(new AssetBundleManifest(), null); });
        }

    }
}