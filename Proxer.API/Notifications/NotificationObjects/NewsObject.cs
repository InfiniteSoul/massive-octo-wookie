﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proxer.API.Notifications.NotificationObjects
{
    /// <summary>
    /// 
    /// </summary>
    public class NewsObject : INotificationObject
    {
        /// <summary>
        /// 
        /// </summary>
        public NewsObject()
        {
            this.Typ = "News";
        }

        /// <summary>
        /// 
        /// </summary>
        public string Typ { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public string Message
        {
            get { return this.description; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string nid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string time { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string mid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string pid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string description { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string image_id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string image_style { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string subject { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string hits { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string thread { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string uname { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string posts { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string catid { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string catname { get; set; }
    }
}
