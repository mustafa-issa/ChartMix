﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChartsMix.Models
{
    public class PieGroupChartDataModel
    {
        public virtual List<CustomLineSeries> Result { get; set; }
        public List<string> Dates { get; set; }
    }
}