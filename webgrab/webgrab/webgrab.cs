using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.IO;
using System.Windows;

namespace webgrab
{
    class webgrab
    {
        /**
        *** OPTIONS
        **/
        //download
        private static bool VALID       = false; 
        private static bool REPLACE     = false; 
        private static bool SKIP        = false; private static List<string> skip_list = new List<string>();
        private static bool SKIP_FIND   = false;
        private static bool SKIP_FILE   = false;
        private static bool SKIP_EXT    = false;
        private static bool SKIP_CASE   = false;
        private static bool SKIP_OUTPUT = false;
        private static bool NODUPES     = false;
        //display
        private static bool COMPACT  = false; private static Thread print_info_th_compact = new Thread(print_info_thread_compact) { IsBackground = true };
        private static bool CLEAN    = false;
        private static bool COUNT    = false; 
        private static bool VERBOSE  = false; private static Thread print_info_th = new Thread(print_info_thread) { IsBackground = true };
        private static bool FILENAME = false;
        private static bool COLOR    = false;

        //COUNT
        private static int count_processed = 0;
        private static int count_total     = 0;
        private static int count_accepted  = 0;
        private static int count_dled      = 0; private static double count_dled_bytes = 0;
        private static int count_dupes     = 0;
        private static int count_replaced  = 0;
        private static int count_skipped   = 0;
        private static int count_disposed  = 0; //wrong file type
        private static int count_failed    = 0;

        //ONLY / !media / +video / +image / ...
        private static List<string> only           = new List<string>();
        private static bool         only_bool      = false;
        private static List<string> only_must      = new List<string>();
        private static bool         only_must_bool = false;

        //IGNORE
        private static bool         ignore_bool = false;
        private static List<string> ignore = new List<string>();

        private static WebClient wc = new WebClient();
        //add this to fix 403 error: wc.Headers.Add("user-agent", "App Name"); // +agent

        //FILE TYPES FOR 'media' OPTION
        //CONST FILE TYPES
        private static readonly List<string> TYPES_VIDEO = new List<string> { ".mp4", ".webm", ".avi", ".mov", ".mkv", ".flv", ".mpeg", ".mpg", ".wmv", ".mp3", ".ogg" };
        private static readonly List<string> TYPES_IMAGE = new List<string> { ".jpg", ".jpeg", ".jpe", ".jiff", ".jfif", ".png", ".gif", ".ico", ".svg", ".bmp", ".dib", ".tif", ".tiff" };
        private static readonly List<string> TYPES_OTHER = new List<string> { ".zip", ".rar", ".exe", ".swf", ".dll", ".txt" };
        private static readonly List<string> TYPES_CODEC = new List<string>
        {
            ".asp", ".aspx", ".axd", ".asx", ".asmx", ".ashx", ".css", ".cfm", ".yaws", ".swf", ".html",
            ".htm", ".xhtml", ".jhtml", ".jsp", ".jspx", ".wss", ".do", ".action", ".js", ".pl", ".php",
            ".php4", ".php3", ".phtml", ".py", ".rb", ".rhtml", ".shtml", ".xml", ".rss", ".svg", ".cgi", ".dll"
        };
        
        [STAThread]
        static int Main(string[] args)
        {
            //SETUP
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // Use SecurityProtocolType.Ssl3 if needed for compatibility reasons
            }
            catch (System.NotSupportedException e) { print(print_type.err, "ERR[NotSupportedException] " + e.Message); return e.HResult; }
            catch (System.Exception e)             { print(print_type.err, "ERR[UNK] "                   + e.Message); return e.HResult; }

            //if application has just ran (no args)
            if (args.Length == 0) { print_help(); return 1; }

            //RO
            switch (args[0])
            {
                case "/?":
                case "-h":
                case "--h":
                case "help":
                case "-help":
                case "--help": { print_help(args); return 0; /*break*/  }

                case "echo":
                case "-echo":
                case "--echo":
                case "print":
                case "-print":
                case "--print":
                case "output":
                case "-output":
                case "--output":
                case "out":
                case "-out":
                case "--out": { return RunOption_outUrls(args); /*break*/  }

                case "watch":
                case "-watch":
                case "--watch":
                case "listen":
                case "-listen":
                case "--listen": { return RunOption_listen(args); /*break*/  }

                case "ping":
                case "-ping":
                case "--ping":
                case "try":
                case "-try":
                case "--try":
                case "test":
                case "-test":
                case "--test": { return RunOption_testWebsite(args); /*break*/ }
                    
                case "read":
                case "-read":
                case "--read":
                case "txt":
                case "-txt":
                case "--txt":
                case "text":
                case "-text":
                case "--text": { return RunOption_read(args); /*break*/ }
            }

            //IF THERE ARE 2 ARGS GET OPTIONS
            if (args.Length == 2) { get_options(args[1]); }

            string input = args[0];
            byte[] html = null;

            //IF URL==FILE just download and quit
            foreach (string type in TYPES_VIDEO) { if (input.Contains(type)) { return wc_dl(args[0]); } }
            foreach (string type in TYPES_IMAGE) { if (input.Contains(type)) { return wc_dl(args[0]); } }
            foreach (string type in TYPES_OTHER) { if (input.Contains(type)) { return wc_dl(args[0]); } }

