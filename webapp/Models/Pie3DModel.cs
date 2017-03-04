﻿using System.Collections.Generic;
using Highsoft.Web.Mvc.Charts;
using System.Web.Mvc;

namespace ChartsMix.Models
{
    public class Pie3DModel
    {
        public Pie3DModel()
        {
            TreeRoot = new Meter();
        }
        public Meter TreeRoot { get; set; }
        public List<PieSeriesData> Data { get; set; }
        public PiePeriod period { get; set; }
    }
}