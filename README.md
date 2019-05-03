# webgrab
web page file scanner / downloader (C#, console)

> webgrab --help
```
 +===[ ABOUT ]
 | ABOUT....: web page file scanner / downloader
 | BUILT IN.: C# .NET 4.6.1
 | Version..: 71
 | Author...: 0xC0LD
 | USAGE....: webgrab.exe <webpage url / RO> <switch1,sw2,sw3,sw4,...>

 +===[ RUN OPTIONS (RO) ]
 | --help "switch1,sw2,sw3,sw4,..."                         = shows this (help)
 | --list "http(s)://www.webpage_url.ext/$$$$$" "from" "to" = list generator (for .bat/.cmd)
 | --out "webpage url" "switch1,sw2,sw3,sw4,..."            = no downloading, only output URL(s)
 | --test "webpage url" "switch1,sw2,sw3,sw4,..."           = no downloading, just test the website
 | --read "file_with_urls.txt" "switch1,sw2,sw3,sw4,..."    = read a .txt file and treat every line as an URL, then download them
 | --watch "switch1,sw2,sw3,sw4,..."                        = download copied (clipboard) URL(s)

 +===[ RULES / SWITCHES ]
 | +==[DOWNLOAD RULES]
 | | @valid         = only download valid URI addresses
 | | -<type/name>   = ignore (ex. -thumb.jpg)
 | | @replace       = if file exists, replace the file...
 | | @skip          = if file exists, skip the url...
 | |   @skip_find   = search for the file in subdirectories and if it exists skip it
 | |   @skip_file   = make your own *.webgrab_skip file(s) that contain filenames (each on every line) to skip...
 | |   @skip_ext    = ignore extensions when comparing filenames
 | |   @skip_case   = ignore case when comparing filenames
 | |   @skip_output = test existence of file before output (useful if you use --out/--test/...)
 | |   @skip_all    = @skip,@skip_find,@skip_file,@skip_ext,@skip_case,@skip_output
 | | @nodupes       = dispose duplicate URL(s)...
 | | @agent         = add User-Agent to request header (403 error fix)
 |
 | +=[DISPLAY RULES]
 | | @compact  = only print numbers (on a single line)
 | | @clean    = don't print errors, (only successful downloads (DLED))
 | | @count    = count items
 | | @verbose  = print current progress on key press... with --out print debug... (+@count)
 | | @filename = for --out, only print filename(s)
 | | @color    = use colors in output
 |
 | +==[FILE RULES]
 | | @media = @video,@image,@other
 | | @grab  = @clean,@color,@count,@valid,@media,-thumb 
 | | @video = ~.mp4,~.webm,~.avi,~.mov,~.mkv,~.flv,~.mpeg,~.mpg,~.wmv,~.mp3,~.ogg
 | | @image = ~.jpg,~.jpeg,~.jpe,~.jiff,~.jfif,~.png,~.gif,~.ico,~.svg,~.bmp,~.dib,~.tif,~.tiff
 | | @other = ~.zip,~.rar,~.exe,~.swf,~.dll,~.txt
 | | @codec = ~.asp,~.aspx,~.axd,~.asx,~.asmx,~.ashx,~.css,~.cfm,~.yaws,~.swf,~.html,~.htm,~.xhtml,~.jhtml,~.jsp,~.jspx,~.wss,~.do,~.action,~.js,~.pl,~.php,~.php4,~.php3,~.phtml,~.py,~.rb,~.rhtml,~.shtml,~.xml,~.rss,~.svg,~.cgi,~.dll

 +===[ MORE INFO / EXAMPLES ]
 | - if skip and replace are specified, file download will be skipped...
 |
 | > webgrab <url> @rule,keyword,-keyword,~keyword
 |      @rule     = download rule / variable
 |      keyword   = keyword that must be present
 |      -keyword  = keyword that must NOT be present
 |      ~keyword  = keyword that should / could be present
 |
 | > webgrab.exe <url> @clean,.jpg
 | > webgrab.exe --list "webgrab.exe \"https://some_website.net/$$$$$\" @grab,@clean" 1 30000 > download.bat
```
