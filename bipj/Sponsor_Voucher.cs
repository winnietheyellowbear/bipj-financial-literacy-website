using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using System.Net.Mail;
using System.ComponentModel.DataAnnotations;
using bipj;

namespace bipj
{
    public class Sponsor_Voucher
    {
        string _connStr = ConfigurationManager.ConnectionStrings["FinLitDB"].ConnectionString;

        private string _Email_ID;
        private string _Email;
        private string _Subject;
        private string _Message;
        private string _Receive_DateTime;
        private string _Create_DateTime;
        private string _Status;

        public Sponsor_Voucher()
        {
        }

        // insert sponsor
        public Sponsor_Voucher(string email, string subject, string message, string receive_datetime)
        {
            _Email = email;
            _Subject = subject;
            _Message = message;
            _Receive_DateTime = receive_datetime;
        }

        // retrieve sponsor
        public Sponsor_Voucher(string email_id, string email, string subject, string message, string receive_datetime, string create_datetime, string status)
        {
            _Email_ID = email_id;
            _Email = email;
            _Subject = subject;
            _Message = message;
            _Receive_DateTime = receive_datetime;
            _Create_DateTime = create_datetime;
            _Status = status;
        }

        public string Email_ID
        {
            get { return _Email_ID; }
            set { _Email_ID = value; }
        }

        public string Email
        {
            get { return _Email; }
            set { _Email = value; }
        }

        public string Subject
        {
            get { return _Subject; }
            set { _Subject = value; }
        }

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }

        public string Receive_DateTime
        {
            get { return _Receive_DateTime; }
            set { _Receive_DateTime = value; }
        }

        public string Create_DateTime
        {
            get { return _Create_DateTime; }
            set { _Create_DateTime = value; }
        }

        public string Status
        {
            get { return _Status; }
            set { _Status = value; }
        }

        public void RetrieveSponsorVoucherEmails()
        {
            using (var client = new ImapClient())
            {
                // Connect to mail server (replace with your actual server and credentials)
                client.Connect("imap.gmail.com", 993, true); // SSL enabled
                client.Authenticate("usagitheyellowrabbit@gmail.com", "ohxf cbou kwjk dmou"); // Not your main Gmail password!

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                // Search for emails with relevant subject/body
                var query = SearchQuery.NotSeen.And(
                            SearchQuery.SubjectContains("voucher")
                            .Or(SearchQuery.SubjectContains("sponsorship"))
                            .Or(SearchQuery.SubjectContains("sponsor"))
                            .Or(SearchQuery.BodyContains("voucher"))
                            .Or(SearchQuery.BodyContains("sponsor"))
                            .Or(SearchQuery.BodyContains("sponsorship"))
                        );

                var results = inbox.Search(query);

                foreach (var id in results)
                {
                    var message = inbox.GetMessage(id);

                    Sponsor_Voucher sponsor_voucher = new Sponsor_Voucher
                    (
                        message.From.Mailboxes.FirstOrDefault()?.Address ?? "unknown",
                        message.Subject,
                        message.TextBody ?? message.HtmlBody ?? "",
                        message.Date.ToString("yyyy-MM-dd HH:mm:ss")
                    );

                    sponsor_voucher.SponsorInsert();
                    inbox.AddFlags(id, MessageFlags.Seen, true);
                }

                client.Disconnect(true);
            }

            
        }

        public int SponsorInsert()
        {
            int result = 0;

            string queryStr = "INSERT INTO Voucher_Sponsor(Email, Subject, Message, Receive_DateTime, Status)"
                            + "VALUES (@Email, @Subject, @Message, @Receive_DateTime, @Status)";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);

            cmd.Parameters.AddWithValue("@Email", this.Email);
            cmd.Parameters.AddWithValue("@Subject", this.Subject);
            cmd.Parameters.AddWithValue("@Message", this.Message);
            cmd.Parameters.AddWithValue("@Receive_DateTime", this.Receive_DateTime);
            cmd.Parameters.AddWithValue("@Status", "Pending");

            conn.Open();
            result += cmd.ExecuteNonQuery();
            conn.Close();

            return result;
        }

        public List<Sponsor_Voucher> GetAllSponsors()
        {
            string email_id, email, subject, message, receive_datetime, create_datetime, status;
            List<Sponsor_Voucher> sponsor_list = new List<Sponsor_Voucher>();

            string queryStr = "SELECT * FROM Voucher_Sponsor";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
           
            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                email_id = dr["Email_ID"].ToString();
                email = dr["Email"].ToString();
                subject = dr["Subject"].ToString();
                message = dr["Message"].ToString();
                receive_datetime = dr["Receive_DateTime"].ToString();
                create_datetime = dr["Create_DateTime"].ToString();
                status = dr["Status"].ToString();

                Sponsor_Voucher sponsor_voucher = new Sponsor_Voucher(email_id, email, subject, message, receive_datetime, create_datetime, status);
                sponsor_list.Add(sponsor_voucher);
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return sponsor_list;
        }

        public Sponsor_Voucher GetEmailByEmailID(string email_id)
        {
            string email, subject, message, receive_datetime, create_datetime, status;
            Sponsor_Voucher sponsor_voucher = new Sponsor_Voucher();    
            
            string queryStr = "SELECT * FROM Voucher_Sponsor WHERE Email_ID = @Email_ID";

            SqlConnection conn = new SqlConnection(_connStr);
            SqlCommand cmd = new SqlCommand(queryStr, conn);
            cmd.Parameters.AddWithValue("@Email_ID", email_id);

            conn.Open();
            SqlDataReader dr = cmd.ExecuteReader();

            while (dr.Read())
            {
                email = dr["Email"].ToString();
                subject = dr["Subject"].ToString();
                message = dr["Message"].ToString();
                receive_datetime = dr["Receive_DateTime"].ToString();
                create_datetime = dr["Create_DateTime"].ToString();
                status = dr["Status"].ToString();

                sponsor_voucher = new Sponsor_Voucher(email_id, email, subject, message, receive_datetime, create_datetime, status);
                
            }

            conn.Close();
            dr.Close();
            dr.Dispose();

            return sponsor_voucher;
        }
    }
}
