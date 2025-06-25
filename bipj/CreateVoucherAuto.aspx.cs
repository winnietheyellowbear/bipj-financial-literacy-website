using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

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
                    string email_id = Session["Email_ID"].ToString();

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

            Staff_Voucher staff_voucher = new Staff_Voucher(name, description, validity, points_required);
            result = staff_voucher.VoucherInsert();

            if (result > 0)
            {
                Email();

                string email_id = Session["Email_ID"].ToString();
                sponsor_voucher.StatusUpdate(email_id);

                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Voucher created. 😊'); window.location='VoucherSponsor.aspx';", true);
            }
            else
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Failed to create voucher. 😞');", true);
            }
        }

        protected void Email()
        {
            string email_id = Session["Email_ID"].ToString();

            Sponsor_Voucher sponsor_voucher = new Sponsor_Voucher();
            sponsor_voucher = sponsor_voucher.GetEmailByEmailID(email_id);

            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            var message = new MailMessage(
               new MailAddress("usagitheyellowrabbit@gmail.com", "Sender"),
               new MailAddress(sponsor_voucher.Email, "First receiver"));

            message.Subject = "Voucher created!";
            message.BodyText = "Hi, your sponsor voucher has been successfully created. Please click the link to review the details before enabling it, or if you wish to disable the voucher.";

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Connect();
                smtp.Authenticate("usagitheyellowrabbit@gmail.com", "kmnm twtb qxnw kveu");
                smtp.SendMessage(message);
                smtp.Disconnect();
            }
        }
    }
}
