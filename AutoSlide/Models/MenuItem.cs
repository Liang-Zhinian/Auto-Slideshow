﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSlide.Models
{
    //選單Model
    public class MenuItem
    {
        public string Icon { get; set; }
        public string Title { get; set; }
        public Type View { get; set; }
    }
}
