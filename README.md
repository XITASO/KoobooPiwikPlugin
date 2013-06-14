Kooboo Piwik Plugin
===================

This plugin allows easy integration of [Piwik](http://www.piwik.org) with [Kooboo CMS](http://www.kooboo.com).

Installation
============

Download the **PiwikPlugin.dll** from [here](bin/) and install it as page plugin in your Kooboo system.
Ensure, that the plugin was compiled for your version of the Kooboo CMS.
If the versions do not match, your Kooboo system may not start anymore and requires you to manually delete the plugin.

You can also manually open the VisualStudio solution and compile the plugin for your Kooboo version.
To change the Kooboo version, the plugin is compiled against, replace the DLLs found in the [lib](PiwikPlugin/lib) folder.

More information about Kooboo Page Plugins can be found [here](http://wiki.kooboo.com/?wiki=Page_plugin_development).

Configuration
=============

The plugin uses several custom site settings.
See [here](http://wiki.kooboo.com/?wiki=Site_setting) for Kooboo site settings.

Setting             | Description
--------------------|------------
piwik_siteid        | The Piwik site id to track.
piwik_urlhttp       | The URL of the Piwik installation accessed with HTTP. Example: *www.example.com:80*
piwik_urlhttps      | The URL of the Piwik installation accessed with HTTPS. Example: *www.example.com:443*
piwik_basepath      | The base path of the folder where the Piwik installation resides in. Usually left blank, if Piwik is in the root path. Example: *path/to/piwik*

Using
=====

To start using Piwik, it must be included on in your page layout near the `</body>` html tag.
Add this code:

```
@Html.Raw(ViewData["Piwik"])
```

This will render the Piwik Javascript tracking code with a `<noscript>` fallback.