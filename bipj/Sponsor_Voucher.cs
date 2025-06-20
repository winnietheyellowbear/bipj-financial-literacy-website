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
        private string _Response;
        private string _Response_DateTime;
        private string _Status;

        public Sponsor_Voucher()
        {
        }
        public Sponsor_Voucher(string email, string subject, string message, string receive_datetime)
        {
            _Email = email;
            _Subject = subject;
            _Message = message;
            _Receive_DateTime = receive_datetime;
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

        public string Response
        {
            get { return _Response; }
            set { _Response = value; }
        }

        public string Response_DateTime
        {
            get { return _Response_DateTime; }
            set { _Response_DateTime = value; }
        }

        public string Status
        {
            get { return Status; }
            set { Status = value; }
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
            cmd.Parameters.AddWithValue("@Receive_DateTime", this._Receive_DateTime);
            cmd.Parameters.AddWithValue("@Status", "Pending");

            conn.Open();
            result += cmd.ExecuteNonQuery();
            conn.Close();

            return result;
        }

    }
}