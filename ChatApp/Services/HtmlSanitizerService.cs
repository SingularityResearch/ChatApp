using System.Text.RegularExpressions;

namespace ChatApp.Services
{
    public class HtmlSanitizerService
    {
        public string Sanitize(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return html;
            }

            // 1. Remove <script> tags and their contents
            html = Regex.Replace(html, @"<script[^>]*>([\s\S]*?)</script>", "", RegexOptions.IgnoreCase);

            // 2. Remove on* event attributes (e.g., onclick, onload, onerror)
            // Example: <img src="x" onerror="alert(1)"> -> <img src="x" >
            html = Regex.Replace(html, @"(?i)\s(on[a-z]+)\s*=\s*(?:""[^""]*""|'[^']*'|[^'"">\s]+)", "", RegexOptions.IgnoreCase);

            // 3. Remove javascript: protocols from href and src attributes
            // Example: <a href="javascript:alert(1)"> -> <a href="">
            html = Regex.Replace(html, @"(?i)(href|src)\s*=\s*([""']?)javascript:[^""'>]*\2", "$1=$2$2", RegexOptions.IgnoreCase);

            return html;
        }
    }
}
