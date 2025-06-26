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

            // TODO: Add your validation and voucher logic here, e.g.:
            // Check if scannedData is a valid voucher token in DB
            // Mark voucher as used if valid
            // Return error message if invalid

            // For demo, return a redirect URL:
            string redirectUrl = "https://localhost:44369/VoucherRedemption.aspx?token=" + HttpUtility.UrlEncode(scannedData);
            return redirectUrl;
        }
    }
}
