   M S B A M L   `   `   `  ����W  SEureka.CoinTrade.AlertModule, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null_4clr-namespace:Eureka.CoinTrade.AlertModule.ViewModel&Eureka.CoinTrade.AlertModule.ViewModel  5      8    2Eureka.CoinTrade.AlertModule.View.CoupleAlertsView   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35N JEureka.Localization, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullW SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      Flocal4clr-namespace:Eureka.CoinTrade.AlertModule.ViewModel     5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      '  !Eureka.Localization.ResxExtension    DefaultResxName$0  *Eureka.CoinTrade.AlertModule.View.Resource��5       � 	Resources 5
         	boolToVis?   9Eureka.CoinTrade.AlertModule.ViewModel.PriceTypeConverter  priceTypeConverterB   <Eureka.CoinTrade.AlertModule.ViewModel.MonitorStateConverter  monitorStateConverter��%W   &	        &	      &	 &     �� 5   
   6'     5   
   6#     5   
   6&   5      .��5      �� .+�5   
   � $��Center=�6   .+�5      �� ��6     *_Every+6/   �� 5      $��30��66   ��6   �� *UpdateInterval+6C   �� 5      ��6     *	_Second+60   �� 5      ��6     *_RefreshPrice+66   d� 5      -   5        $��WatchToggle��5      ��6.   �� *WatchCommand+��6O   ��  �� ElementName$ WatchToggle�� �� Path$ 	IsChecked���6�     *_BeginAutoMonitor+6�   �� 5      $��50,0,0,0q�65   ��6   �� *SaveCommand+��6G     *_SaveYourAlerts+5      �� 5   
   $	��345��6   $
��Auto=�6$   .��5      �� -   5        $��AlertsControl��5      $	��Top=�61   ��6I   �� *Alerts+
 � Tag 6i   �� *DeleteItemCommand+��5      �� 5      .R�5      � .+�5      � $
��1030��6E   {�6(   �� *IsOpen+��5   "   �� 5   &    �� Color 66   �� *BackgroundColor+ � ToolTip 5!   "   � 5"   &   .+�5#   *   �� ��64     *_WhenDiffHighThan+6V   �� 5$   *   ��64   �� *PriceDifference+6W   �� 5%   *   ��64     *_BackgroundWillBe+6V   �� 5&   *   $��20��64   ��5'   .   �� 5(   2    6B   �� *
AlertColor+5*   +   � 5+   *   y�64   �� *IsPlaySound+ �� 	Converter# [  .+�5,   .   �� ��68     *_AndPlaySound+6V   �� 5-   .   ��68   �� *	SoundFile+6U   5.   +   5/   '   .+�51   "   �� $��14(�6,   $��Center=�6:   ��6U     *_NowTradePair+6s   �� 52   "   $
��Bold"�6,   ��6>   �� *
PairCode+53   #   �� 54   "   $��14(�6,   $��Center=�6:   ��6U     *_InExchange+6q   �� 55   "   $
��Bold"�6,   ��6>   �� *Exchange1AbbrName+56   #   �� 57   "   $��14(�6,   $��Center=�6:   ��6U     *_s+6h   �� 58   "   $��PriceTypeTextBlock1��6?   $��Black"�6,   ��59   &     5:   *   
   Key$ PriceTypeTextBlock1.Text��6/   	   BindingPath$	 
PriceType1�5;   /   
   BindingConverter#
 [ 5<   /   5>   #   �� 5?   "   $��	ExtraBold"�6,   ��6C   �� *Price1+�� 5@   "   $��14(�6,   $��Center=�6:   ��6U     *	_HighThan+6o   �� 5A   "   $
��Bold"�6,   ��6>   �� *Exchange2AbbrName+6c   �� 5B   "   $��14(�6,   $��Center=�6:   ��6U     *_s+6h   �� 5C   "   $��PriceTypeTextBlock2��6?   $��Black"�6,   ��5D   &     5E   *   $ PriceTypeTextBlock2.Text��6/   $	 
PriceType2�5F   /   #
 [ 5G   /   5I   #   �� 5J   "   $��	ExtraBold"�6,   ��6C   �� *Price2+�� 5K   "   $��14(�6,   $��Center=�6:   ��6U     *_Diff+�� 5L   "   $��
ExtraBlack"�6,   ��6D   �� *CurrentDifference+�� 5M   "   $��StateTexbBlock��6>   $
��Bold"�6,   ��5N   &     5O   *   $ StateTexbBlock.Text��6/   $	 State�6I   #
 [ 6]   5Q   #   5R      d� 5S      $��
OpenButton��6+    d� 	IsChecked 6=   �� *IsOpen+.��5T   "   �� $��ToggleTextBlock��6,   ��5U   &     5V   *   $ ToggleTextBlock��6/   $	 
ToggleText�6E   5X   #   5Y      �� 5Z      ��6%   �� $ AlertsControl��$	 Tag���6^   �� ��6{     *_DeleteAlert+6�   5[      5\      5^      5_      5`      5a      