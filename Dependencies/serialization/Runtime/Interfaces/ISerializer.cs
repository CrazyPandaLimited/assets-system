#if CRAZYPANDA_UNITYCORE_SERIALIZATION_JSON
namespace CrazyPanda.UnityCore.Serialization
{
    public interface ISerializer
    {
        #region Public Members
        byte[ ] Serialize( object obj );

        T Deserialize< T >( byte[ ] data ) where T : class;
        #endregion
    }
}
#endif