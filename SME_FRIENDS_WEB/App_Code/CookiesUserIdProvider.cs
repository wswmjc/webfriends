using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for CookiesUserIdProvider
/// </summary>
public class CookiesUserIdProvider : IUserIdProvider
{
    public CookiesUserIdProvider()
    {
        //
        // TODO: Add constructor logic here
        //
    }

    public string GetUserId(IRequest request)
    {
        if (request == null)
        {
            throw new ArgumentNullException("request");
        }
        Cookie cookie;
        if (request.Cookies.TryGetValue("SNS_ID", out cookie))
        {
            return cookie.Value;
        }
        else
        {
            return string.Empty;
        }
    }
}