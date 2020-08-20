#if CRAZYPANDA_UNITYCORE_ASSETSSYSTEM_JSON
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class FileCaching : ICache
    {
#region Private Fields
        private CacheInfo _cacheInfo;        
        private FileLocksManager _fileLocksManager;

        private string _cachingDirectory;
        private string _cacheInfoPath;
#endregion

#region Properties
        public IEnumerable< string > CachedKeys { get { return _cacheInfo.Files.Select( f => f.Key ); } }
#endregion

#region Constructors
        /// <summary>
        /// Use different directories for different FileCachings
        /// You need provide full path
        /// </summary>
        public FileCaching( string directory )
        {
            _cachingDirectory = directory;
            if( !Directory.Exists( _cachingDirectory ) )
            {
                Directory.CreateDirectory( _cachingDirectory );
            }
            
            _fileLocksManager = new FileLocksManager();

            _cacheInfoPath = GetPathForKey( "cacheinfo.txt" );
            if( File.Exists( _cacheInfoPath ) )
            {
                _cacheInfo = JsonUtility.FromJson<CacheInfo>(File.ReadAllText(_cacheInfoPath));                    
            }
            else
            {
                _cacheInfo = new CacheInfo();
            }
        }
#endregion

#region Public Members
        public bool Contains( string key )
        {
            return _cacheInfo.Contains( key );
        }

        public List< string > GetAllAssetsNames()
        {
            var result = new List< string >();
            foreach( var info in _cacheInfo.Files )
            {
                result.Add( info.Key );
            }

            return result;
        }

        public void Add( string key, object asset )
        {
            if( !( asset is byte[ ] ) )
            {
                throw new InvalidOperationException( $"Try to add not bytes to file caching!!! Key:{key}" );
            }

            try
            {
                _fileLocksManager.AddWriteLock( key );
            }
            catch( LockAlreadyCapturedException e )
            {
                throw new FileCacheReadException( key, GetPathForKey( key ), e );
            }

            try
            {
                InternalSave( key, ( byte[] ) asset );

                AddKeyAndSaveMetaFile( key, ( byte[] ) asset );
            }
            finally
            {
                _fileLocksManager.RemoveWriteLock( key );
            }
        }

        public object Get( string key )
        {
            try
            {
                _fileLocksManager.AddReadLock( key );
            }
            catch( LockAlreadyCapturedException e )
            {
                throw new FileCacheReadException( key, GetPathForKey( key ), e );
            }

            try
            {
                var cachedFileInfo = GetCachedFileInfoStrict( key );
                return InternalLoad( key, cachedFileInfo );
            }
            finally
            {
                _fileLocksManager.RemoveReadLock( key );
            }
        }

        public void Remove( string key )
        {
            if( !_cacheInfo.Contains( key ) )
            {
                return;
            }

            if( _fileLocksManager.HasAnyLock( key ) )
            {
                return;
            }

            InternalDelete( key );
            _cacheInfo.Remove( key );
            SaveMetaFile();
        }

        public void ClearCache()
        {
            List< CachedFileInfo > infoForRemove = new List< CachedFileInfo >();
            foreach( var file in _cacheInfo.Files.ToArray() )
            {
                if( _fileLocksManager.HasAnyLock( file.Key ) )
                {
                    continue;
                }

                InternalDelete( file.Key );
                infoForRemove.Add( file );
            }

            foreach( var removedKey in infoForRemove )
            {
                _cacheInfo.Files.Remove( removedKey );
            }

            SaveMetaFile();
        }
#endregion

#region Private Members
        private void InternalSave( string key, byte[ ] file )
        {
            var path = GetPathForKey( key );
            File.WriteAllBytes( path, file );
        }

        private byte[ ] InternalLoad( string key, CachedFileInfo cachedFileInfo )
        {
            byte[ ] bytes;
            var path = GetPathForKey( key );
            try
            {
                bytes = File.ReadAllBytes( path );
            }
            catch( Exception e )
            {
                throw new FileCacheReadException( key, path, e );
            }

            var actualMd5 = CalculateMd5( bytes );
            if( cachedFileInfo.Hash != actualMd5 )
            {
                throw new InvalidHashException( key, path, cachedFileInfo.Hash, actualMd5 );
            }

            return bytes;
        }

        private void InternalDelete( string key )
        {
            var path = GetPathForKey( key );
            File.Delete( path );
        }

        private string GetPathForKey( string key )
        {
            var invalidFileNameChars = Path.GetInvalidFileNameChars();
            var filename = new string( key.Select( ch => invalidFileNameChars.Contains( ch ) ? '_' : ch ).ToArray() );
            return string.Format( "{0}/{1}", _cachingDirectory, filename );
        }

        private CachedFileInfo GetCachedFileInfoStrict( string key )
        {
            if( !_cacheInfo.Contains( key ) )
            {
                throw new AssetNotFoundInCacheException( key );
            }

            return _cacheInfo.Get( key );
        }

        private void AddKeyAndSaveMetaFile( string key, byte[ ] file )
        {
            _cacheInfo.AddOrUpdate( key, CalculateMd5( file ) );
            SaveMetaFile();
        }

        private void SaveMetaFile()
        {
            File.WriteAllText( _cacheInfoPath, JsonUtility.ToJson( _cacheInfo ) );
        }

        private static string CalculateMd5( byte[ ] file )
        {
            return ToPrettyString( MD5.Create().ComputeHash( file ) );
        }

        private static string ToPrettyString( byte[ ] bytes )
        {
            var sb = new StringBuilder();
            for( var i = 0; i < bytes.Length; i++ )
            {
                sb.Append( bytes[ i ].ToString( "x2" ) );
            }

            return sb.ToString();
        }
#endregion
    }
}
#endif