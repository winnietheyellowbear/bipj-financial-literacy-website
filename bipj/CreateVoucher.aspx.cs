using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Security.Cryptography;

using Newtonsoft.Json;
// Go to Tools > NuGet Package Manager > Manage NuGet Packages for Solution
// Install Newtonsoft.Json

using GemBox.Email;
using GemBox.Email.Smtp;
// Manage NuGet Packages > Install GemBox.Email

namespace bipj
{
    public partial class CreateVoucherAuto : System.Web.UI.Page
    {
        Sponsor_Voucher sponsor_voucher = new Sponsor_Voucher();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Page.RegisterAsyncTask(new PageAsyncTask(async () =>
                {
                    string email_id = Request.QueryString["Email_ID"];
                    sponsor_voucher = sponsor_voucher.GetEmailByEmailID(email_id);

                    await fill_in_fields(sponsor_voucher.Subject + " " + sponsor_voucher.Message);
                }));
            }
        }

        public async Task fill_in_fields(string text)
        {
            string apiKey = "";
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer " + apiKey);

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new
                    {
                        role = "system",
                        content = @"You are an assistant that extracts sponsor voucher information from text messages. 
                        Only return a JSON object with these fields:

                        {
                          ""SponsorName"": ""..."",
                          ""Description"": ""..."",
                          ""ValidityValue"": ""..."",
                          ""ValidityUnit"": ""..."",
                          ""PointsRequired"": ""...""
                        }

                        Leave any field blank if the info is not provided. No explanation, just the JSON."
                    },
                    new { role = "user", content = text }
                }
            };

            string json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
            string responseString = await response.Content.ReadAsStringAsync();

            dynamic result = JsonConvert.DeserializeObject(responseString);
            string replyJson = result.choices[0].message.content;

            var extracted = JsonConvert.DeserializeObject<Dictionary<string, string>>(replyJson);

            tb_Sponsor_Name.Text = extracted["SponsorName"];
            tb_Desc.Text = extracted["Description"];
            tb_Validity.Text = extracted["ValidityValue"];
            ddl_Validity.SelectedValue = extracted["ValidityUnit"];
            tb_Points_Required.Text = extracted["PointsRequired"];
        }

        protected void btn_create_Click(object sender, EventArgs e)
        {
            int result = 0;

            string name = tb_Sponsor_Name.Text;
            string description = tb_Desc.Text;
            string validity = tb_Validity.Text + " " + ddl_Validity.SelectedValue;
            int points_required = int.Parse(tb_Points_Required.Text);
            string token = GenerateToken();

            Staff_Voucher staff_voucher = new Staff_Voucher(name, description, validity, points_required, token);
            result = staff_voucher.VoucherInsert();

            if (result > 0)
            {
                Email(token);

                string email_id = Request.QueryString["Email_ID"];
                sponsor_voucher.StatusUpdate(email_id);

                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Voucher created. 😊'); window.location='VoucherSponsor.aspx';", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to create voucher. 😞');", true);
            }
        }

        protected void Email(string token)
        {
            string email_id = Request.QueryString["Email_ID"];
            sponsor_voucher = sponsor_voucher.GetEmailByEmailID(email_id);

            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            var message = new MailMessage(
                new MailAddress("usagitheyellowrabbit@gmail.com", "Voucher Team"),
                new MailAddress(sponsor_voucher.Email, "Valued Sponsor"));

            string voucherUrl = $"https://localhost:44369/VoucherManagement.aspx?token={token}";
            string qrCodeUrl = "https://localhost:44369/QRCodeScanner.aspx";

            StringBuilder bodyText = new StringBuilder();
            bodyText.AppendLine("Dear Sponsor,");
            bodyText.AppendLine();
            bodyText.AppendLine("We are excited to inform you that your sponsor voucher has been successfully created! 🎉");
            bodyText.AppendLine();
            bodyText.AppendLine("To manage and use your voucher, please follow the links below:");
            bodyText.AppendLine();
            bodyText.AppendLine($"1. **[Voucher Management]( {voucherUrl} )**: Enable or disable your voucher.");
            bodyText.AppendLine($"2. **[QR Code Scanner]( {qrCodeUrl} )**: Scan the voucher when the customer presents it to redeem.");
            bodyText.AppendLine();
            bodyText.AppendLine("Should you have any questions or need assistance, please do not hesitate to contact us.");
            bodyText.AppendLine();
            bodyText.AppendLine("Thank you for being a sponsor. We appreciate your support!");
            bodyText.AppendLine();
            bodyText.AppendLine("Best regards,");
            bodyText.AppendLine("Fin Clarity");

            message.Subject = "Your sponsor is ready!";
            message.BodyText = bodyText.ToString();

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Connect();
                smtp.Authenticate("usagitheyellowrabbit@gmail.com", "kmnm twtb qxnw kveu");
                smtp.SendMessage(message);
                smtp.Disconnect();
            }
        }


        protected void btn_back_Click(object sender, EventArgs e)
        {
            Response.Redirect("VoucherSponsor.aspx");
        }

        static string GenerateToken()
        {
            byte[] randomBytes = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }

            // Manually convert to hex string
            StringBuilder sb = new StringBuilder(64);
            foreach (byte b in randomBytes)
            {
                sb.Append(b.ToString("X2")); // Uppercase hex (e.g. "A3")
            }

            return sb.ToString();

        }

    }
}
