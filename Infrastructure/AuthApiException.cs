using System;
using System.Runtime.Serialization;
using Eureka.CoinTrade.ApiBase;

namespace Eureka.CoinTrade.Infrastructure
{
    /// <summary>
    /// 自定义授权API异常。
    /// </summary>
    /// <seealso cref="http://msdn.microsoft.com/zh-cn/library/vstudio/seyhszts(v=vs.110).aspx"/>
    /// 如果您设计需要创建自己的异常的应用程序，应从 Exception 选件类派生自定义异常。 最初要求自定义异常应该从 ApplicationException 类派生；但是在实践中并未发现这样有很大意义。
    /// <remarks>
    /// </remarks>
    [Serializable]
    public class AuthApiException : Exception
    {
        public Exchange Exchange { get; private set; }
        public string ApiName { get; set; }
        public AuthApiException(Exchange exchange)
            : base()
        {
            Exchange = exchange;
        }

        public AuthApiException(Exchange exchange, string message)
            : base(message)
        {
            Exchange = exchange;
        }

        public AuthApiException(Exchange exchange, string message, Exception innerException)
            : base(message, innerException)
        {
            Exchange = exchange;
        }

        protected AuthApiException(Exchange exchange, SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Exchange = exchange;
        }

    }


}