            int check = check_URLorFILE(input);
            switch (check)
            {
                case 0:
                default: return 1;

                case 1: {
                    /**
                    *** DOWNLOAD HTML
                    **/
                    print(print_type.def, "");
                    print(print_type.top, "===[ DOWNLOADING HTML ]");
                    print(print_type.cur, "DING..: " + input + " ", false); //ONE LINE

                    try { html = wc.DownloadData(new Uri(input)); }
                    catch (System.Net.WebException e)       { print(print_type.cer, "ERR[WebException] "           + e.Message, true, true); return returnStatusCode(e); }
                    catch (System.ArgumentNullException e)  { print(print_type.cer, "ERR[ArgumentNullException] "  + e.Message, true, true); return e.HResult;           }
                    catch (System.NullReferenceException e) { print(print_type.cer, "ERR[NullReferenceException] " + e.Message, true, true); return e.HResult;           }
                    catch (System.Exception e)              { print(print_type.cer, "ERR[UNK] "                    + e.Message, true, true); return e.HResult;           }
                    print(print_type.def, ""); //ENDLINE (or with an error)

                    print(print_type.pri, "DLED..: " + ROund(html.Length) + " (" + html.Length + " bytes)");
                    break;
                }
                case 2: html = File.ReadAllBytes(input); break;
            }
            
            if (VERBOSE && !COMPACT) { print_info_th.Start(); }
            if (COMPACT) { print_info_th_compact.Start(); }

            /**
            *** HTML SCAN (CLEAN UP)
            **/
            print(print_type.def, "");
            print(print_type.top, "===[ SCANNING HTML ]");
            print(print_type.inf, "title from html.: " + get_html_title(html)); //GET TITLE
            List<string> html_all = get_html_all(check == 1 ? input : null, html); //HTML SCAN
            print(print_type.inf, "total items ....: " + count_total);
            List<string> accepted_items = new List<string>(); //CLEAN/REMOVE ITEMS BY RULES
            foreach (string str in html_all) { if (url_is_valid(str)) { accepted_items.Add(str); } }
            print(print_type.inf, "accepted items..: " + count_accepted);
            
            /**
            *** DOWNLOAD FOUND URLS
            **/
            print(print_type.def, "");
            print(print_type.top, "===[ DOWNLOADING ]");
            foreach (string l in accepted_items) { wc_dl(l); }

            try
            {
                print_info_th.Abort();
                print_info_th_compact.Abort();
            }
            catch (ThreadStateException)              { print(print_type.err, "ERR[ThreadStateException]"); }
            catch (System.Security.SecurityException) { print(print_type.err, "ERR[SecurityException]"); }
            catch (Exception)                         { print(print_type.err, "ERR[UNK]"); }

            //END
            print_info(COMPACT);
            print(print_type.def, "", true, COMPACT); //endline
            
            return 0;
        }
        
        private enum print_type
        {
            def, //default
            inf, //information
            top, //title, header
            unk, //unknown

            cur, //current, count, number, ding
            pri, //priority, downloaded, dled

