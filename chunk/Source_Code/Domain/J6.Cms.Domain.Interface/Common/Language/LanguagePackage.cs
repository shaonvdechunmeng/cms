﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace J6.Cms.Domain.Interface.Common.Language
{
    public class LanguagePackage
    {
        private readonly IDictionary<string, string> _languagePack = new Dictionary<string, string>();

        /// <summary>
        /// 从XML中加载语言
        /// </summary>
        /// <param name="xml">XML内容</param>
        public void LoadFromXml(string xml)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(xml);

            XmlNodeList nodes = xd.SelectNodes("/lang/item");

            Type langKeyType=typeof(LanguagePackageKey);
            Type langType=typeof(Languages);
            LanguagePackageKey langKey;
            Languages lang;
            string strKey;

            IDictionary<Languages, string> dict = new Dictionary<Languages, string>();

            if (nodes != null)
                foreach (XmlNode node in nodes)
                {
                    dict.Clear();

                    if (node.Attributes != null)
                    {
                        foreach (XmlAttribute xa in node.Attributes)
                        {
                            if (xa.Name != "key")
                            {
                                lang = (Languages)System.Enum.Parse(langType, xa.Name);
                                dict.Add(lang, xa.Value);
                            }
                        }

                        strKey = node.Attributes["key"].Value;

                        if (System.Enum.IsDefined(langKeyType, strKey))
                        {

                            langKey = (LanguagePackageKey)System.Enum.Parse(langKeyType, strKey, true);
                            this.Add(langKey, dict);
                        }
                        else
                        {
                            this.AddOtherLangItem(node.Attributes["key"].Value, dict);
                        }
                    }
                }
        }



        private string GetXmlString(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllText(path);
            }
            return null;
        }


        /// <summary>
        /// 加载单独的XML
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="xmlPath"></param>
        public void LoadStandXml(Languages lang, string xmlPath)
        {
            XmlDocument xd = new XmlDocument();
            xd.Load(xmlPath);

            XmlNodeList nodes = xd.SelectNodes("/lang/item");
            string strKey;
            string strVal;

            IDictionary<Languages, string> dict = new Dictionary<Languages, string>();

            bool isRewrite = false;

            if (nodes != null)
                foreach (XmlNode node in nodes)
                {
                    dict.Clear();
                    strKey = node.Attributes["key"].Value;
                    strVal = node.InnerText;

                    this.AddOne(lang, strKey, strVal);

                    //如果不是文本注释,删除第一个节点并重新保存值
                    if (node.FirstChild != null && node.FirstChild.Name != "#cdata-section")
                    {
                        node.RemoveChild(node.FirstChild);
                        node.InsertBefore(xd.CreateCDataSection(strVal ?? ""), node.FirstChild);
                        isRewrite = true;
                    }
                }

            if (isRewrite)
            {
                xd.Save(xmlPath);
            }
        }

        private void AddOne(Languages lang, string key, string value)
        {
            var packKeyStr = String.Concat(key, ">", ((int) lang).ToString());
            if (!_languagePack.ContainsKey(packKeyStr))
            {
                _languagePack.Add(packKeyStr, value);
            }
        }

        /// <summary>
        /// 添加默认语言
        /// </summary>
        /// <param name="key"></param>
        /// <param name="langs"></param>
        public void Add(LanguagePackageKey key, IDictionary<Languages, string> langs)
        {
            String keyStr = ((int)key).ToString();
            foreach (KeyValuePair<Languages, string> pair in langs)
            {
                this.AddOne(pair.Key,keyStr,pair.Value);
            }
        }

        /// <summary>
        /// 添加自定义语言项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="langs"></param>
        public void AddOtherLangItem(string key, IDictionary<Languages, string> langs)
        {
            foreach (KeyValuePair<Languages, string> pair in langs)
            {
                _languagePack.Add(String.Concat(key, ">", ((int)pair.Key).ToString()), pair.Value);
            }
        }

        /// <summary>
        /// 获取语言项
        /// </summary>
        /// <param name="key"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public string Get(Languages lang,LanguagePackageKey key)
        {
            string dictKey = String.Concat(
                ((int)key).ToString(CultureInfo.InvariantCulture), 
                ">", 
                ((int)lang).ToString(CultureInfo.InvariantCulture));

            string outStr = null;
            if (_languagePack.TryGetValue(dictKey, out outStr))
            {
                return outStr;
            }

            throw new ArgumentNullException("key", 
                String.Format("({0}->{1})不包含当前语言的配置项!", 
                lang.ToString(),
                key.ToString()));
        }

        /// <summary>
        /// 获取自定义的语言项值，如果不存在此项，则返回String.Empty
        /// </summary>
        /// <param name="key"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public String GetValueByKey(Languages lang,string key)
        {
            string dictKey = String.Concat(key, ">", ((int)lang).ToString());
            string outStr = null;
            if (_languagePack.TryGetValue(dictKey, out outStr))
            {
                return outStr;
            }
            return String.Empty;
        }
    }
}