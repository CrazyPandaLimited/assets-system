using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public sealed class WeakCache : ICache
    {
        private readonly List< string > _removeCache = new List< string >();
        private readonly Dictionary< string, WeakReference > _assets = new Dictionary< string, WeakReference >();

        public void ClearCache() => _assets.Clear();

        public bool Contains( string key )
        {
            ValidateId( key, nameof(key) );
            return _assets.TryGetValue( key, out WeakReference refer ) && IsAlive( refer );
        }

        public List< string > GetAllAssetsNames()
        {
            ValidateItems();
            return _assets.Keys.ToList();
        }

        public void Add( string key, object asset )
        {
            if( asset == null )
            {
                throw new ArgumentNullException( nameof(asset) );
            }

            ValidateId( key, nameof(key) );
            ValidateItems();

            if( !_assets.ContainsKey( key ) )
            {
                _assets[ key ] = new WeakReference( asset );
                return;
            }

            throw new ArgumentException( $@"key: {key} already exist in cache" );
        }

        public object Get( string key )
        {
            ValidateId( key, nameof(key) );
            ValidateItems();

            if( _assets.TryGetValue( key, out WeakReference value ) )
            {
                return value.Target;
            }

            ThrowNotExistException( key );

            return default;
        }

        public void Remove( string key )
        {
            ValidateId( key, nameof(key) );
            if( !_assets.Remove( key ) )
            {
                ThrowNotExistException( key );
            }
        }

        private void ValidateItems()
        {
            _removeCache.Clear();
            foreach( KeyValuePair< string, WeakReference > pair in _assets )
            {
                if( !IsAlive( pair.Value ) )
                {
                    _removeCache.Add( pair.Key );
                }
            }

            foreach( string removeKey in _removeCache )
            {
                _assets.Remove( removeKey );
            }
        }

        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        private void ValidateId( string id, string name = @"id" )
        {
            if( string.IsNullOrEmpty( id ) )
            {
                throw new ArgumentNullException( $"{name ?? "N/A"} cannot be null or empty" );
            }
        }

        private void ThrowNotExistException( string key ) => throw new KeyNotFoundException( $@"key: {key} not exist in cache" );
        private bool IsAlive( WeakReference reference ) => reference.IsAlive && (!(reference.Target is Object target) || target);
    }
}