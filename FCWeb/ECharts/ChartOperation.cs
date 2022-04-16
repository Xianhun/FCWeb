using OAWeb.Echarts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OAWeb.ECharts
{
    /// <summary>
    /// 图表操作数据处理类
    /// </summary>
    public class ChartOperation
    {
        /// <summary>
        /// 线形图数据处理
        /// </summary>
        /// <param name="lengendData">图例数据</param>
        /// <param name="chartxAxisData">x轴数据</param>       
        /// <param name="seriesData">图表数据</param>
        /// <param name="xName">x轴名称</param>
        /// <param name="yName">y轴名称</param>
        /// <returns>图标所需的相关数据的json字符串</returns>
        public static string ChartLineDataProcess(List<object> lengendData, List<object> chartxAxisData, List<object> seriesData, string headline = null, string xName = null, string yName = null)
        {
            var jsondata = new
            {
                legend = lengendData,
                xAxis = chartxAxisData,
                series = seriesData,
                title = headline,
                xAxisName = xName,
                yAxisName = yName
            };
            return jsondata.ToJson();
        }

    }
}