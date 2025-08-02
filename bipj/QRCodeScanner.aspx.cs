using System;
using System.Web;
using System.Web.Services;

namespace bipj
{
    public partial class QRCodeScanner : System.Web.UI.Page
    {
        [WebMethod]
        public static string ProcessScannedCode(string scannedData)
        {
            if (string.IsNullOrEmpty(scannedData))
                return "Invalid data.";

            string redirectUrl = "https://localhost:44369/VoucherRedemption.aspx?token=" + HttpUtility.UrlEncode(scannedData);
            return redirectUrl;
        }
    }
}
