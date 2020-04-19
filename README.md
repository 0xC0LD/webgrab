# webgrab
- Web Page: Crawler / File Scanner / Downloader (C#, console)

## DOWNLOAD
\[[here](https://github.com/0xC0LD/webgrab/raw/master/webgrab/webgrab/bin/Release/webgrab.exe)\]

## HELP
> webgrab
```
 +===[ ABOUT ]
 | ABOUT....: web page file scanner / downloader
 | BUILT IN.: C# .NET 4.6.1
 | Version..: 75
 | Author...: 0xC0LD
 | USAGE....: webgrab.exe <webpage url / file.html / RO> <switch1,sw2,sw3,sw4,...>

 +===[ RUN OPTIONS (RO) ]
 | --help "switch1,sw2,sw3,sw4,..."                           = shows this (help)
 | --out "webpage url / file.html" "switch1,sw2,sw3,sw4,..."  = no downloading, only output URL(s)
 | --test "webpage url / file.html" "switch1,sw2,sw3,sw4,..." = no downloading, just print counts
 | --read "file_with_urls.txt" "switch1,sw2,sw3,sw4,..."      = read a .txt file and treat every line as an URL, then download them
 | --watch "switch1,sw2,sw3,sw4,..."                          = download copied (clipboard) URL(s)

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
 | DOWNLOAD YOUTUBE THUMBNAIL: 
 |  > webgrab.exe "https://www.youtube.com/watch?v=XXXXXXXXXXX" @valid,@image,@nodupes,maxres
 |
 | [...]
```

## EXAMPLES
#### download files from a 4chan thread:
> webgrab "https://boards.4channel.org/???/thread/???????" @valid,@media,@nodupes,-s.,@color

#### download a youtube thumbnail:
> webgrab <https://www.youtube.com/watch?v=???????????> @valid,@media,@nodupes

### how it works:
It finds strings on a webpage and filters them by the given options.
Then it downloads/outputs the results.