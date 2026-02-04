using DevExpress.XtraReports.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace cpcApi.Report
{
    public partial class RepOrderPengisianAtm : DevExpress.XtraReports.UI.XtraReport
    {
        public RepOrderPengisianAtm()
        {
            InitializeComponent();
        }

        int _bankIndex = 0;

        //private void grBank_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        //{
        //    if (_bankIndex > 0)
        //    {
        //        grBank.PageBreak = DevExpress.XtraReports.UI.PageBreak.BeforeBand;
        //    }
        //    else
        //    {
        //        grBank.PageBreak = DevExpress.XtraReports.UI.PageBreak.None;
        //    }

        //    _bankIndex++;
        //}

        private void grCstd_BeforePrint(object sender, CancelEventArgs e)
        {
            if (_bankIndex > 0)
            {
                grCstd.PageBreak = DevExpress.XtraReports.UI.PageBreak.BeforeBand;
            }
            else
            {
                grCstd.PageBreak = DevExpress.XtraReports.UI.PageBreak.None;
            }

            _bankIndex++;
        }
    }
}
