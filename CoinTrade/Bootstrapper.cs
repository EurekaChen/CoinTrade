using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using Eureka.CoinTrade.Infrastructure;
//using Eureka.CoinTrade.Properties;
using Eureka.CoinTrade.Infrastructure.Properties;
using MahApps.Metro.Controls;
using Microsoft.Practices.Prism.Logging;
using Microsoft.Practices.Prism.MefExtensions;
using Newtonsoft.Json;

namespace Eureka.CoinTrade
{
    class Bootstrapper : MefBootstrapper
    {
        protected override ILoggerFacade CreateLogger()
        {
            return EventSourceLogger.Logger;
        }

        protected override void ConfigureAggregateCatalog()
        {
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(typeof(Bootstrapper).Assembly));

            string exeLocation = Assembly.GetEntryAssembly().Location;
            string exePath = System.IO.Path.GetDirectoryName(exeLocation);

            string modulePath = exePath + "\\Module";
            var moduleCatalog = new DirectoryCatalog(modulePath);
            this.AggregateCatalog.Catalogs.Add(moduleCatalog);

            string apiPath = exePath + "\\Api";
            var apiCatalog = new DirectoryCatalog(apiPath);
            this.AggregateCatalog.Catalogs.Add(apiCatalog);

            AddCatalog("Eureka.CoinTrade.Infrastructure.dll");
        }
        private void AddCatalog(string dllName)
        {
            Assembly assembly = Assembly.LoadFrom(dllName);
            this.AggregateCatalog.Catalogs.Add(new AssemblyCatalog(assembly));
            EventSourceLogger.Logger.LoadAssembly(dllName);
        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();
            Container.ComposeExportedValue(Container);
        }

        protected override DependencyObject CreateShell()
        {
            //如果Shell没有依赖,可以使用 new Shell(),建议一直使用如下方式从容器中获得Shell以备依赖注入。           
            return this.Container.GetExportedValue<Shell>();
            // return new Shell();
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            App.Current.MainWindow = (MetroWindow)this.Shell;
            App.Current.MainWindow.Show();

            /* 2016-09-01 去掉了认证
            if (!File.Exists("cointrade.key"))
            {
                //从本地获取
                //string info = Resources.ResourceManager.GetString("NoCoinTradeKey");
                //统一到Infrastructure中获取。
                //string info = Infrastructure.Properties.Resources.NoCoinTradeKey;
                //强类型化获取
                string info = Resources.NoCoinTradeKey;

                EventSourceLogger.Logger.RegisterAuthFail(info);

                string caption = Resources.NoLicense;

                MessageBox.Show(info, caption, MessageBoxButton.OK);
                App.Current.MainWindow.Close();
                //程序退出了，还在获取报价信息？！

            }
            else
            {
                FileInfo keyFileInfo = new FileInfo("cointrade.key");

                //解压
                string base64Text = Setting.Decompress(keyFileInfo);
                //解密
                byte[] encryptedBytes = Convert.FromBase64String(base64Text);
                byte[] decryptedBytes = Decrypt(encryptedBytes);
                string registerInfoJson = Encoding.UTF8.GetString(decryptedBytes);

                RegisterInfo registerInfo = JsonConvert.DeserializeObject<RegisterInfo>(registerInfoJson);
                Container.ComposeExportedValue<RegisterInfo>(registerInfo);
                App.Current.MainWindow.DataContext = registerInfo;

                try
                {
                    //网络上获取时间
                    string url = "http://cointrade.pw/s/date.php";
                    using (WebClient webClient = new WebClient())
                    {
                        string dateText = webClient.DownloadString(url);
                        DateTime today = DateTime.Parse(dateText);
                        today = today + new TimeSpan(12, 0, 0);
                        if (registerInfo.ExpireDate < today)
                        {
                            string expiredInfo = Resources.ExpiredInfo;
                            string message = expiredInfo + string.Format("{0:yyyy-MM-dd }", registerInfo.ExpireDate);
                            EventSourceLogger.Logger.RegisterAuthFail(message);

                            string expired = Resources.Expired;
                            MessageBox.Show(message, expired, MessageBoxButton.OK);
                            App.Current.MainWindow.Close();
                        }
                    }
                }
                catch
                {
                    if (registerInfo.ExpireDate < DateTime.Today)
                    {
                        string message = Resources.ExpiredInfo + string.Format("{0:yyyy-MM-dd }", registerInfo.ExpireDate);
                        EventSourceLogger.Logger.RegisterAuthFail(message);

                        string expired = Resources.Expired;
                        MessageBox.Show(message, expired, MessageBoxButton.OK);
                        App.Current.MainWindow.Close();
                    }
                }

                byte[] macBytes = GetMacAddressBytes();
                string machineCode = Convert.ToBase64String(macBytes);

                if (registerInfo.MachineCode != machineCode)
                {
                    string message = Resources.MachineCodeNotMatchInfo + machineCode;
                    MessageBox.Show(message, Resources.MachineCodeNotMatch, MessageBoxButton.OK);
                    EventSourceLogger.Logger.RegisterAuthFail(message);
                    App.Current.MainWindow.Close();
                }

                if (!File.Exists("SelectedPairs.key"))
                {

                    if (registerInfo.IsEnabledAll == true || registerInfo.EnabledPairs == null)
                    {
                        File.Copy("AllSupportPair.key", "SelectedPairs.key", true);

                    }
                    else
                    {
                        //由于EnabledPairs和SelectedPairs可能在发布新的Api 之前，所以有些API对应的项可能为null，而不是空！
                        //这在选项中进行选择时需要考虑为null的情况。
                        string json = JsonConvert.SerializeObject(registerInfo.EnabledPairs);
                        Setting.WriteJsonToCompressFile("SelectedPairs.key", json);
                    }
                }
            }
            */
        }

