﻿using ChartsMix.Models;
using System.Web.Mvc;
using Highsoft.Web;
using Highsoft.Web.Mvc;
using Highsoft.Web.Mvc.Charts;
using System;
using System.Collections.Generic;

namespace ChartsMix.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private ChartsDatabaseManager db;
        // GET: home/index
        public ActionResult Index()
        {
            db = new Models.ChartsDatabaseManager();
            var model = new DashbordModel();
            PrepareChartsModel(model);
            
            return View(model);

            
            
        }

        public ActionResult LineLabels()
        {
            List<double> tokyoValues = new List<double> { 7.0, 6.9, 9.5, 14.5, 18.2, 21.5, 25.2, 26.5, 23.3, 18.3, 13.9, 9.6 };
            List<double> nyValues = new List<double> { -0.2, 0.8, 5.7, 11.3, 17.0, 22.0, 24.8, 24.1, 20.1, 14.1, 8.6, 2.5 };
            List<double> berlinValues = new List<double> { -0.9, 0.6, 3.5, 8.4, 13.5, 17.0, 18.6, 17.9, 14.3, 9.0, 3.9, 1.0 };
            List<double> londonValues = new List<double> { 3.9, 4.2, 5.7, 8.5, 11.9, 15.2, 17.0, 16.6, 14.2, 10.3, 6.6, 4.8 };
            List<LineSeriesData> tokyoData = new List<LineSeriesData>();
            List<LineSeriesData> nyData = new List<LineSeriesData>();
            List<LineSeriesData> berlinData = new List<LineSeriesData>();
            List<LineSeriesData> londonData = new List<LineSeriesData>();


 
            tokyoValues.ForEach(p => tokyoData.Add(new LineSeriesData { Y = p }));
            nyValues.ForEach(p => nyData.Add(new LineSeriesData { Y = p }));
            berlinValues.ForEach(p => berlinData.Add(new LineSeriesData { Y = p }));
            londonValues.ForEach(p => londonData.Add(new LineSeriesData { Y = p }));

            ViewData["tokyoData"] = tokyoData;
            ViewData["nyData"] = nyData;
            ViewData["berlinData"] = berlinData;
            ViewData["londonData"] = londonData;

            return View();
        }

        [HttpPost]
        public ActionResult Index(DashbordModel model, int[] pieIds, int[] barIds, int[] lineIds)
        {
            PrepareChartsModel(model);

            //if (pieIds != null && pieIds.Length > 0)
            //{
            //    model.PieModel.Data = new ChartsDatabaseManager().GetPieChartMeters(pieIds,DateTime.MinValue,DateTime.MaxValue,model.PieModel.period);
            //}
            List<string> dates;
            //if (barIds != null && barIds.Length > 0)
            //{
                model.barChartModel.Data = new ChartsDatabaseManager().GetBarChartMeters(barIds, DateTime.MinValue, DateTime.MaxValue, model.barChartModel.period, out dates);
            //}
            model.barChartModel.Dates = dates;
            model.lineChartModel.Data = new ChartsDatabaseManager().GetLineChartMeters(lineIds, DateTime.MinValue, DateTime.MaxValue, model.lineChartModel.period, out dates);

            model.lineChartModel.Dates = dates;

            //if (compIds != null && compIds.Length > 0)
            //{
            //    model.PieModel.Data = new ChartsDatabaseManager().GetPieChartMeters(compIds);
            //}

            //if (lineIds != null && lineIds.Length > 0)
            //{
            //    model.PieModel.Data = new ChartsDatabaseManager().GetPieChartMeters(lineIds);
            //}
            return View(model);
        }

        private void PrepareChartsModel(DashbordModel model)
        {
            db = new Models.ChartsDatabaseManager();
            model.PieModel.TreeRoot = db.GetMeterTree();
        }
    }
}