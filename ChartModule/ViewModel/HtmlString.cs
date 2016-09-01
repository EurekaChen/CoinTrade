
namespace Eureka.CoinTrade.ChartModule.ViewModel
{
    public static class HtmlString
    {
        static string begin = @"<!DOCTYPE HTML><html><head><meta http-equiv='Content-Type' content='text/html; charset=utf-8'>";
        static string styleScale75 = @"<style type='text/css'>
                            html{overflow:hidden;}
                            iframe
                            {
                                -moz-transform: scale(0.75, 0.75); 
                                -webkit-transform: scale(0.75, 0.75); 
                                -o-transform: scale(0.75, 0.75);
                                -ms-transform: scale(0.75, 0.75);
                                transform: scale(0.75, 0.75); 
                                -moz-transform-origin: top left;
                                -webkit-transform-origin: top left;
                                -o-transform-origin: top left;
                                -ms-transform-origin: top left;
                                transform-origin: top left;
                            }";
        static string stylePeriod = @"	.position
                            {
                                position : absolute;   
                                top:-170px;
                                left:-60px;
                                width    : 1280px;
                                height   : 580px;
                            }
                            </style>";
        static string styleDepth = @"	.position
                            {
                                position: absolute;
                                top: -650px;
                                left: -60px;
                                width: 1280px;
                                height: 1160px;
                            }
                            </style>";

        static string styleOneWrapDepth = @"	.position
                            {
                                position: absolute;
                                top: -690px;
                                left: -60px;
                                width: 1280px;
                                height: 1200px;
                            }
                            </style>";

        static string script = @" <script type='text/javascript'>
                            document.oncontextmenu=function(e){return false;} 
                            function noError() { return true; }
                            window.onerror = noError;
                            </script>";
        static string body = @"
                            </head>
                            <body>
                            <iframe class='position' scrolling='no' src='";
        // string src="http://www.cryptocoincharts.info/v2/pair/ltc/cny/okcoin/10-days";
        static string end = @" '></iframe>
                            <!--制作了一个层，并把透明度设低，从而使得右键无效可以作用在层上-->
                            <div class='position'  style='filter:alpha(Opacity=1);-moz-opacity:0.1;opacity: 0.1;z-index:100; background-color:#ffffff;'></div>
                            </body>
                            </html>";

        public static string GetPeriodHtml(string src)
        {
            return begin + styleScale75 + stylePeriod + script + body + src + end;
        }

        public static string GetDepthHtml(string src)
        {
            return begin + styleScale75 + styleDepth + script + body + src + end;
        }

        public static string GetOneWrapDepthHtml(string src)
        {
            return begin + styleScale75 + styleOneWrapDepth + script + body + src + end;
        }
    }
}
