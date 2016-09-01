using System;
using System.Reflection;
namespace Eureka.CoinTrade
{
    public class AboutViewModel
    {
        private const string propertyNameTitle = "Title";
        private const string propertyNameDescription = "Description";
        private const string propertyNameProduct = "Product";
        private const string propertyNameCopyright = "Copyright";
        private const string propertyNameCompany = "Company";

        #region Properties
        /// <summary>
        /// 获取产品标题信息.
        /// </summary>
        public string ProductTitle
        {
            get
            {
                string result = CalculatePropertyValue<AssemblyTitleAttribute>(propertyNameTitle);
                return result;
            }
        }

        /// <summary>
        /// 产品版本信息.
        /// </summary>
        public string Version
        {
            get
            {
                string result = string.Empty;
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version != null)
                {
                    result = version.ToString();
                }

                return result;
            }
        }

        /// <summary>
        /// 应用描述信息.
        /// </summary>
        public string Description
        {
            get { return CalculatePropertyValue<AssemblyDescriptionAttribute>(propertyNameDescription); }
        }

        /// <summary>
        ///  产品全称.
        /// </summary>
        public string Product
        {
            get { return CalculatePropertyValue<AssemblyProductAttribute>(propertyNameProduct); }
        }

        /// <summary>
        /// 产品版权信息.
        /// </summary>
        public string Copyright
        {
            get { return CalculatePropertyValue<AssemblyCopyrightAttribute>(propertyNameCopyright); }
        }

        /// <summary>
        /// 产品公司名称.
        /// </summary>
        public string Company
        {
            get { return CalculatePropertyValue<AssemblyCompanyAttribute>(propertyNameCompany); }
        }


        /// <summary>
        /// 链接地址.
        /// </summary>
        public string Uri
        {
            get { return "http://www.ee-studio.com"; }
        }
        #endregion


        /// <summary>
        /// 从程序集信息中获得软件相关信息。
        /// </summary>
        /// <typeparam name="T">要取得数据的属性类型.</typeparam>
        /// <param name="propertyName">属性使用的名称.</param>    
        private string CalculatePropertyValue<T>(string propertyName)
        {
            string result = string.Empty;
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(T), false);
            if (attributes.Length > 0)
            {
                T attrib = (T)attributes[0];
                PropertyInfo property = attrib.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    result = property.GetValue(attributes[0], null) as string;
                }
            }
            return result;
        }

    }
}
