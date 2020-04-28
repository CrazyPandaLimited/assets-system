using CrazyPanda.UnityCore.AssetsSystem.Caching;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class AssetReferenceInfoTests
    { 
        private AssetReferenceInfo _assetReferenceInfo;

        private object refObject1 = new object();
        private object refObject2 = new object();

        [SetUp]
        public void Setup()
        {
            _assetReferenceInfo = new AssetReferenceInfo();
        }

        [Test]
        public void AddReferenceAndContainsReferenceTest()
        {
            _assetReferenceInfo.AddReference(refObject1);
            Assert.True(_assetReferenceInfo.ContainsReference(refObject1));
        }

        [Test]
        public void RemoveReferenceTest()
        {
            _assetReferenceInfo.AddReference(refObject1);
            Assert.True(_assetReferenceInfo.ContainsReference(refObject1));
            _assetReferenceInfo.RemoveReference(refObject1);
            Assert.False(_assetReferenceInfo.ContainsReference(refObject1));
        }

        [Test]
        public void AddReferenceMultipleTimesTest()
        {
            _assetReferenceInfo.AddReference(refObject1);
            _assetReferenceInfo.AddReference(refObject1);
            Assert.True(_assetReferenceInfo.ContainsReference(refObject1));
            _assetReferenceInfo.RemoveReference(refObject1);
            Assert.False(_assetReferenceInfo.ContainsReference(refObject1));
        }

        [Test]
        public void HasReferencesTrueTest()
        {
            _assetReferenceInfo.AddReference(refObject1);
            Assert.True(_assetReferenceInfo.HasReferences());
        }

        [Test]
        public void HasReferencesFalseTest()
        {
            _assetReferenceInfo.AddReference(refObject1);
            _assetReferenceInfo.AddReference(refObject2);
            Assert.True(_assetReferenceInfo.HasReferences());
            _assetReferenceInfo.RemoveReference(refObject1);
            Assert.True(_assetReferenceInfo.HasReferences());
            _assetReferenceInfo.RemoveReference(refObject2);
            Assert.False(_assetReferenceInfo.HasReferences());
        }
    }
}