using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SMT.SAAS.Platform.Model
{
    public class NewsModel
    {
        /// <summary>
        /// 
        /// </summary>
        public string NEWSID
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public string NEWSTYPEID
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public string NEWSTITEL
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public byte[] NEWSCONTENT
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public string READCOUNT
        {
            set;
            get;
        }
        /// <summary>
        /// 
        /// </summary>
        public string COMMENTCOUNT
        {
            set;
            get;
        }
        /// <summary>
        /// 0:未发布，1:已发布,2:已关闭
        /// </summary>
        public string NEWSSTATE
        {
            set;
            get;
        }
        /// <summary>
        /// 是否发布在首页
        /// </summary>
        public bool ISRELEASE
        { get; set; }

        /// <summary>
        /// 是否为图片新闻
        /// </summary>
        public bool ISIMAGE
        { get; set; }

        /// <summary>
        /// 发布对象集合ID
        /// </summary>
        public ObservableCollection<DISTR> VIEWER
        { get; set; }

        /// <summary>
        /// 发布对象集合ID
        /// </summary>
        public DateTime UPDATEDATE
        { get; set; }
        //UPDATEDATE

        public bool ISPOPUP
        { get; set; }

        public DateTime ENDDATE
        { get; set; }

        public string PUTDEPTNAME
        {
            set;
            get;
        }
        public string PUTDEPTID
        {
            set;
            get;
        }

    }

    public class DISTR
    {
        public string VIEWERS
        {
            get;
            set;
        }
        public string MODELNAMES
        {

            get;
            set;
        }
    }
}
