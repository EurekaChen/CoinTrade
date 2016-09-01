using System.Collections.Generic;
namespace Eureka.CoinTrade.ApiBase
{
    public interface IAuthApi
    {
        AuthKey AuthKey { get; }

        /// <summary>
        /// 授权交易API访问
        /// </summary>
        /// <param name="method"></param>
        /// <param name="args"></param>
        /// <returns>返回Json数据，如果返回的是String.Empty，说明遇到异常情况。</returns>
        string AuthQuery(string method, Dictionary<string, string> args);
    }
}
