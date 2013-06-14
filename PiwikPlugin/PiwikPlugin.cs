// ***********************************************************************
// Assembly         : PiwikPlugin
// Author           : Tobias Wallura
// Created          : 2013-06-14
// ***********************************************************************
// <copyright file="PiwikPlugin.cs" company="Xitaso GmbH">
//     Copyright (c) Xitaso GmbH
//     Licensed under the MIT License
//     See the file LICENSE.txt for details.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace Kooboo.CMS.Piwik
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Kooboo.CMS.Sites.Extension;
    using Kooboo.CMS.Sites.Models;
    using Kooboo.CMS.Sites.View;
    
    /// <summary>
    /// Kooboo page plugin for Piwik web analytics.
    /// </summary>
    [Description("Piwik web analytics plugin for Kooboo CMS. See http://www.piwik.org/.")]
    public class PiwikPlugin : IPagePlugin
    {
        /// <summary>
        /// Configurations used by the Piwik plugin.
        /// All properties annotated with <see cref="DisplayNameAttribute"/> will be populated with values from <c>Site.Current.CustomFields</c>.
        /// </summary>
        private class Config
        {
            /// <summary>
            /// Gets or sets the Piwik site id.
            /// </summary>
            /// <value>The site id.</value>
            [DisplayName("piwik_siteid")]
            public int SiteId { get; set; }

            /// <summary>
            /// Gets or sets the hostname/port for HTTP access.
            /// <example>www.example.com:80</example>
            /// </summary>
            /// <value>The URL for HTTP access.</value>
            [DisplayName("piwik_urlhttp")]
            public string UrlHttp { get; set; }

            /// <summary>
            /// Gets or sets the hostname/port for HTTPS access.
            /// <example>www.example.com:443</example>
            /// </summary>
            /// <value>The URL for HTTPS access.</value>
            [DisplayName("piwik_urlhttps")]
            public string UrlHttps { get; set; }

            /// <summary>
            /// Gets or sets the base path for the Piwik installation.
            /// This is the folder the Piwik installation resides in, based on to the <see cref="UrlHttp"/>/<see cref="UrlHttps"/>.
            /// </summary>
            /// <example>path/to/piwik</example>
            /// <value>The base path.</value>
            [DisplayName("piwik_basepath")]
            public string BasePath { get; set; }
        }

        public ActionResult Execute(Page_Context pageViewContext, PagePositionContext positionContext)
        {
            var ctx = pageViewContext.ControllerContext.HttpContext;
            var controller = pageViewContext.ControllerContext.Controller;

            try
            {
                controller.ViewData["Piwik"] = this.GetPiwikScript(ctx, this.GetConfig());
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Gets the plugin configuration.
        /// </summary>
        /// <returns>Plugin configuration.</returns>
        private Config GetConfig()
        {
            var config = new Config();

            foreach (var i in typeof(Config).GetProperties())
            {
                var attr = i.GetCustomAttributes(typeof(DisplayNameAttribute), false).FirstOrDefault() as DisplayNameAttribute;
                if (attr != null)
                {
                    var key = attr.DisplayName;
                    if (Site.Current.CustomFields.ContainsKey(key))
                    {
                        try
                        {
                            object value = Site.Current.CustomFields[key];
                            value = Convert.ChangeType(value, i.PropertyType);
                            i.SetValue(config, value, null);
                        }
                        catch
                        {
                            System.Console.WriteLine(string.Format("[Warning] Failed to read setting: {0}", key));
                        }
                    }
                }
            }

            return config;
        }

        /// <summary>
        /// Generates the Piwik tracking script with the specified information.
        /// </summary>
        /// <param name="ctx">The Http Context used for this request.</param>
        /// <param name="config">The Piwik configuration.</param>
        /// <returns>Piwik javascript.</returns>
        private string GetPiwikScript(HttpContextBase ctx, Config config)
        {
            if (config.SiteId == 0)
            {
                return @"<!-- Piwik disabled: no site id -->";
            }

            config.UrlHttps = config.UrlHttps ?? config.UrlHttp;
            config.UrlHttp = config.UrlHttp ?? config.UrlHttps;
            if (!string.IsNullOrEmpty(config.BasePath))
            {
                config.BasePath += "/";
            }

            string imgu;
            if (ctx.Request.IsSecureConnection)
            {
                imgu = string.Format("https://{0}/", config.UrlHttps);
            }
            else
            {
                imgu = string.Format("http://{0}/", config.UrlHttp);
            }

            string u;
            if (!string.IsNullOrEmpty(config.UrlHttp) && config.UrlHttp == config.UrlHttps)
            {
                u = string.Format(@"var u=((""https:"" == document.location.protocol) ? ""https"" : ""http"") + ""://{0}/"";", config.UrlHttp);
            }
            else if (!string.IsNullOrEmpty(config.UrlHttp))
            {
                u = string.Format(@"var u=((""https:"" == document.location.protocol) ? ""https://{1}/"" : ""http://{0}/"");", config.UrlHttp, config.UrlHttps);
            }
            else
            {
                u = @"var u=((""https:"" == document.location.protocol) ? ""https"" : ""http"") + ""://"" + document.location.host + ""/"";";
                imgu = "/";
            }

            var script = string.Format(
                @"<!-- Piwik -->
<script type=""text/javascript"">
  var _paq = _paq || [];
  _paq.push([""trackPageView""]);
  _paq.push([""enableLinkTracking""]);

  (function() {{
    {1}
    _paq.push([""setTrackerUrl"", u+""{2}piwik.php""]);
    _paq.push([""setSiteId"", ""{0}""]);
    var d=document, g=d.createElement(""script""), s=d.getElementsByTagName(""script"")[0]; g.type=""text/javascript"";
    g.defer=true; g.async=true; g.src=u+""{2}piwik.js""; s.parentNode.insertBefore(g,s);
  }})();
</script>
<noscript>
<!-- Piwik Image Tracker -->
<img src=""{3}{2}piwik.php?idsite={0}&amp;rec=1"" style=""border:0"" alt="""" />
</noscript>
<!-- End Piwik Code -->", config.SiteId, u, config.BasePath, imgu);
            return script;
        }
    }
}
