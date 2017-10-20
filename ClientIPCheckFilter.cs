using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace ViennaFeedback
{
    public class ClientIPCheckilter : ActionFilterAttribute 
    {
        private readonly ILogger _logger;
 
        public ClientIPCheckilter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("IPLogger");
        }
 
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            string ipAddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
            _logger.LogInformation($"Remote IpAddress: {context.HttpContext.Connection.RemoteIpAddress}");
            
            // when the site is hosted in Docker, the real client IP comes through the header X-Client-IP
            if (!String.IsNullOrEmpty(context.HttpContext.Request.Headers["X-Client-IP"]))
                ipAddress = context.HttpContext.Request.Headers["X-Client-IP"];
            
            if (!IsValidIP(ipAddress)) {
                context.Result = new RedirectResult("https://aka.ms/azureml-corpnetonly");
            } 

            base.OnActionExecuting(context);
        }

        private bool IsValidIP(string ip)
        {
            string[] ranges = new string[] {

                "167.220.0.0/16", //Boston
                "131.107.0.0/16", // MS Corp-8 (Redmond)
                "157.54.0.0/15", // Other MSCorp. ** this list came from Steve Morin.
                "157.56.0.0/15",
                "157.58.0.0/15",
                "157.60.0.0/16",
                //"0.0.0.0/8", // localhost
                //"::1", // localhosts
                "73.4.97.83" // comcast
                //"127.0.0.1" // localhost
            };
            var userIP = IPNetwork.Parse(ip);
            foreach (string r in ranges)
            {
                var range = IPNetwork.Parse(r);
                if (IPNetwork.Contains(range, userIP))
                    return true;
            }
            return false;
        }
    }
}