using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace Rs3Tracker.Classes {

    /// <summary>One row scraped out of a wiki table.</summary>
    public class WikiRow {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Link { get; set; }
        public double Cooldown { get; set; }
    }

    /// <summary>One logical column of a wiki table, already expanded over its colspan.</summary>
    public class WikiColumn {
        public string Header { get; set; }
        public int Start { get; set; }
        public int Span { get; set; }
        public int Last { get { return Start + Span - 1; } }
        public int End { get { return Start + Span; } }
    }

    /// <summary>
    /// Structure based scraping of runescape.wiki tables.
    /// The wiki rewrites the class attribute of its tables every few months, so nothing in here
    /// matches on class names: tables are found by the text of their header row and the columns
    /// inside them are found by their header text instead of by a hardcoded child index.
    /// </summary>
    public static class WikiTables {

        private static readonly Regex Whitespace = new Regex(@"\s+", RegexOptions.Compiled);
        private static readonly Regex FootNotes = new Regex(@"\[[^\]]*\]", RegexOptions.Compiled);
        private static readonly Regex Duration = new Regex(
            @"(\d+(?:\.\d+)?)\s*(ticks?|seconds?|secs?|s|minutes?|mins?|m|hours?|hrs?|h)?",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string CleanText(string text) {
            if (string.IsNullOrEmpty(text))
                return "";
            string clean = HtmlEntity.DeEntitize(text) ?? text;
            clean = clean.Replace('\u00a0', ' ');
            return Whitespace.Replace(clean, " ").Trim();
        }

        /// <summary>Header text without the footnote markers the wiki likes to append, eg "Damage[?]".</summary>
        public static string CleanHeader(string text) {
            return FootNotes.Replace(CleanText(text), "").Trim();
        }

        /// <summary>Every wikitable on the page whose first column is titled <paramref name="firstHeader"/>.</summary>
        public static List<HtmlNode> FindTables(HtmlDocument doc, string firstHeader) {
            var found = new List<HtmlNode>();
            if (doc == null)
                return found;
            var tables = doc.DocumentNode.SelectNodes(
                "//table[contains(concat(' ', normalize-space(@class), ' '), ' wikitable ')]");
            if (tables == null)
                return found;
            foreach (var table in tables) {
                var columns = GetColumns(table);
                if (columns.Count > 0 && columns[0].Header.Equals(firstHeader, StringComparison.OrdinalIgnoreCase))
                    found.Add(table);
            }
            return found;
        }

        /// <summary>The columns of a table, read off its first row and expanded over colspan.</summary>
        public static List<WikiColumn> GetColumns(HtmlNode table) {
            var columns = new List<WikiColumn>();
            var rows = Rows(table);
            if (rows.Count == 0)
                return columns;
            var cells = rows[0].SelectNodes("./th|./td");
            if (cells == null)
                return columns;
            int index = 0;
            foreach (var cell in cells) {
                int span;
                if (!int.TryParse(cell.GetAttributeValue("colspan", "1"), out span) || span < 1)
                    span = 1;
                columns.Add(new WikiColumn { Header = CleanHeader(cell.InnerText), Start = index, Span = span });
                index += span;
            }
            return columns;
        }

        /// <summary>The column titled <paramref name="header"/>, or null. Falls back to a partial match.</summary>
        public static WikiColumn Column(List<WikiColumn> columns, string header) {
            if (columns == null || string.IsNullOrEmpty(header))
                return null;
            foreach (var column in columns)
                if (column.Header.Equals(header, StringComparison.OrdinalIgnoreCase))
                    return column;
            foreach (var column in columns)
                if (column.Header.IndexOf(header, StringComparison.OrdinalIgnoreCase) >= 0)
                    return column;
            return null;
        }

        /// <summary>Rows belonging to this table itself, never to a table nested inside one of its cells.</summary>
        public static List<HtmlNode> Rows(HtmlNode table) {
            var rows = table.SelectNodes("./tr|./thead/tr|./tbody/tr|./tfoot/tr");
            return rows == null ? new List<HtmlNode>() : rows.ToList();
        }

        /// <summary>
        /// The name / icon / cooldown of every data row of a table. Rows that do not have the full
        /// set of columns (sub headings, note rows) are skipped.
        /// </summary>
        public static List<WikiRow> ParseRows(HtmlNode table, string nameHeader, string cooldownHeader) {
            var parsed = new List<WikiRow>();
            var columns = GetColumns(table);
            var nameColumn = Column(columns, nameHeader);
            if (nameColumn == null)
                return parsed;
            var cooldownColumn = Column(columns, cooldownHeader);
            int width = columns[columns.Count - 1].End;

            foreach (var row in Rows(table)) {
                if (row.SelectSingleNode("./td") == null)
                    continue;                                   // header row
                var cells = row.SelectNodes("./td|./th");
                if (cells == null || cells.Count < width)
                    continue;                                   // spanned note row
                var scraped = new WikiRow();
                scraped.Name = CleanText(cells[nameColumn.Last].InnerText);
                if (string.IsNullOrEmpty(scraped.Name))
                    continue;
                scraped.ImageUrl = ImageUrl(cells, nameColumn);
                scraped.Link = PageLink(cells[nameColumn.Last]);
                if (cooldownColumn != null)
                    scraped.Cooldown = ParseSeconds(cells[cooldownColumn.Last].InnerText);
                parsed.Add(scraped);
            }
            return parsed;
        }

        /// <summary>The title of the section the table sits in, eg "Melee" for the melee ability table.</summary>
        public static string SectionOf(HtmlNode table) {
            var heading = table.SelectSingleNode("preceding::*[self::h2 or self::h3 or self::h4][1]");
            if (heading == null)
                return "";
            string id = heading.GetAttributeValue("id", "");
            if (!string.IsNullOrEmpty(id))
                return CleanText(id.Replace('_', ' '));
            string text = CleanText(heading.InnerText);
            int edit = text.IndexOf("[edit", StringComparison.OrdinalIgnoreCase);
            return edit >= 0 ? text.Substring(0, edit).Trim() : text;
        }

        /// <summary>Seconds out of the wiki's duration wording, eg "20.4 seconds (34 ticks)" or "6 ticks".</summary>
        public static double ParseSeconds(string text) {
            string clean = CleanText(text);
            if (clean.Length == 0)
                return 0;
            var match = Duration.Match(clean);
            if (!match.Success)
                return 0;
            double value;
            if (!double.TryParse(match.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return 0;
            string unit = match.Groups[2].Value.ToLowerInvariant();
            if (unit.StartsWith("tick"))
                return Math.Round(value * 0.6, 3);
            if (unit.StartsWith("h"))
                return value * 3600;
            if (unit.StartsWith("m"))
                return value * 60;
            return value;
        }

        /// <summary>The wiki article a cell links to, eg "/w/Invoke_Death".</summary>
        public static string PageLink(HtmlNode cell) {
            var link = cell.SelectSingleNode(".//a[@href]");
            if (link == null)
                return "";
            string href = HtmlEntity.DeEntitize(link.GetAttributeValue("href", "")) ?? "";
            return href.StartsWith("/w/", StringComparison.OrdinalIgnoreCase) ? href : "";
        }

        /// <summary>The icon of a row, taken from anywhere inside the name column.</summary>
        private static string ImageUrl(HtmlNodeCollection cells, WikiColumn column) {
            for (int i = column.Start; i < column.End && i < cells.Count; i++) {
                string url = ImageUrlFrom(cells[i]);
                if (!string.IsNullOrEmpty(url))
                    return url;
            }
            return "";
        }

        /// <summary>The full size image behind the first icon in a cell, or "" when there is none.</summary>
        public static string ImageUrlFrom(HtmlNode cell) {
            if (cell == null)
                return "";
            var img = cell.SelectSingleNode(".//img");
            if (img == null)
                return "";
            string url = FullSizeImage(BestSource(img));
            if (url.IndexOf("/images/", StringComparison.OrdinalIgnoreCase) < 0)
                return "";
            if (!url.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                && !url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase)
                && !url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                return "";
            return url;
        }

        /// <summary>The largest candidate of an img: the biggest srcset entry, else its src.</summary>
        private static string BestSource(HtmlNode img) {
            string best = "";
            double bestScale = 0;
            foreach (string candidate in img.GetAttributeValue("srcset", "").Split(',')) {
                var parts = candidate.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;
                double scale = 1;
                if (parts.Length > 1)
                    double.TryParse(parts[1].TrimEnd('x', 'X'), NumberStyles.Float, CultureInfo.InvariantCulture, out scale);
                if (scale >= bestScale) {
                    bestScale = scale;
                    best = parts[0];
                }
            }
            if (string.IsNullOrEmpty(best))
                best = img.GetAttributeValue("src", "");
            return HtmlEntity.DeEntitize(best) ?? best;
        }

        /// <summary>Turns a thumbnail url into the original upload: /images/thumb/X.png/30px-X.png -> /images/X.png.</summary>
        public static string FullSizeImage(string url) {
            if (string.IsNullOrEmpty(url))
                return "";
            int query = url.IndexOf('?');
            if (query >= 0)
                url = url.Substring(0, query);
            const string thumbs = "/images/thumb/";
            int at = url.IndexOf(thumbs, StringComparison.OrdinalIgnoreCase);
            if (at >= 0) {
                string rest = url.Substring(at + thumbs.Length);
                int slash = rest.IndexOf('/');
                if (slash > 0)
                    url = url.Substring(0, at) + "/images/" + rest.Substring(0, slash);
            }
            return url;
        }
    }
}
