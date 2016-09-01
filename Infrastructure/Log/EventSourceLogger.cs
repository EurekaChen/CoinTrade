using System;
using System.Diagnostics.Tracing;
using System.Runtime.CompilerServices;
using Eureka.CoinTrade.Infrastructure.Event;
using Eureka.CoinTrade.Infrastructure.Properties;
using Eureka.SystemE.Base;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.ServiceLocation;

namespace Eureka.CoinTrade.Infrastructure
{
    //由于使用了ILoggerFacade，所以API中引用该库时同时要引用Prism! 去掉是不是会更好？
    [EventSource(Name = "Eureka-CoinTrade")]
    public partial class EventSourceLogger : EventSource, ILoggerFacade
    {
        private static readonly Lazy<EventSourceLogger> Instance = new Lazy<EventSourceLogger>(() => new EventSourceLogger());
        private EventSourceLogger() { }

        private static IEventAggregator eventAggregator;
        private static void SendToStatusBar(KeyE keyE, string info)
        {
            if (eventAggregator == null)
            {
                eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            }

            eventAggregator.GetEvent<StatusInfoEvent>().Publish(new Tuple<KeyE, string>(keyE, info));
        }
        public static EventSourceLogger Logger
        {
            get { return Instance.Value; }
        }

        #region 101-200 :一般性信息
        [Event(101, Level = EventLevel.Informational, Message = "提示信息：“{0}”", Version = 1)]
        public void Prompt(string message, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(2, 0);//蓝
            string info = string.Format(Resources.Prompt + "：“{0}”", message);
            SendToStatusBar(keyE, message);
            if (this.IsEnabled())
            {
                this.WriteEvent(101, message, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(102, Level = EventLevel.Warning, Message = "警告信息：“{0}”", Version = 1)]
        public void Warn(string message, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(2, 2);//黄
            string info = string.Format(Resources.WarnInfo + "：“{0}”", message);
            SendToStatusBar(keyE, message);
            if (this.IsEnabled())
            {
                this.WriteEvent(102, message, memberName, sourceFilePath, sourceLineNumber);
            }
        }
        #endregion

        #region 201-300:系统信息，不通告
        [Event(201, Level = EventLevel.Informational, Message = "模块信息：加载Dll模块“{0}”到MEF目录。", Task = Tasks.Module, Version = 1)]
        public void LoadAssembly(string dllName, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(201, dllName, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(202, Level = EventLevel.Informational, Message = "模块信息：初始化模块{0};", Task = Tasks.Module, Version = 1)]
        public void InitModule(string moduleName, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(202, moduleName, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(203, Level = EventLevel.Informational, Message = "初始化表示器: {0};", Task = Tasks.Presenter, Version = 1)]
        public void InitPresenter(string presenterName, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            if (this.IsEnabled())
            {
                this.WriteEvent(203, presenterName, memberName, sourceFilePath, sourceLineNumber);
            }
        }
        #endregion

        #region 301-400:设置及注册
        [Event(301, Level = EventLevel.Critical, Message = "授权失败信息：“{0}”", Task = Tasks.Register, Version = 1)]
        public void RegisterAuthFail(string message, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(2, 3); //红
            string info = string.Format(Resources.AuthorizeFail + "：“{0}”", message);
            SendToStatusBar(keyE, info);
            if (this.IsEnabled())
            {
                this.WriteEvent(301, message, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(302, Level = EventLevel.Warning, Message = "选项设置信息：“{0}”，重启该软件后设置生效！", Task = Tasks.Config, Version = 1)]
        public void UpdateSetting(string message, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(2, 2);//黄
            string info = string.Format(Resources.ConfigOption + "：“{0}”，" + Resources.TakeEffectAfterRestart, message);
            SendToStatusBar(keyE, info);
            if (this.IsEnabled())
            {
                this.WriteEvent(302, message, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(303, Level = EventLevel.Warning, Message = "选项设置信息：您没有选择交易所“{0}”的任何交易对，至少选择1个或以上的交易对才能开通该功能：“{1}”。", Task = Tasks.Config, Version = 1)]
        public void NoPairSelected(string exchangeName, string functionInfo, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(2, 2); //黄
            string info = string.Format(Resources.ConfigOption + ":" + Resources.YourNeverSelectAnyPairInExchage, exchangeName, functionInfo);
            SendToStatusBar(keyE, info);
            if (this.IsEnabled())
            {
                this.WriteEvent(303, exchangeName, functionInfo, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(304, Level = EventLevel.Warning, Message = "API密钥信息: 您没有配置交易所“{0}”的密钥对，不能操作该功能“{1}”！", Task = Tasks.Config, Version = 1)]
        public void ApiKeyNotExist(string exchangeName, string functionInfo, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(2, 2);//黄
            SendToStatusBar(keyE, string.Format(Resources.NoApiKeyCantWork, exchangeName, functionInfo));

            if (this.IsEnabled())
            {
                this.WriteEvent(304, exchangeName, functionInfo, memberName, sourceFilePath, sourceLineNumber);
            }
        }
        #endregion

        #region 汇率获取

        [Event(401, Level = EventLevel.Informational, Message = "汇率信息：开始后台获取来自“{0}”的“{1}”汇率数据...", Task = Tasks.UpdateRate, Opcode = Opcodes.DownloadAsyncSuccess, Version = 1)]
        public void BeginDownloadRate(string apiName, string pairCode, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 0);//坤色
            SendToStatusBar(keyE, string.Format(Resources.RateInfoBeginQuery, apiName, pairCode));
            if (this.IsEnabled())
            {
                this.WriteEvent(401, apiName, pairCode, memberName, sourceFilePath, sourceLineNumber);
            }
        }



        [Event(402, Level = EventLevel.Error, Message = "汇率信息: 获取来自“{0}”的“{1}”的汇率数据失败！异步错误信息：{2}", Task = Tasks.UpdateRate, Opcode = Opcodes.AsyncError, Version = 1)]
        public void DownloadRateAsnycError(string apiName, string pairCode, string errorMessage, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 1); //艮色
            SendToStatusBar(keyE, string.Format(Resources.RateInfoQueryFail, apiName, pairCode, errorMessage));
            if (this.IsEnabled())
            {
                this.WriteEvent(402, apiName, pairCode, errorMessage, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        //Bug说明：当rate为decimal时WriteEvent不支持！但没有提示。会导至整个侦听失败！
        [Event(403, Level = EventLevel.Informational, Message = "汇率信息: 获取来自“{0}”的“{1}”的汇率数据成功，汇率为{2}", Task = Tasks.UpdateRate, Opcode = EventOpcode.Info, Version = 1)]
        public void UpdateRateSuccess(string apiName, string pairCode, string rate, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 2); //坎色
            SendToStatusBar(keyE, string.Format(Resources.RateInfoQuerySuccess, apiName, pairCode, rate));
            if (this.IsEnabled())
            {
                this.WriteEvent(403, apiName, pairCode, rate, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(404, Level = EventLevel.Error, Message = "汇率信息: 获取来自“{0}”的“{1}”的汇率数据时出现异常！异常信息为：{2}", Opcode = Opcodes.Exception, Task = Tasks.UpdateRate, Version = 1)]
        public void UpdateRateException(string apiName, string pairCode, string exceptionMessage, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 3); //巽色
            SendToStatusBar(keyE, string.Format("", apiName, pairCode, exceptionMessage));
            if (this.IsEnabled())
            {
                this.WriteEvent(404, apiName, pairCode, exceptionMessage, memberName, sourceFilePath, sourceLineNumber);
            }
        }
        #endregion

        #region 异步获取信息
        [Event(501, Level = EventLevel.Informational, Message = "{0}信息：开始后台获取来自交易所“{1}”的交易对“{2}”的{0}信息...", Opcode = Opcodes.BeginDownloadAsync, Task = Tasks.UpdateData, Version = 1)]
        public void BeginDownloadDataAsync(string dataType, string exchangeName, string pairCode, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 0);
            string info = string.Format(Resources.BeginDownladDataAsync, dataType, exchangeName, pairCode);
            if (this.IsEnabled())
            {
                this.WriteEvent(501, dataType, exchangeName, pairCode, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(502, Level = EventLevel.Error, Message = "{0}信息:在后台获取来自交易所“{1}”的交易对“{2}”的数据时出现异步错误，异步错误信息为：{3}。", Task = Tasks.UpdateData, Opcode = Opcodes.AsyncError, Version = 1)]
        public void DownloadDataAsyncError(string dataType, string exchangeName, string pairCode, string errorMessage, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 1);
            string info = string.Format(Resources.DownloadDataAsyncError, dataType, exchangeName, pairCode, errorMessage);
            SendToStatusBar(keyE, info);
            if (this.IsEnabled())
            {
                this.WriteEvent(502, dataType, exchangeName, pairCode, errorMessage, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(503, Level = EventLevel.Informational, Message = "{0}信息: 成功在后台获取来自交易所“{1}”的交易对“{2}”的{0}信息。", Task = Tasks.UpdateData, Opcode = Opcodes.DownloadAsyncSuccess, Version = 1)]
        public void UpdateDataSuccess(string dataType, string exchangeName, string pairCode, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 2);
            SendToStatusBar(keyE, string.Format(Resources.UpdateDataSuccess, dataType, exchangeName, pairCode));
            if (this.IsEnabled())
            {
                this.WriteEvent(503, dataType, exchangeName, pairCode, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(504, Level = EventLevel.Warning, Message = "{0}信息：后台获取交易所{1}的交易对{2}的{0}信息时出现异常，异常信息为：{3}。", Task = Tasks.UpdateData, Opcode = Opcodes.Exception, Version = 1)]
        public void UpdateDataException(string dataType, string exchangeName, string pairCode, string exceptionMessage, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 3);
            string info = string.Format(Resources.UpdateDataException, dataType, exchangeName, pairCode, exceptionMessage);
            SendToStatusBar(keyE, info);
            if (this.IsEnabled())
            {
                this.WriteEvent(504, dataType, exchangeName, pairCode, exceptionMessage, memberName, sourceFilePath, sourceLineNumber);
            }
        }
        #endregion

        #region 公用同步获取信息

        [Event(1001, Level = EventLevel.Informational, Message = "{0}信息: 成功获取交易所“{1}”的{0}信息", Task = Tasks.QueryFund, Version = 1)]
        public void QueryDataSuccess(string dataType, string exchangeName, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 6);
            SendToStatusBar(keyE, string.Format(Resources.QueryDataSuccess, dataType, exchangeName));
            if (this.IsEnabled())
            {
                this.WriteEvent(1001, dataType, exchangeName, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(1002, Level = EventLevel.Warning, Message = "{0}信息: 获取交易所“{1}”的{0}信息时出现异常，异常信息为：{2}", Opcode = EventOpcode.Info, Version = 1)]
        public void QueryDataException(string dataType, string exchangeName, string exceptionMessage, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 5);
            SendToStatusBar(keyE, string.Format("", dataType, exchangeName, exceptionMessage));
            if (this.IsEnabled())
            {
                this.WriteEvent(1002, dataType, exchangeName, exceptionMessage, memberName, sourceFilePath, sourceLineNumber);
            }
        }
        #endregion

        #region 下单和取消订单
        [Event(1101, Level = EventLevel.Informational, Message = "订单信息: 成功提交订单，订单号为{1}，来自“{0}”的反馈信息为：{2}", Task = Tasks.SubmitOrder, Version = 1)]
        public void SubmitOrderSuccess(string exchangeName, string orderId, string info, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 6);
            SendToStatusBar(keyE, string.Format("", exchangeName, orderId, info));
            if (this.IsEnabled())
            {
                this.WriteEvent(1101, exchangeName, orderId, info, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(1102, Level = EventLevel.Error, Message = "订单信息: 提交订单失败，订单号{1}，来自“{0}”的错误信息为：{2}", Task = Tasks.SubmitOrder, Version = 1)]
        public void SubmitOrderFail(string exchangeName, string orderId, string info, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 5);
            SendToStatusBar(keyE, string.Format(Resources.SumbitOrderFail, exchangeName, orderId, info));
            if (this.IsEnabled())
            {
                this.WriteEvent(1102, exchangeName, orderId, info, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(1103, Level = EventLevel.Informational, Message = "订单信息: 向交易所{0}提交订单时发生异常，来自“{0}”的反馈信息为：{1}", Opcode = EventOpcode.Info, Version = 1)]
        public void SubmitOrderException(string exchangeName, string info, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 7);
            SendToStatusBar(keyE, string.Format(Resources.SubmitOrderException, exchangeName, info));
            if (this.IsEnabled())
            {
                this.WriteEvent(1103, exchangeName, info, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(1201, Level = EventLevel.Informational, Message = "取消订单信息: 成功取消来自“{0}”的订单号为“{1}”的订单。", Task = Tasks.CancelOrder, Version = 1)]
        public void CancelOrderSuccess(string exchangeName, string orderId, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 6);
            SendToStatusBar(keyE, string.Format(Resources.CancelOrderSuccess, exchangeName, orderId));
            if (this.IsEnabled())
            {
                this.WriteEvent(1201, exchangeName, orderId, memberName, sourceFilePath, sourceLineNumber);
            }
        }

        [Event(1202, Level = EventLevel.Informational, Message = "取消订单信息: 取消来自交易所“{0}”的订单号为“{1}”的订单失败，来自“{0}”的错误信息为：{2}", Task = Tasks.CancelOrder, Version = 1)]
        public void CancelOrderFail(string exchangeName, string orderId, string info, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(3, 2);
            SendToStatusBar(keyE, string.Format(Resources.CancelOrderFail, exchangeName, orderId, info));
            if (this.IsEnabled())
            {
                this.WriteEvent(1202, exchangeName, orderId, memberName, sourceFilePath, sourceLineNumber);
            }
        }


        [Event(1203, Level = EventLevel.Warning, Message = "取消订单信息：取消来自交易所“{0}”的订单号为“{1}”的订单时发生异常，异常信息为:{2}。", Task = Tasks.CancelOrder, Opcode = Opcodes.Exception, Version = 1)]
        public void CancelOrderException(string exchangeName, string orderId, string exceptionMessage, [CallerMemberName]string memberName = "", [CallerFilePath]string sourceFilePath = "", [CallerLineNumber]int sourceLineNumber = -1)
        {
            KeyE keyE = new KeyE(2, 2);
            string info = string.Format(Resources.CancelOrderException, exchangeName, orderId, exceptionMessage);
            SendToStatusBar(keyE, exceptionMessage);
            if (this.IsEnabled())
            {
                this.WriteEvent(1203, exchangeName, orderId, exceptionMessage, memberName, sourceFilePath, sourceLineNumber);
            }
        }
        #endregion

    }
}
