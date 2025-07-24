
using System;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;

namespace bipj
{
    public class ImageUploadHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";

            try
            {
                if (context.Request.Files.Count == 0)
                {
                    SendError(context, "No file uploaded");
                    return;
                }

                HttpPostedFile file = context.Request.Files[0];

                // Validate file type
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string extension = Path.GetExtension(file.FileName).ToLower();
                if (Array.IndexOf(allowedExtensions, extension) == -1)
                {
                    SendError(context, "Invalid file type. Only JPG, PNG, and GIF are allowed.");
                    return;
                }

                // Create uploads directory
                string uploadPath = context.Server.MapPath("~/uploads/");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // Generate unique filename
                string fileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadPath, fileName);
                file.SaveAs(filePath);

                // Return success response
                var response = new
                {
                    success = 1,
                    file = new
                    {
                        url = $"{context.Request.Url.Scheme}://{context.Request.Url.Authority}/uploads/{fileName}"
                    }
                };

                context.Response.Write(new JavaScriptSerializer().Serialize(response));
            }
            catch (Exception ex)
            {
                SendError(context, ex.Message);
            }
        }

        private void SendError(HttpContext context, string message)
        {
            context.Response.Write(new JavaScriptSerializer().Serialize(new
            {
                success = 0,
                message = message
            }));
        }

        public bool IsReusable => false;
    }
}