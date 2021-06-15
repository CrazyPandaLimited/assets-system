using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class WebRequestException : AbstractProcessorException
    {
        public string Url { get; }
        public long StatusCode { get; }
        public string ErrorMessage { get; }
        public bool IsHttpError { get; }
        public bool IsNetworkError { get; }

        public WebRequestException( UnityWebRequest request, IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( $"WebRequestError: {request.responseCode} {request.url} - {request.error}", flowNode, messageHeader, messageBody )
        {
            Url = request.url;
            StatusCode = request.responseCode;
            ErrorMessage = request.error;
            IsHttpError = request.isHttpError;
            IsNetworkError = request.isNetworkError;
        }
    }
}
