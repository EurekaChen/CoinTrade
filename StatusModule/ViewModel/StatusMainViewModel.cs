
using System;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Eureka.CoinTrade.Base;
using Eureka.CoinTrade.Infrastructure.Event;
using Eureka.SystemE.Base;
using Eureka.SystemE.Thing;
using Microsoft.Practices.Prism.Events;
using Eureka.CoinTrade.Infrastructure.Properties;

namespace Eureka.CoinTrade.StatusModule.ViewModel
{
    [Export]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class StatusMainViewModel : BindableBase
    {
        [ImportingConstructor]
        public StatusMainViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<StatusInfoEvent>().Subscribe(DisplayInfo);
            Info = Resources.NoteStatusbar;
        }


        #region Foreground
        private SolidColorBrush foreground;
        public SolidColorBrush Foreground
        {
            get { return foreground; }
            set { SetProperty(ref foreground, value); }
        }
        #endregion



        private SolidColorBrush background;
        public SolidColorBrush Background
        {
            get
            {
                return background;
            }
            set
            {
                SetProperty(ref background, value);
            }
        }

        private string info;
        public string Info
        {
            get
            {
                return info;
            }
            set
            {
                SetProperty(ref info, value);
            }
        }



        private void DisplayInfo(Tuple<KeyE, string> info)
        {
            Info = info.Item2;
            if (info.Item1.Level == 2)
            {
                Background = GetRGBYColor(info.Item1.Sequence);
                Foreground = Brushes.Black;
            }
            else
            {
                ThingsE<SolidColorBrush> colorsE = SolidColorBrushEGenerator.ThingsE6;
                Background = colorsE[info.Item1].Thing;
                if (info.Item1.Sequence < 4)
                {
                    Foreground = Brushes.Black;
                }
                else
                {
                    Foreground = Brushes.White;
                }
            }
        }

        private SolidColorBrush GetRGBYColor(int sequence)
        {
            switch (sequence)
            {
                case 0:
                    return new SolidColorBrush(Colors.LightBlue);
                case 1:
                    return new SolidColorBrush(Colors.LightGreen);
                case 2:
                    return new SolidColorBrush(Colors.Yellow);
                case 3:
                    return new SolidColorBrush(Colors.Red);
                default:
                    return new SolidColorBrush(Colors.LightGray);
            }
        }
    }
}
