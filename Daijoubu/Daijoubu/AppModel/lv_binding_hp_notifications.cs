﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Daijoubu.AppModel
{
    public class lv_binding_hp_notifications
    {
        public string MainLabel { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Clock { get; set; }
        public string Percent { get; set; }

        public TimeSpan _tspan { get; set; }
        public string TableName { get; set; }
        public int ItemID { get; set; }
    }
}
