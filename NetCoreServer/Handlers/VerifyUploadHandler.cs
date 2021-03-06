﻿using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

namespace NetCoreServer
{
    public class VerifyUploadHandler
    {
        public static void Invoke(HttpContext context)
        {
            // Report back original request method verb.
            context.Response.Headers.Add("X-HttpRequest-Method", context.Request.Method);

            // Report back original entity-body related request headers.
            string contentLength = context.Request.Headers["Content-Length"];
            if (!string.IsNullOrEmpty(contentLength))
            {
                context.Response.Headers.Add("X-HttpRequest-Headers-ContentLength", contentLength);
            }

            string transferEncoding = context.Request.Headers["Transfer-Encoding"];
            if (!string.IsNullOrEmpty(transferEncoding))
            {
                context.Response.Headers.Add("X-HttpRequest-Headers-TransferEncoding", transferEncoding);
            }

            // Get expected MD5 hash of request body.
            string expectedHash = context.Request.Headers["Content-MD5"];
            if (string.IsNullOrEmpty(expectedHash))
            {
                context.Response.StatusCode = 500;
                context.Response.SetStatusDescription("Missing 'Content-MD5' request header");
                return;
            }

            // Compute MD5 hash of received request body.
            string actualHash;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(ReadAllRequestBytes(context));
                actualHash = Convert.ToBase64String(hash);
            }

            if (expectedHash == actualHash)
            {
                context.Response.StatusCode = 200;
            }
            else
            {
                context.Response.StatusCode = 500;
                context.Response.SetStatusDescription("Request body not verfied");
            }
        }

        private static byte[] ReadAllRequestBytes(HttpContext context)
        {
            Stream requestStream = context.Request.Body;
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = requestStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
