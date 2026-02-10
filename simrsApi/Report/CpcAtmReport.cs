using cpcApi.Model.DTO.Cpc.Report;
using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace cpcApi.Report
{
    public partial class CpcAtmReport : DevExpress.XtraReports.UI.XtraReport
    {
        public CpcAtmReport()
        {
            InitializeComponent();

            // ROOT = HEADER
            //DataSource = new List<CpcReportHeaderDto> { data };
        }
    }
}