            war, //warning
            eri, //error info (standard output) (ERROR COUNTS)
            err, //error      (error output)
            cer  //critical error
        }
        private static void print(print_type type, string text, bool new_line = true, bool iamCompact = false)
        {
            //compact logic
            if (COMPACT == true && iamCompact == false && type != print_type.cer) { return; }
            
            ConsoleColor default_color = Console.ForegroundColor; //set past color
            ConsoleColor fcolor = Console.ForegroundColor;

            /* COLOR START */
            if (COLOR)
            {
                switch (type)
                {
                    case print_type.def: default: fcolor = ConsoleColor.Gray; break;
                    case print_type.inf: fcolor = ConsoleColor.DarkYellow;    break;
                    case print_type.top: fcolor = ConsoleColor.DarkCyan;      break;
                    case print_type.unk: fcolor = ConsoleColor.Cyan;          break;
                    case print_type.cur: fcolor = ConsoleColor.DarkGreen;     break;
                    case print_type.pri: fcolor = ConsoleColor.Green;         break;
                    case print_type.war: fcolor = ConsoleColor.Yellow;        break;
                    case print_type.eri: fcolor = ConsoleColor.Red;           break;
                    case print_type.err: fcolor = ConsoleColor.Red;           break;
                    case print_type.cer: fcolor = ConsoleColor.Red;           break;
                }

                Console.ForegroundColor = fcolor;
            }
            
            if (type == print_type.err || type == print_type.cer)
            { if (new_line) { Console.Error.WriteLine(text); } else { Console.Error.Write(text); } }
            else
            { if (new_line) { Console.WriteLine(text);       } else { Console.Write(text);       } }

            /* COLOR END */
            if (COLOR) { Console.ForegroundColor = default_color; }
        }
        private static void print_help(string[] args = null)
        {
            if (args != null && args.Length == 2) { get_options(args[1]); }

            print(print_type.def, "");
            print(print_type.top, " +===[ ABOUT ]");
            print(print_type.top, " |", false); print(print_type.inf, " ABOUT....: web page file scanner / downloader");
            print(print_type.top, " |", false); print(print_type.inf, " BUILT IN.: C# .NET 4.6.1");
            print(print_type.top, " |", false); print(print_type.inf, " Version..: 75");
            print(print_type.top, " |", false); print(print_type.inf, " Author...: 0xC0LD");
            print(print_type.top, " |", false); print(print_type.inf, " USAGE....: webgrab.exe <webpage url / file.html / RO> <switch1,sw2,sw3,sw4,...>");
            print(print_type.def, "");
            print(print_type.top, " +===[ RUN OPTIONS (RO) ]");
            print(print_type.top, " |", false); print(print_type.inf, " --help \"switch1,sw2,sw3,sw4,...\"                           = shows this (help)");
            print(print_type.top, " |", false); print(print_type.inf, " --out \"webpage url / file.html\" \"switch1,sw2,sw3,sw4,...\"  = no downloading, only output URL(s)");
            print(print_type.top, " |", false); print(print_type.inf, " --test \"webpage url / file.html\" \"switch1,sw2,sw3,sw4,...\" = no downloading, just print counts");
            print(print_type.top, " |", false); print(print_type.inf, " --read \"file_with_urls.txt\" \"switch1,sw2,sw3,sw4,...\"      = read a .txt file and treat every line as an URL, then download them");
            print(print_type.top, " |", false); print(print_type.inf, " --watch \"switch1,sw2,sw3,sw4,...\"                          = download copied (clipboard) URL(s)");
            print(print_type.def, "");
            print(print_type.top, " +===[ RULES / SWITCHES ]");
            print(print_type.top, " |", false); print(print_type.top, " +==[DOWNLOAD RULES]");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @valid         = only download valid URI addresses");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " -<type/name>   = ignore (ex. -thumb.jpg)");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @replace       = if file exists, replace the file...");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @skip          = if file exists, skip the url...");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, "   @skip_find   = search for the file in subdirectories and if it exists skip it");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, "   @skip_file   = make your own *.webgrab_skip file(s) that contain filenames (each on every line) to skip...");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, "   @skip_ext    = ignore extensions when comparing filenames");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, "   @skip_case   = ignore case when comparing filenames");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, "   @skip_output = test existence of file before output (useful if you use --out/--test/...)");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, "   @skip_all    = @skip,@skip_find,@skip_file,@skip_ext,@skip_case,@skip_output");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @nodupes       = dispose duplicate URL(s)...");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @agent         = add User-Agent to request header (403 error fix)");
            print(print_type.top, " |");
            print(print_type.top, " |", false); print(print_type.top, " +=[DISPLAY RULES]");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @compact  = only print numbers (on a single line)");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @clean    = don't print errors, (only successful downloads (DLED))");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @count    = count items");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @verbose  = print current progress on key press... with --out print debug... (+@count)");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @filename = for --out, only print filename(s)");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @color    = use colors in output");
            print(print_type.top, " |");
            print(print_type.top, " |", false); print(print_type.top, " +==[FILE RULES]");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @media = @video,@image,@other");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @grab  = @clean,@color,@count,@valid,@media,-thumb ");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @video = ", false); foreach (string type in TYPES_VIDEO) { if (type == TYPES_VIDEO[TYPES_VIDEO.Count - 1]) { print(print_type.inf, "~" + type, false); } else { print(print_type.inf, "~" + type + ",", false); } }
            print(print_type.def, "");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @image = ", false); foreach (string type in TYPES_IMAGE) { if (type == TYPES_IMAGE[TYPES_IMAGE.Count - 1]) { print(print_type.inf, "~" + type, false); } else { print(print_type.inf, "~" + type + ",", false); } }
            print(print_type.def, "");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @other = ", false); foreach (string type in TYPES_OTHER) { if (type == TYPES_OTHER[TYPES_OTHER.Count - 1]) { print(print_type.inf, "~" + type, false); } else { print(print_type.inf, "~" + type + ",", false); } }
            print(print_type.def, "");
            print(print_type.top, " |", false); print(print_type.top, " |", false); print(print_type.inf, " @codec = ", false); foreach (string type in TYPES_CODEC) { if (type == TYPES_CODEC[TYPES_CODEC.Count - 1]) { print(print_type.inf, "~" + type, false); } else { print(print_type.inf, "~" + type + ",", false); } }
            print(print_type.def, "");
            print(print_type.def, "");
            print(print_type.top, " +===[ MORE INFO / EXAMPLES ]");
            print(print_type.top, " |", false); print(print_type.inf, " - if skip and replace are specified, file download will be skipped...");
            print(print_type.top, " |", true);
            print(print_type.top, " |", false); print(print_type.inf, " > webgrab <url> @rule,keyword,-keyword,~keyword");
            print(print_type.top, " |", false); print(print_type.inf, "      @rule     = download rule / variable");
            print(print_type.top, " |", false); print(print_type.inf, "      keyword   = keyword that must be present");
            print(print_type.top, " |", false); print(print_type.inf, "      -keyword  = keyword that must NOT be present");
            print(print_type.top, " |", false); print(print_type.inf, "      ~keyword  = keyword that should / could be present");
            print(print_type.top, " |", true);
            print(print_type.top, " |", false); print(print_type.inf, " DOWNLOAD YOUTUBE THUMBNAIL: ");
            print(print_type.top, " |", false); print(print_type.inf, "  > webgrab.exe \"https://www.youtube.com/watch?v=XXXXXXXXXXX\" @valid,@image,@nodupes,maxres");
            print(print_type.top, " |", true); 
            print(print_type.top, " |", false); print(print_type.inf, " [...]");
            print(print_type.def, "");
        }

        private static void print_info(bool compact = false)
        {
            if (compact == true)
            {
                if (count_total     != 0) { print(print_type.inf, "\rtotal: "  + count_total,     false, true); print(print_type.def, " ", false, true); }
                if (count_accepted  != 0) { print(print_type.inf,  "acptd: "   + count_accepted,  false, true); print(print_type.def, " ", false, true); }
                if (count_processed != 0) { print(print_type.cur,   "count: "  + count_processed, false, true); print(print_type.def, " ", false, true); }
                if (count_dled      != 0) { print(print_type.pri,   "dled: "   + count_dled,      false, true); print(print_type.def, " ", false, true); }
                if (count_dupes     != 0) { print(print_type.war,   "dupes: "  + count_dupes,     false, true); print(print_type.def, " ", false, true); }
                if (count_replaced  != 0) { print(print_type.war,   "replc: "  + count_replaced,  false, true); print(print_type.def, " ", false, true); }
                if (count_skipped   != 0) { print(print_type.war,   "skip: "   + count_skipped,   false, true); print(print_type.def, " ", false, true); }
                if (count_disposed  != 0) { print(print_type.eri,   "junk: "   + count_disposed,  false, true); print(print_type.def, " ", false, true); }
                if (count_failed    != 0) { print(print_type.eri,   "fail: "   + count_failed,    false, true);                                             }
            }
            else
            {
                print(print_type.def, "", true, true);
                print(print_type.def, "", true, true);
                print(print_type.top, "===[ DEBUG ]",                                                                                              true, true);
                print(print_type.inf, "total/found/all...: " + count_total,                                                                        true, true);
                print(print_type.inf, "accepted/valid....: " + count_accepted,                                                                    true, true);
                print(print_type.cur, "count/processed...: " + count_processed,                                                                    true, true);
                print(print_type.pri, "dled/downloaded...: " + count_dled + " (" + ROund(count_dled_bytes) + " (" + count_dled_bytes + " bytes))", true, true);
                print(print_type.war, "dupes/renamed.....: " + count_dupes,                                                                        true, true);
                print(print_type.war, "replaced..........: " + count_replaced,                                                                     true, true);
                print(print_type.war, "skipped/found.....: " + count_skipped,                                                                      true, true);
                print(print_type.eri, "disposed/rejected.: " + count_disposed,                                                                     true, true);
                print(print_type.eri, "failed............: " + count_failed,                                                                       true, true);
            }
            
        }
        private static void print_info_thread()
        { while (true) { Console.ReadKey(true); print_info(); } }
        private static void print_info_thread_compact()
        {
            while (true)
            {
                print_info(true);

                Thread.Sleep(50);
            }
        }

        private static void get_options(string options_string)
        {
            string[] ent = options_string.Split(',');

            foreach (string item in ent)
            {
                if (string.IsNullOrEmpty(item)) { continue; }

                if (item.StartsWith("@"))
                {
                    switch (item.Remove(0, 1))
                    {
                        case "compact"    : COMPACT     = true;                  break;
                        case "count"      : COUNT       = true;                  break;
                        case "color"      : COLOR       = true;                  break;
                        case "clean"      : CLEAN       = true;                  break;
                        case "valid"      : VALID       = true;                  break;
                        case "replace"    : REPLACE     = true;                  break;
                        case "skip"       : SKIP        = true; skip_run();      break;
                        case "skip_find"  : SKIP_FIND   = true; skip_find_run(); break;
                        case "skip_file"  : SKIP_FILE   = true; skip_file_run(); break;
                        case "skip_output": SKIP_OUTPUT = true;                  break;
                        case "skip_case"  : SKIP_CASE   = true;                  break;
                        case "skip_ext"   : SKIP_EXT    = true;                  break;
                        case "skip_all"   :
                            {
                                SKIP_FIND   = true; skip_find_run();
                                SKIP_FILE   = true; skip_file_run();
                                SKIP_CASE   = true;
                                SKIP_EXT    = true;
                                SKIP_OUTPUT = true;
                                break;
                            }
                        case "nodupes" : NODUPES   = true;                            break;
                        case "verbose" : VERBOSE   = true; COUNT = true;              break;
                        case "filename": FILENAME  = true;                            break;
                        case "agent"   : wc.Headers.Add("user-agent", "webgrab.exe"); break;

                        case "media":
                            {
                                only_bool = true;
                                foreach (string type in TYPES_VIDEO) { only.Add(type); }
                                foreach (string type in TYPES_IMAGE) { only.Add(type); }
                                foreach (string type in TYPES_OTHER) { only.Add(type); }
                                break;
                            }
                        case "video": only_bool = true; foreach (string type in TYPES_VIDEO) { only.Add(type); } break;
                        case "image": only_bool = true; foreach (string type in TYPES_IMAGE) { only.Add(type); } break;
                        case "other": only_bool = true; foreach (string type in TYPES_OTHER) { only.Add(type); } break;
                        case "codec": only_bool = true; foreach (string type in TYPES_CODEC) { only.Add(type); } break;

                        case "grab":
                            {
                                CLEAN = true;
                                COLOR = true;
                                COUNT = true;
                                VALID = true;

                                only_bool = true;
                                foreach (string type in TYPES_VIDEO) { only.Add(type); }
                                foreach (string type in TYPES_IMAGE) { only.Add(type); }
                                foreach (string type in TYPES_OTHER) { only.Add(type); }

                                ignore_bool = true;
                                ignore.Add("thumb");

                                break;
                            }
                    }
                }
                else if (item.StartsWith("~")) { string str = item.Remove(0, 1); if (!string.IsNullOrEmpty(str)) { only_bool = true; only.Add(str); } }
                else if (item.StartsWith("-")) { string str = item.Remove(0, 1); if (!string.IsNullOrEmpty(str)) { ignore_bool = true; ignore.Add(str); } }
                else                           { only_must_bool = true; only_must.Add(item); }
            }
        }
        private static bool url_is_valid(string link)
        {
            //CHECK IF LINK IS EMPTY
            if (string.IsNullOrEmpty(link)) { count_disposed++; return false; }
            
            //IF URL IS VALID BY THE URI CLASS
            if (VALID) { if (!Uri.IsWellFormedUriString(link, UriKind.Absolute)) { count_disposed++; return false; } }
            
            bool skip = false;
            System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("");
            if (only_bool)
            {
                skip = true;
                foreach (string item in only) //can be (ex. mp4;mp3;webm;jpg;png <- must be in this list)
                {
                    if (ci.CompareInfo.IndexOf(link, item, System.Globalization.CompareOptions.IgnoreCase) >= 0)
                    { skip = false; break; }
                }
            }
            if (only_must_bool)
            {
                foreach (string item in only_must) //must be (ex. "thumb")
                {
                    if (!(ci.CompareInfo.IndexOf(link, item, System.Globalization.CompareOptions.IgnoreCase) >= 0))
                    { skip = true; }
                }
            }
            if (ignore_bool)
            {
                foreach (string item in ignore)
                {
                    if (ci.CompareInfo.IndexOf(link, item, System.Globalization.CompareOptions.IgnoreCase) >= 0)
                    { skip = true; break; }
                }
            }
            if (skip) { count_disposed++; return false; }
            
            //FILE SKIPING (very slow, that's why it's the last test)
            if (SKIP_OUTPUT)
            {
                //get filename from url...
                string filename = string.Empty; try { filename = System.IO.Path.GetFileName(new Uri(link).LocalPath); } catch (Exception) { }

                //...and if it's not empty test file skip...
                if (!string.IsNullOrEmpty(filename))
                {
                    /* ULTRA SKIP  */ if (SKIP_FIND ||  SKIP_FILE) { foreach (string file in skip_list) { if (compare(filename, file, SKIP_CASE, SKIP_EXT)) { count_accepted++; count_skipped++; return false; } } }
                    /* NORMAL SKIP */ if (SKIP      && !SKIP_FIND) { foreach (string file in skip_list) { if (compare(filename, file, SKIP_CASE, SKIP_EXT)) { count_accepted++; count_skipped++; return false; } } }
                }
            }
            
            count_accepted++;
            return true;
        }
        private static int check_URLorFILE(string input)
        {
            if (string.IsNullOrEmpty(input)) { print(print_type.cer, "input is blank." + input, true, true); return 0; }
            
            bool isURL = false;
            bool isFile = false;
            if (File.Exists(input)) { isFile = true; }
            if (Uri.IsWellFormedUriString(input, UriKind.Absolute)) { isURL = true; }
            
            if (!isURL && !isFile)
            {
                print(print_type.cer, "not an url or file: " + input, true, true);
                return 0;
            }
            else if (isURL) { return 1; }
            else if (isFile) { return 2; }

            return 0;
        }
        
        private static int wc_dl(string url_)
        {
            count_processed++;
            
            string filename = time();

            try
            {
                filename = System.IO.Path.GetFileName(new Uri(url_).LocalPath);
            }
            catch (System.NullReferenceException e) { if (!CLEAN) { print(print_type.err, "ERR[NullReferenceException] " + e.Message); } }
            catch (System.UriFormatException e)     { if (!CLEAN) { print(print_type.err, "ERR[UriFormatException] "     + e.Message); } }
            catch (System.ArgumentNullException e)  { if (!CLEAN) { print(print_type.err, "ERR[ArgumentNullException] "  + e.Message); } }
            catch (System.Exception e)              { if (!CLEAN) { print(print_type.err, "ERR[UNK] "                    + e.Message); } }
            if (string.IsNullOrEmpty(filename)) { filename = time(); }                                                   

            if (!CLEAN)
            {
                             print(print_type.cur,  "DING",                      false); 
                if (COUNT) { print(print_type.cur,  "[" + count_processed + "]", false); }
                             print(print_type.cur,  ": " + url_,                 false);
                             print(print_type.def,       " ",                         false);
                             print(print_type.cur,  "(" + filename + ")",        false);
                             print(print_type.def,       "  ",                       false); //space for errors/skips/...
            }

            try
            {

                /* ULTRA SKIP  */ if (SKIP_FIND ||  SKIP_FILE) { foreach (string file in skip_list) { if (compare(filename, file, SKIP_CASE, SKIP_EXT)) { count_skipped++; if (!CLEAN) { print(print_type.war, "WAR[SKIPPED] " + filename); } return 0; } } }
                /* NORMAL SKIP */ if (SKIP      && !SKIP_FIND) { foreach (string file in skip_list) { if (compare(filename, file, SKIP_CASE, SKIP_EXT)) { count_skipped++; if (!CLEAN) { print(print_type.war, "WAR[SKIPPED] " + filename); } return 0; } } }
                
                if (File.Exists(filename))
                {
                    if (REPLACE)
                    {
                        if (File.Exists(filename)) { File.Delete(filename); }

                        count_replaced++;
                        if (!CLEAN) { print(print_type.war, "WAR[REPLACED] " + filename, false); }
                    }
                    else
                    {
                        //GET NEW FILENAME
                        FileInfo fff = new FileInfo(filename); //get file info for our path
                        int c = 1; //++
                        filename = Path.GetFileNameWithoutExtension(fff.Name) + " [" + c + "]" + fff.Extension;
                        //if file exists add a number to it

                        //keep adding the number until the file doesn't exist
                        while (File.Exists(filename))
                        {
                            c++;
                            filename = Path.GetFileNameWithoutExtension(fff.Name) + " [" + c + "]" + fff.Extension;
                        }

                        count_dupes++;
                        if (!CLEAN) { print(print_type.war, "WAR[RENAMED] " + filename, false); }
                    }
                }

                //add filename to skip list if it appears twice
                skip_list.Add(filename);

                try
                {
                    //DOWNLOAD
                    wc.DownloadFile(url_, filename + ".webgrab_download"); //own part file
                }
                //download exceptions
                catch (System.Net.WebException e)
                {
                    count_failed++;

                    if (!CLEAN) { print(print_type.err, "ERR[WebException] " + e.Message); }
                    
                    return returnStatusCode(e);
                }
                catch (System.ArgumentNullException e)  { count_failed++; if (!CLEAN) { print(print_type.err, "ERR[ArgumentNullException] "  + e.Message); } return e.HResult; }
                catch (System.NullReferenceException e) { count_failed++; if (!CLEAN) { print(print_type.err, "ERR[NullReferenceException] " + e.Message); } return e.HResult; }
                catch (System.Exception e)              { count_failed++; if (!CLEAN) { print(print_type.err, "ERR[UNK] "                    + e.Message); } return e.HResult; }
                if (!CLEAN) { print(print_type.def, ""); }

                //RENAME
                File.Move(filename + ".webgrab_download", filename); //rename part to it's filename

                //COUNT
                count_dled++;

                FileInfo fi = new FileInfo(filename); //get new info

                //add to size
                count_dled_bytes = count_dled_bytes + fi.Length;

                             print(print_type.pri, "DLED",                                                 false);
                if (COUNT) { print(print_type.pri, "[" + count_dled + "]",                                 false); }
                             print(print_type.pri, ": " + url_ + " (" + filename + ")",                    false);
                             print(print_type.def, " ",                                                    false);
                             print(print_type.inf, "(" + ROund(fi.Length) + " (" + fi.Length + " bytes))", false);
                             print(print_type.def, " ",                                                    true);
            }
            catch (Exception) { return 1; } //other exception
            return 0;
        }

        private static List<string> get_html_all(string URL, byte[] html)
        {
            //GET ALL CHARS
            char[] htmlChars = System.Text.Encoding.Default.GetString(html).ToArray();

            //EXTRACT ALL STRINGS LIKE: 'asdsadas' "asdasdasd"
            List<string> html_quotes = html_get_everyobject_in_quotes(htmlChars);
            List<string> html_apostrophes = html_get_everyobject_in_apostrophes(htmlChars);

            //ADD
            List<string> non_rooted = new List<string>();
            foreach (string l in html_quotes) { non_rooted.Add(l); }
            foreach (string l in html_apostrophes) { non_rooted.Add(l); }

            /// Uri myUri = new Uri("http://www.contoso.com:8080/");
            /// string host = myUri.Host;  // host is "www.contoso.com"

            //root urls
            List<string> rooted_urls = new List<string>();
            foreach (string url in non_rooted)
            {
                string item = System.Web.HttpUtility.HtmlDecode(url);
                if (Uri.IsWellFormedUriString(item, UriKind.Absolute)) { rooted_urls.Add(item); continue; }

                if (URL != null)
                {
                    if (item.StartsWith("//")) { rooted_urls.Add(URL.Split(':')[0] + ":" + item); continue; } //add http or https
                    else if (item.StartsWith("/")) //add website name
                    {
                        if (URL.EndsWith("/")) { rooted_urls.Add(URL + item); }
                        else                   { rooted_urls.Add(URL + "/" + item); }
                        continue;
                    }
                }

                rooted_urls.Add(item);
            }

            //ROOT
            List<string> html_all = rooted_urls;
            count_total = html_all.Count;

            if (NODUPES) //CLEAR DUPLICATES
            {
                int before = html_all.Count;

                html_all = html_all.Distinct().ToList(); //REMOVE SAME URLS

                int after = html_all.Count;
                count_disposed += before - after;
            }

            return html_all;
        }
        private static List<string> html_get_everyobject_in_apostrophes(char[] htmlChars)
        {
            //get urls like this: blablablablablablabla "some url we want" blablablablabla

            List<string> links = new List<string>();
            string link = "";
            bool afterQuote = false;
            foreach (char ch in htmlChars)
            {
                if (ch == '\'')
                {
                    afterQuote = !afterQuote;

                    if (!afterQuote)
                    {
                        links.Add(link);
                        link = "";
                    }
                }
                else if (afterQuote)
                {
                    link = link + ch; //add chars to string after quote
                }
            }

            return links;
        }
        private static List<string> html_get_everyobject_in_quotes(char[] htmlChars)
        {
            //get urls like this: blablablablablablabla "some url we want" blablablablabla

            List<string> links = new List<string>();
            string link = "";
            bool afterQuote = false;
            foreach (char ch in htmlChars)
            {
                if (ch == '"')
                {
                    afterQuote = !afterQuote;

                    if (!afterQuote)
                    {
                        links.Add(link);
                        link = "";
                    }
                }
                else if (afterQuote)
                {
                    link = link + ch; //add chars to string after quote
                }
            }

            return links;
        }
        
        private static int RunOption_outUrls(string[] args)
        {
            if (args.Length >= 2)
            {
                ///STANDARD SETUP
                if (args.Length == 3) { get_options(args[2]); } //get options
                string input = args[1];

                byte[] html = null;
                int check = check_URLorFILE(input);
                switch (check)
                {
                    case 0:
                    default: return 1;

                    case 1:
                        {
                            try { html = wc.DownloadData(new Uri(input)); }
                            catch (System.Net.WebException e)       { print(print_type.cer, "ERR[WebException] "           + e.Message, true, true); return returnStatusCode(e); }
                            catch (System.ArgumentNullException e)  { print(print_type.cer, "ERR[ArgumentNullException] "  + e.Message, true, true); return e.HResult;           }
                            catch (System.NullReferenceException e) { print(print_type.cer, "ERR[NullReferenceException] " + e.Message, true, true); return e.HResult;           }
                            catch (System.Exception e)              { print(print_type.cer, "ERR[UNK] "                    + e.Message, true, true); return e.HResult;           }
                            break;
                        }
                    case 2: html = File.ReadAllBytes(input); break;
                }
                
                //HTML SCAN
                List<string> html_all = get_html_all(check == 1 ? input : null, html);
                List<string> final = new List<string>();

                int longest_numb = 3, longest_url = 3;
                foreach (string link in html_all) { if (url_is_valid(link)) { if (link.Length > longest_url) { longest_url = link.Length; } final.Add(link); } }
                if (final.Count.ToString().Length > longest_numb) { longest_numb = final.Count.ToString().Length; }


                if (VERBOSE) //verbose output
                {
                    string format = "{0,-" + longest_numb + "} {1,-" + longest_url + "} {2,0}";
                    Console.WriteLine(format, "NUM", "URL", "FILENAME");

                    foreach (string link in final)
                    { count_processed++; try { Console.WriteLine(format, count_processed, link, System.IO.Path.GetFileName(new Uri(link).LocalPath)); }
                                         catch (Exception) { count_failed++; } }

                    print_info();
                }
                else if (COUNT && !VERBOSE) { foreach (string link in final) { count_processed++; print(print_type.def, "[" + count_processed + "] " + link, true, true); } }
                else if (FILENAME)          { foreach (string link in final) { count_processed++; try { Console.WriteLine(System.IO.Path.GetFileName(new Uri(link).LocalPath)); } catch (Exception) { count_failed++; } } }
                else                        { foreach (string link in final) { count_processed++; print(print_type.def, link, true, true); } }

            }
            else
            {
                print(print_type.cer, "webgrab.exe --out \"URL\" \"switch1,sw2,sw3,sw4,...\"", true, true);
                return 1;
            }

            return 0;
        }
        private static int RunOption_testWebsite(string[] args)
        {
            if (args.Length >= 2)
            {
                ///STANDARD SETUP
                if (args.Length == 3) { get_options(args[2]); } //get options
                string input = args[1];

                print(print_type.top, "===[ TESTING WEBSITE ]");

                byte[] html = null;
                int check = check_URLorFILE(input);
                switch (check)
                {
                    case 0:
                    default: return 1;

                    case 1:
                        {
                            try { html = wc.DownloadData(new Uri(input)); }
                            catch (System.Net.WebException e)       { print(print_type.cer, "ERR[WebException] "           + e.Message, true, true); return returnStatusCode(e); }
                            catch (System.ArgumentNullException e)  { print(print_type.cer, "ERR[ArgumentNullException] "  + e.Message, true, true); return e.HResult;           }
                            catch (System.NullReferenceException e) { print(print_type.cer, "ERR[NullReferenceException] " + e.Message, true, true); return e.HResult;           }
                            catch (System.Exception e)              { print(print_type.cer, "ERR[UNK] "                    + e.Message, true, true); return e.HResult;           }
                            break;
                        }
                    case 2: html = File.ReadAllBytes(input); break;
                }

                
                print(print_type.pri, "html size (DLED)..: " + ROund(html.Length) + " (" + html.Length + " bytes)", true, true);

                //HTML SCAN
                print(print_type.inf, "title from html...: " + get_html_title(html), true, true);

                List<string> html_all = get_html_all(check == 1 ? input : null, html);
                print(print_type.inf, "total items.......: " + count_total, true ,true);
                
                foreach (string str in html_all) { url_is_valid(str); /*get vars*/ }
                print(print_type.inf, "accepted items....: " + count_accepted, true, true);
                print(print_type.war, "skipped/found.....: " + count_skipped, true, true);
                print(print_type.eri, "disposed/rejected.: " + count_disposed, true, true);
            }
            else
            {
                print(print_type.cer, "webgrab.exe --test \"URL\" \"switch1,sw2,sw3,sw4,...\"");
                return 1;
            }

            return 0;
        }
        private static int RunOption_read(string[] args)
        {
            if (args.Length >= 2)
            {
                if (args.Length == 3) { get_options(args[2]); } //get options
                if (!File.Exists(args[1])) { print(print_type.err, "file not found: " + args[1]); return 1; } //CHECK FILE

                print(print_type.def, "");
                print(print_type.top, "===[ SCANNING TEXT FILE ]");
                string[] links_from_file = null;
                try
                {
                    //HTML SCAN
                    links_from_file = File.ReadAllLines(args[1]);
                }
                catch (System.ArgumentException e)             { print(print_type.err, "ERR[ArgumentException] "           + e.Message); return e.HResult; }
                catch (System.IO.PathTooLongException e)       { print(print_type.err, "ERR[PathTooLongException] "        + e.Message); return e.HResult; }
                catch (System.IO.DirectoryNotFoundException e) { print(print_type.err, "ERR[DirectoryNotFoundException] "  + e.Message); return e.HResult; }
                catch (System.IO.IOException e)                { print(print_type.err, "ERR[IOException] "                 + e.Message); return e.HResult; }
                catch (System.UnauthorizedAccessException e)   { print(print_type.err, "ERR[UnauthorizedAccessException] " + e.Message); return e.HResult; }
                catch (System.NotSupportedException e)         { print(print_type.err, "ERR[NotSupportedException] "       + e.Message); return e.HResult; }
                catch (System.Security.SecurityException e)    { print(print_type.err, "ERR[SecurityException] "           + e.Message); return e.HResult; }

                print(print_type.inf, "total items....: " + links_from_file.Length);
                count_total = links_from_file.Length;
                List<string> accepted_items = new List<string>();
                foreach (string str in links_from_file) { if (url_is_valid(str)) { accepted_items.Add(str); } }
                print(print_type.inf, "accepted items.: " + accepted_items.Count);

                print(print_type.def, "");
                print(print_type.top, "===[ DOWNLOADING ]");
                foreach (string l in accepted_items) { wc_dl(l); }

                //END
                print_info();
                return 0;
            }
            else
            {
                print(print_type.cer, "webgrab.exe --read \"file_with_urls.txt\" \"switch1,sw2,sw3,sw4,...\"");
                return 1;
            }
        }
        private static int RunOption_listen(string[] args)
        {
            if (args.Length == 2) { get_options(args[1]); }

            print(print_type.def, "");
            print(print_type.top, "===[ DOWNLOADING COPIED URLS ]");
            
            bool exit = false;
            string pastUrl = string.Empty;
            while (!exit)
            {
                Thread.Sleep(20);
                string clip = System.Windows.Clipboard.GetText(TextDataFormat.Text);
                if (pastUrl == clip) { continue; }
                if (url_is_valid(clip)) { wc_dl(clip); }
                pastUrl = clip;
            }

            return 0;
        }

        //OTHER FUNCTIONS
        private static string get_html_title(byte[] html)
        {
            return System.Net.WebUtility.HtmlDecode(get_string_in_between("<title>", "</title>", System.Text.Encoding.ASCII.GetString(html), false, false));
        }
        private static string get_string_in_between(string strBegin, string strEnd, string strSource, bool includeBegin, bool includeEnd)
        {
            string[] result = { string.Empty, string.Empty };
            int iIndexOfBegin = strSource.IndexOf(strBegin);

            if (iIndexOfBegin != -1)
            {
                // include the Begin string if desired 
                if (includeBegin) { iIndexOfBegin -= strBegin.Length; }

                strSource = strSource.Substring(iIndexOfBegin + strBegin.Length);

                int iEnd = strSource.IndexOf(strEnd);
                if (iEnd != -1)
                {
                    // include the End string if desired 
                    if (includeEnd) { iEnd += strEnd.Length; }
                    result[0] = strSource.Substring(0, iEnd);

                    // advance beyond this segment 
                    if (iEnd + strEnd.Length < strSource.Length) { result[1] = strSource.Substring(iEnd + strEnd.Length); }
                }
            }
            else
            {
                // stay where we are 
                result[1] = strSource;
            }

            return result[0];
        }
        private static string ROund(double len)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }
        private static string time()
        {
            TimeSpan diff = (new DateTime(2011, 02, 10) - new DateTime(2011, 02, 01));
            return diff.TotalMilliseconds.ToString();
        }
        private static string bool_to_string(bool i) { return i ? "true" : "false"; }
        private static string remove_ext(string fileNameWithExt)
        {
            //return
            if (!fileNameWithExt.Contains('.')) { return fileNameWithExt; }
            
            //count dots
            int dots = 0;
            foreach(char ch in fileNameWithExt) { if (ch == '.') { dots++; } }
            
            string fileNameWithoutExt = "";

            //add chars to string until the last dot
            int count_dot = 0;
            foreach (char ch in fileNameWithExt)
            {
                if (ch == '.') { count_dot++; }
                if (count_dot == dots) { break; }
                fileNameWithoutExt = fileNameWithoutExt + ch;
            }

            return fileNameWithoutExt;
        }
        private static int returnStatusCode(System.Net.WebException e)
        {
            HttpWebResponse response = (HttpWebResponse)e.Response;

            HttpStatusCode statusCode = response.StatusCode;
            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string sResponse = reader.ReadToEnd();

            // //DEBUG
            // Console.WriteLine(sResponse); //html
            // Console.WriteLine("Response Code: " + (int)statusCode + " - " + statusCode.ToString()); //e.g. "Response Code: 404 - NotFound"

            return (int)statusCode;
        }
        private static bool compare(string str1, string str2, bool ignore_case = false, bool ignore_ext = false)
        {
            if (ignore_ext)
            {
                if (ignore_case) { if (string.Equals(remove_ext(str1), remove_ext(str2), StringComparison.OrdinalIgnoreCase)) { return true; } }
                else             { if (remove_ext(str1) == remove_ext(str2))                                                  { return true; } }
            }
            else
            {
                if (ignore_case) { if (string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase)) { return true; } }
                else             { if (str1 == str2)                                                  { return true; } }
            }
            
            return false;
        }
        private static void skip_run() { if (!SKIP_FIND) { foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.TopDirectoryOnly)) { skip_list.Add(new FileInfo(file).Name); } } }
        private static void skip_find_run() { foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*.*", SearchOption.AllDirectories)) { skip_list.Add(new FileInfo(file).Name); } }
        private static void skip_file_run()
        {
            //add filenames (lines) in *.webgrab_skip files
            foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*.webgrab_skip", SearchOption.AllDirectories))
            {
                string[] lines = File.ReadAllLines(file);
                skip_list.Add(file + " " + lines.Length);
                foreach (string line in lines) { if (!string.IsNullOrEmpty(line)) { if (!line.StartsWith("#")) { skip_list.Add(line); } } }
            }
        }
    }
}
