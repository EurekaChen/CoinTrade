﻿using System.ComponentModel.Composition;
using System.Windows.Controls;
using Eureka.CoinTrade.Infrastructure;

namespace Eureka.CoinTrade.AlertModule.View
{

    [Export]
    public partial class SingleAddAlertView : UserControl, IView
    {
        public SingleAddAlertView()
        {
            InitializeComponent();
        }
    }
}
