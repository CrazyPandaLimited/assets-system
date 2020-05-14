using CrazyPanda.UnityCore.AssetsSystem.Caching;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
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