        private byte[] Decrypt(byte[] encrypteBytes)
        {
            using (RSACryptoServiceProvider rsaCrypto = GetRSACrypto())
            {
                //进行分段解密：
                int keySize = rsaCrypto.KeySize / 8;
                byte[] buffer = new byte[keySize];
                using (MemoryStream inputMemoryStream = new MemoryStream(encrypteBytes), outputMemoryStream = new MemoryStream())
                {
                    int bufferLength = inputMemoryStream.Read(buffer, 0, keySize);
                    while (bufferLength > 0)
                    {
                        byte[] needDecryptFraction = new byte[bufferLength];
                        Array.Copy(buffer, 0, needDecryptFraction, 0, bufferLength); 
                        //莫名其秒，换windows8.1后这里出错了！
                        //后来发现不是Win8.1原因，而是key的原因！有些key会出错！
                        //TODO:还未找到出错原因！
                        //2014-10-08还是找不到原因！发现在家里电脑和单位电脑上生成的key都不行！
                        //但一些老的key可以！难道Customer生在的key有问题了？
                        //电脑提示的错误不一样，办公室电脑为“参数错误”，家里电脑提示为“不正确的数据”！
                        byte[] decryptedFraction = rsaCrypto.Decrypt(needDecryptFraction, false);                       
                    
                        outputMemoryStream.Write(decryptedFraction, 0, decryptedFraction.Length);
                        bufferLength = inputMemoryStream.Read(buffer, 0, keySize);
                    }
                    byte[] resultBytes = outputMemoryStream.ToArray();    //得到解密结果    
                    return resultBytes;
                }
            }
        }

        //因为.NET库提供的RSACryptoServiceProvider类只支持公钥加密，私钥解密，所以暂时用私钥解密
        //有必要时可以参见：http://www.knowsky.com/561737.html 实现公钥解密并附有代码 RSATest.
        private static RSACryptoServiceProvider GetRSACrypto()
        {
            RSACryptoServiceProvider rsaCrypto = new RSACryptoServiceProvider();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetName().Name + ".RSAKey.xml";
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            using (TextReader textReader = new StreamReader(stream))
            {
                string rsaKey = textReader.ReadToEnd();
                rsaCrypto.FromXmlString(rsaKey);
            }
            return rsaCrypto;
        }

        //原先的算法可能存在的问题：1、当用VPN时得到的是空的MAC地址。2断开网络时得不到地址。 注：网卡禁用还是会得不到地址的！

        //private byte[] GetMacAddressBytes()
        //{
        //    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
        //    {                 
        //        if (nic.OperationalStatus == OperationalStatus.Up)
        //        {                   
        //            return nic.GetPhysicalAddress().GetAddressBytes();   //ToString()则获得Mac地址。            
        //        }
        //    }
        //    return new byte[6];
        //}

        /// <summary>
        /// 通过检测每个MAC，获得第一个MAC地址。
        /// </summary>
        /// <returns></returns>
        private byte[] GetMacAddressBytes()
        {
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                PhysicalAddress mac = nic.GetPhysicalAddress();
                //微软的 Microsoft ISATAP Adapter 有8个字节数据。
                //很多隧道没有物理地址。
                if (mac.GetAddressBytes().Length == 6)
                {
                    return mac.GetAddressBytes();
                }
            }
            return new byte[6];
        }
    }
}
