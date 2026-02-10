namespace TRPGLogArrangeTool.Blazor.Services
{
    public static class HtmlResource
    {
        public const string ChatColor = @".chat.{0} .chatOneLine,.chat.{0} 
span{{
 color: {1} ; 
 white-space: normal;
 word-wrap: break-word;
}}";

        public const string DivChatArea = @"<div class=""chatOneLine"">{0}</div>";

        public const string DivChatTextArea = @"<div class=""chatTextArea"">";

        public const string DivChatUserETC = @"<div class=""chat {0} right"">";

        public const string DivChatUserMain = @"<div class=""chat {0}"">";

        public const string DivChatUserSecret = @"<div class=""chat {0}  secretChat"">";

        public const string DivEndLine = @"</div>
</div>";

        public const string DivIcon = @"<div class=""icon image_{0} center""></div>";

        public const string DivMainChat = @"<div class=""chatinfo""><span class=""namearea"">{0}</span> <span class=""tabnamearea"">[{1}]</span></div>";

        public const string DivMainChatETC = @"<div class=""chatinfo""><span class=""namearea"">{0}</span> <span class=""tabnamearea"">[{1}]</span></div>";

        public const string EventCharacter = @"<div class=""chatImage_Icon"">
 <img src=""data:image/png;base64,{0}""      alt=""EventImage"">
</div>";

        public const string EventImage = @"<div class=""chatImage"">
 <img src=""data:image/png;base64,{0}""      alt=""EventImage"">
</div>";

        public const string HTMLFooter = @"</body>
</html>";

        public const string HTMLHeader = @"<!DOCTYPE html>
<html lang=""ja"">
    <head>
        <meta charset=""utf-8"">
        <title></title>
<style>
.icon {
  width: 80px;
  height: 80px;
  margin: 10px;
  background-image: url(""icon.png"");
  background-repeat: no-repeat;
  background-position: center;
  background-size: contain;
}
.chat {
background-color: #F8F8F8;
display: flex;
margin-top: 5px;
margin-bottom: 5px;
}
a.charactorSheet {
text-decoration: none;
}
.chat.right {
text-align: right;
background-color: #CCCCCC;
 flex-direction: row-reverse;
}
.chatTextArea {
display: inline-block;
flex: 1;
vertical-align: top;
padding: 10px 15px;
word-break: break-all;
white-space: normal;
}
.chatTextArea .namearea{
font-weight: bold;
}
.chat.secretChat{
text-align: right;
background-color: #FA8072;
 flex-direction: row-reverse;
}
.width100{
width: 100%;
}
/* タブ */
.chatinfo{
 font-size: 75%;
}
.chatImage{
 margin: 10px;
}
.chatImage img {
  max-width: 70%;
  border: 1px solid #ccc;
  border-radius: 8px;
  display: block;
  margin: 12px auto;
}
.chatImage_Icon{
 margin: 10px;
}
.chatImage_Icon img {
  max-width: 20%;
  border: 1px solid #ccc;
  border-radius: 8px;
  display: block;
  margin: 12px auto;
}
.chatOneLine{
 border-bottom: 0px;
}
";

        public const string ImageData = @"background-image: url(data:image/png;base64,{0});";

        public const string Imagefooter = @"background-repeat: no-repeat;
background-size: contain;
}
";

        public const string ImageHeader = @".image_{0}{{";

        public const string StringInfoEN = "info";
        public const string StringInfoJP = "情報";
        public const string StringMainEN = "main";
        public const string StringMainJP = "メイン";
        public const string StringOtherEN = "other";
        public const string StringOtherJP = "雑談";
        public const string StringSecretEN = "secret";
        public const string StringSecretJP = "秘匿";
        public const string StyleEnd = @"</style>
    </head>
    <body>";
    }
}
