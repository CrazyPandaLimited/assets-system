using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public static class Utils
    {
        public static string ConstructAssetWithSubassetName( string assetName, string subAssetName )
        {
            return $"{assetName}_SubAsset:{subAssetName}";
        }

        public static void LinkTo< T >( this IOutputNode< T > output, AbstractRequestInputProcessor< T > processor )
            where T : IMessageBody
        {
            output.LinkTo( processor.DefaultInput );
        }
    }
}
