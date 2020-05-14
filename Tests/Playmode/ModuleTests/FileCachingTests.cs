#if CRAZYPANDA_UNITYCORE_ASSETSSYSTEM_JSON
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
    class FileCachingTests
    {
#region Private Fields

        private byte[] _test1 =
        {
            0,
            72,
            255,
            102
        };

        private byte[] _test2 =
        {
            1,
            2,
            3,
            4
        };

#endregion

#region Properties

        public string TestPath
        {
            get { return string.Format("{0}/Test", Application.persistentDataPath); }
        }

#endregion

#region Public Members

        [SetUp]
        public void Setup()
        {
            if (Directory.Exists(TestPath))
            {
                Directory.Delete(TestPath, true);
            }
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(TestPath))
            {
                Directory.Delete(TestPath, true);
            }
        }

        [Test]
        public void Consistent()
        {
            Assert.IsFalse(Directory.Exists(TestPath));

            var fileCaching = new FileCaching(TestPath);
            Assert.IsFalse(fileCaching.Contains("TEST"));


            fileCaching.Add("TEST", _test1);
            Assert.AreEqual(_test1, fileCaching.Get("TEST"));
            Assert.IsTrue(fileCaching.Contains("TEST"));


            fileCaching.Add("TEST", _test2);
            Assert.AreEqual(_test2, fileCaching.Get("TEST"));


            fileCaching.Remove("TEST");
            Assert.IsFalse(fileCaching.Contains("TEST"));
            Assert.Throws<CachedFileNotFoundException>(() => fileCaching.Get("TEST"));

            fileCaching.Add("TEST", _test1);
            File.WriteAllBytes(TestPath + "/TEST", _test2);
            Assert.Throws<InvalidHashException>(() => fileCaching.Get("TEST"));


            fileCaching.ClearCache();
            Assert.IsFalse(File.Exists(TestPath + "/TEST )"));
        }

        [Test]
        public void Resurection()
        {
            var fileCaching = new FileCaching(TestPath);
            fileCaching.Add("test1", _test1);
            fileCaching.Add("test2", _test2);
            fileCaching = null;

            fileCaching = new FileCaching(TestPath);

            var keys = fileCaching.CachedKeys.ToArray();
            Assert.AreEqual(2, keys.Length);
            Assert.True(keys.Contains("test1"));
            Assert.True(keys.Contains("test2"));

            Assert.AreEqual(_test1, fileCaching.Get("test1"));
            Assert.AreEqual(_test2, fileCaching.Get("test2"));
        }

#endregion
    }
}
#endif