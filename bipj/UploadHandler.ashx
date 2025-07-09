<%@ WebHandler Language="C#" Class="ImageUploadHandler" %>

using System;
using System.Web;
using System.IO;

public class ImageUploadHandler : IHttpHandler
{
    public void ProcessRequest(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        
        try
        {
            if (context.Request.Files.Count > 0)
            {
                HttpPostedFile file = context.Request.Files[0];
                
                // Validate file type
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                string fileExtension = Path.GetExtension(file.FileName).ToLower();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new Exception("Only image files are allowed (JPG, PNG, GIF)");
                }
                
                // Create uploads directory if it doesn't exist
                string uploadPath = context.Server.MapPath("~/uploads/");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                
                // Generate unique filename
                string fileName = $"{Guid.NewGuid()}{fileExtension}";
                string fullPath = Path.Combine(uploadPath, fileName);
                
                // Save file
                file.SaveAs(fullPath);
                
                // Return success response
                context.Response.Write($"{{\"success\":1,\"file\":{{\"url\":\"/uploads/{fileName}\"}}}}");
            }
        }
        catch (Exception ex)
        {
            context.Response.Write($"{{\"success\":0,\"message\":\"{ex.Message}\"}}");
        }
    }

    public bool IsReusable => false;
}