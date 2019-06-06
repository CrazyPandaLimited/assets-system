//#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
//using System.Collections.Generic;
//
//namespace CrazyPanda.UnityCore.ResourcesSystem
//{
//    public class AssetFilesManifest
//    {
//        public Dictionary< string, AssetFileInfo > _assetsInfos = new Dictionary< string, AssetFileInfo >();
//        
//        public void AddManifestPart( AssetFilesManifest manifestPart )
//        {
//            foreach (var part in manifestPart._assetsInfos)
//            {
//                _assetsInfos.Add(part.Key,part.Value);
//            }
//        }
//
//        public bool HasResource(string name)
//        {
//            return _assetsInfos.ContainsKey(name);
//        }
//
//        public AssetFileInfo GetAssetByName(string name)
//        {
//            if (!_assetsInfos.ContainsKey(name))
//            {
//                throw new AssetInFileManifestMissingException(name);
//            }
//
//            return _assetsInfos[name];
//        }
//    }
//}
//#endif