using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eureka.CoinTrade.Base;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.ViewModel;
namespace Eureka.CoinTrade.Infrastructure
{
    public class Presenter<TView, TViewModel>
        where TView : IView
        where TViewModel : BindableBase
    {
        [Import(AllowDefault = true)]
        public TView View { get; set; }

        [Import(AllowDefault = true)]
        public TViewModel ViewModel { get; set; }

        [Import]
        protected IEventAggregator eventAggregator;

        [Import]
        protected IRegionManager regionManager;

        [Import]
        protected ILoggerFacade logger;

        protected void Log(string info)
        {
            logger.Log(info, Category.Info, Priority.None);
        }

        public virtual void Initialize()
        {
            EventSourceLogger.Logger.InitPresenter(GetType().ToString());
            if (ViewModel != null && View != null) View.DataContext = ViewModel;
        }
    }
}
