using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TRPGLogArrangeTool.Blazor.Models;

namespace TRPGLogArrangeTool.Blazor.Services
{
    /// <summary>
    /// ログ解析結果を保持するクラス
    /// </summary>
    public class LogArrangeResult
    {
        /// <summary>
        /// 解析が成功したかどうか
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 実行結果に関するメッセージ、またはエラーメッセージ
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 生成されたHTMLコンテンツ
        /// </summary>
        public string HtmlContent { get; set; }

        /// <summary>
        /// 発生した個別エラー（画像読み込み失敗など）のリスト
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();
    }

    /// <summary>
    /// TRPGのチャットログを解析・整形するメインサービス
    /// </summary>
    public class LogArrangeService
    {
        private const string CONST_EVENT_AREA = "EVENT";
        private const string NAME_EVENT = "EVENT_IMAGE";
        private const string NAME_EVENT_CHARACTER = "EVENT_CHARACTER_IMAGE";
        private const string zipName = "chat.xml";
        private const string zipNameFly = "fly_chat.xml";
        private const string zipStandFly = "fly_data.xml";
        private static readonly string[] allowedExtensions = new[] { ".png", ".jpg", ".jpeg" };
        private const string COLOR_FFFFFF = "FFFFFF";

        private readonly ImageService _imageService;

        /// <summary>
        /// 解析によって抽出されたキャラクター名のリスト
        /// </summary>
        public List<ChatName> ChatNameList { get; private set; } = new List<ChatName>();

        /// <summary>
        /// 解析によって抽出されたメッセージのリスト
        /// </summary>
        public List<ChatMessage> ChatMessageList { get; private set; } = new List<ChatMessage>();

        public LogArrangeService(ImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>
        /// 保持している解析データを初期化します
        /// </summary>
        public void ClearData()
        {
            ChatNameList.Clear();
            ChatMessageList.Clear();
            _imageService.Clear();
            ChatNameList.Add(new ChatName() { Name = NAME_EVENT });
            ChatNameList.Add(new ChatName() { Name = NAME_EVENT_CHARACTER });
        }

        /// <summary>
        /// HTML形式のログを解析します
        /// </summary>
        /// <param name="htmlContent">HTMLの文字列</param>
        /// <returns>解析結果</returns>
        public LogArrangeResult HtmlAnalyze(string htmlContent)
        {
            ClearData();

            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                var ps = doc.DocumentNode.SelectNodes("//p");
                if (ps == null) return new LogArrangeResult { Success = false, Message = "段落タグ (p) が見つかりませんでした。" };

                for (int i = 0; i < ps.Count; i++)
                {
                    HtmlNode p = ps[i];
                    var spans = p.SelectNodes("./span");
                    if (spans != null)
                    {
                        string area = string.Empty;
                        string name = string.Empty;
                        string message = string.Empty;
                        bool secretTabFlg = false;

                        for (int lp = 0; lp < spans.Count; lp++)
                        {
                            if (lp == 0)
                            {
                                area = spans[lp].InnerText;
                                while (true)
                                {
                                    string oldArea = area;
                                    area = area.Trim('[').Trim(']').Trim(' ');
                                    if (oldArea == area) break;
                                }

                                if (area == HtmlResource.StringMainEN) area = HtmlResource.StringMainJP;
                                else if (area == HtmlResource.StringInfoEN) name = HtmlResource.StringInfoJP;
                                else if (area == HtmlResource.StringOtherEN) area = HtmlResource.StringOtherJP;
                                else if (area.ToLower().Contains(HtmlResource.StringSecretJP) || area.ToLower().Contains(HtmlResource.StringSecretEN))
                                {
                                    secretTabFlg = true;
                                }
                            }
                            else if (lp == 1)
                            {
                                name = NameConverter(spans[lp].InnerText);
                            }
                            else
                            {
                                message = TextHtmlEmbellishment(spans[lp].InnerHtml);
                            }
                        }

                        if (!ChatNameList.Any(x => x.Name == name))
                        {
                            ChatNameList.Add(new ChatName { Name = name });
                        }
                        
                        var chatName = ChatNameList.FirstOrDefault(x => x.Name == name);
                        ChatMessage tmpMessage = new ChatMessage
                        {
                            IsAddedMessage = false,
                            IsSecretMessage = secretTabFlg,
                            Area = area,
                            Name = name,
                            TimeStamp = i * 10,
                            Text = message,
                            ImageKey = chatName?.DefaultImageKey
                        };
                        ChatMessageList.Add(tmpMessage);
                    }
                }
                return new LogArrangeResult { Success = true };
            }
            catch (Exception ex)
            {
                return new LogArrangeResult { Success = false, Message = ex.Message };
            }
        }


        /// <summary>
        /// Zip形式のユドナリウムログを非同期で解析します
        /// </summary>
        /// <param name="zipStream">Zipファイルのストリーム</param>
        /// <param name="detailCheck">画像の詳細解析を行うかどうか</param>
        /// <param name="standCheck">キャラクターの立ち絵情報を読み込むかどうか</param>
        /// <returns>解析結果</returns>
        public async Task<LogArrangeResult> ZipAnalyzeAsync(Stream zipStream, bool detailCheck, bool standCheck)
        {
            ClearData();
            bool flyFlg = false;
            string targetPath = "";
            
            // ZipArchiveのエントリを複数回走査するためにメモリストリームにコピーします。
            using (var memoryStream = new MemoryStream())
            {
                await zipStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true))
                {
                    if (CheckFlyBasic(archive, zipName))
                    {
                        targetPath = zipName;
                    }
                    else if (CheckFlyBasic(archive, zipNameFly))
                    {
                        targetPath = zipNameFly;
                        flyFlg = standCheck;
                    }
                    else
                    {
                        return new LogArrangeResult { Success = false, Message = "Zip内に解析対象のXMLが見つかりませんでした。" };
                    }
                }
                
                memoryStream.Position = 0;
                using (ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Read, true))
                {
                    string xmlContent = ExtractXmlFromZip(archive, targetPath);
                    if (xmlContent == null) return new LogArrangeResult { Success = false, Message = "XMLの抽出に失敗しました。" };
                    
                    // チャットメッセージをパース（画像の抽出も含む）
                    return ParseChatMessages(xmlContent, archive, flyFlg, detailCheck);
                }
            }
        }

        /// <summary>
        /// Zip内に指定されたファイル名が存在するか確認します
        /// </summary>
        private bool CheckFlyBasic(ZipArchive archive, string fileName)
        {
            return archive.Entries.Any(entry => entry.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Zipから指定されたXMLファイルを文字列として抽出します
        /// </summary>
        private string ExtractXmlFromZip(ZipArchive archive, string fileName)
        {
             foreach (ZipArchiveEntry entry in archive.Entries)
             {
                 if (entry.FullName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                 {
                     using (StreamReader reader = new StreamReader(entry.Open()))
                     {
                         return reader.ReadToEnd();
                     }
                 }
             }
             return null;
        }

        /// <summary>
        /// XML文字列からチャットメッセージをパースし、画像を抽出します
        /// </summary>
        private LogArrangeResult ParseChatMessages(string xmlString, ZipArchive archive, bool flyFlg, bool detailCheck)
        {
            List<ChatMessage> tmpMessageList = new List<ChatMessage>();
            List<string> tmpIconFilePathList = new List<string>();
            List<string> errorImage = new List<string>();

            System.Xml.Linq.XElement root = System.Xml.Linq.XElement.Parse(xmlString);

            List<CharacterStandInfo> standInfos = new List<CharacterStandInfo>();
            if (flyFlg)
            {
                standInfos = StandListCreate(archive);
            }

            foreach (var chatTabElement in root.Elements("chat-tab"))
            {
                string tabName = chatTabElement.Attribute("name")?.Value ?? "その他";

                foreach (var chatElement in chatTabElement.Elements("chat"))
                {
                    string strTimeStamp = chatElement.Attribute("timestamp")?.Value ?? "0";
                    long.TryParse(strTimeStamp, out long tmpTimeStamp);

                    string name = chatElement.Attribute("name")?.Value ?? string.Empty;
                    string text = TextHtmlEmbellishment(chatElement.Value.Trim());
                    string imageIdentifier = chatElement.Attribute("imageIdentifier")?.Value ?? string.Empty;
                    if (imageIdentifier == "null") imageIdentifier = string.Empty;

                    if (flyFlg)
                    {
                        string selectedStandName = chatElement.Attribute("standName")?.Value ?? string.Empty;
                        if (!string.IsNullOrEmpty(selectedStandName))
                        {
                            var targetCharacter = standInfos.FirstOrDefault(x => x.Name == name);
                            if (targetCharacter != null && targetCharacter.StandDictionary.TryGetValue(selectedStandName, out string tmpImage))
                            {
                                if (!string.IsNullOrEmpty(tmpImage)) imageIdentifier = tmpImage;
                            }
                        }
                    }

                    if (chatElement.Attribute("to")?.Value != null) continue; // 秘匿はスキップ

                    var chatName = ChatNameList.FirstOrDefault(x => x.Name == name);
                    if (chatName == null)
                    {
                        chatName = new ChatName { Name = name, DefaultImageKey = imageIdentifier };
                        ChatNameList.Add(chatName);
                    }

                    // 詳細解析でない場合は、出力サイズを抑えるためにそのキャラクターで最初に現れた画像のみを使用します。
                    if (!detailCheck && !string.IsNullOrEmpty(chatName.DefaultImageKey))
                    {
                        imageIdentifier = chatName.DefaultImageKey;
                    }

                    if (!tmpIconFilePathList.Contains(imageIdentifier)) tmpIconFilePathList.Add(imageIdentifier);

                    var chatMessage = new ChatMessage
                    {
                        IsAddedMessage = false,
                        IsSecretMessage = false,
                        Area = tabName,
                        Name = name,
                        TimeStamp = tmpTimeStamp,
                        Text = text,
                        ImageKey = imageIdentifier
                    };
                    tmpMessageList.Add(chatMessage);
                }
            }

            // 画像の抽出
            foreach (var entry in archive.Entries)
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(entry.Name);
                string ext = Path.GetExtension(entry.Name).ToLowerInvariant();

                try
                {
                    if (!allowedExtensions.Contains(ext)) continue;

                    var matchingMessages = tmpMessageList.Where(x => x.ImageKey == nameWithoutExt).ToList();
                    if (matchingMessages.Count == 0 && !ChatNameList.Any(cn => cn.DefaultImageKey == nameWithoutExt))
                    {
                          if (matchingMessages.Count == 0) continue;
                    }
                    
                    if (matchingMessages.Count == 0) continue;

                    using (var ms = new MemoryStream())
                    {
                        entry.Open().CopyTo(ms);
                        string base64 = Convert.ToBase64String(ms.ToArray());
                        string key = _imageService.GetOrAddFromBase64(base64, out _);

                        foreach (var msg in matchingMessages)
                        {
                            msg.ImageKey = key;
                        }
                        
                         foreach (var item in ChatNameList.Where(x => x.Name.Trim() == matchingMessages[0].Name.Trim()))
                         {
                             if (!item.ImageKeys.Contains(key)) item.ImageKeys.Add(key);
                         }
                    }
                }
                catch
                {
                    errorImage.Add(Path.GetFileName(entry.Name));
                    continue;
                }
            }

            foreach (var item in tmpMessageList.OrderBy(x => x.TimeStamp))
            {
                ChatMessageList.Add(item);
            }

            var result = new LogArrangeResult { Success = true };
            if (errorImage.Count > 0) result.ErrorMessages.AddRange(errorImage);
            
            return result;
        }

        /// <summary>
        /// Zip内の fly_data.xml からキャラクターの立ち絵情報のリストを作成します
        /// </summary>
        private List<CharacterStandInfo> StandListCreate(ZipArchive archive)
        {
            List<CharacterStandInfo> characterStandList = new List<CharacterStandInfo>();
            string fileData = string.Empty;

            foreach (var entry in archive.Entries)
            {
                 if (entry.FullName.Equals(zipStandFly, StringComparison.OrdinalIgnoreCase))
                 {
                     using (StreamReader reader = new StreamReader(entry.Open()))
                     {
                         fileData = reader.ReadToEnd();
                         break;
                     }
                 }
            }

            if (fileData == string.Empty) return characterStandList;

            System.Xml.Linq.XElement root = System.Xml.Linq.XElement.Parse(fileData);

            foreach (var charElem in root.Elements("character"))
            {
                var characterData = charElem.Element("data");
                if (characterData == null) continue;

                var commonData = characterData.Elements("data").FirstOrDefault(x => (string)x.Attribute("name") == "common");
                string name = commonData?.Elements("data").FirstOrDefault(x => (string)x.Attribute("name") == "name")?.Value?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(name)) continue;

                var standInfo = new CharacterStandInfo { Name = name };

                var standListElement = charElem.Element("stand-list");
                if (standListElement != null)
                {
                    foreach (var standElem in standListElement.Elements("data").Where(x => (string)x.Attribute("name") == "stand"))
                    {
                        string standName = standElem.Elements("data").FirstOrDefault(x => (string)x.Attribute("name") == "name")?.Value?.Trim() ?? string.Empty;
                        string standImage = standElem.Elements("data").FirstOrDefault(x => (string)x.Attribute("type") == "image" && (string)x.Attribute("name") == "imageIdentifier")?.Value?.Trim() ?? string.Empty;

                        if (!string.IsNullOrEmpty(standName) && !string.IsNullOrEmpty(standImage))
                        {
                            standInfo.StandDictionary[standName] = standImage;
                        }
                    }
                }
                if (standInfo.StandDictionary.Count > 0) characterStandList.Add(standInfo);
            }
            return characterStandList;
        }

        /// <summary>
        /// 現在保持しているデータからHTML文字列を生成します
        /// </summary>
        /// <returns>生成されたHTML</returns>
        public string ConvertWrite()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(HtmlResource.HTMLHeader);

            var usedImageKeys = ChatMessageList
                .Where(x => !string.IsNullOrEmpty(x.ImageKey))
                .Select(m => m.ImageKey)
                .Distinct()
                .ToList();

            foreach (var key in usedImageKeys)
            {
                var base64 = _imageService.GetBase64ByKey(key);
                if (base64 == null) continue;
                
                sb.AppendLine(string.Format(HtmlResource.ImageHeader, key));
                sb.AppendLine(string.Format(HtmlResource.ImageData, base64));
                sb.AppendLine(HtmlResource.Imagefooter);
            }

            foreach (var item in ChatNameList)
            {
                string[] nameArray = item.Name.Select(x => x.ToString()).ToArray();
                string convertName = StringConvert16(nameArray);
                sb.AppendLine(string.Format(HtmlResource.ChatColor, convertName, COLOR_FFFFFF));
            }
            sb.AppendLine(HtmlResource.StyleEnd);

            string tmpUserName = string.Empty;
            string tmpImageKey = string.Empty;
            string tmpAreaName = string.Empty;
            bool firstFlg = true;

            foreach (var writeData in ChatMessageList)
            {
                if (writeData.IsEventImage)
                {
                    if (firstFlg) firstFlg = false;
                    else if (tmpAreaName != CONST_EVENT_AREA) sb.AppendLine(HtmlResource.DivEndLine);

                    tmpAreaName = CONST_EVENT_AREA;
                    tmpUserName = string.Empty;
                    tmpImageKey = string.Empty;

                    string base64 = _imageService.GetBase64ByKey(writeData.ImageKey);
                    if (!string.IsNullOrEmpty(base64))
                    {
                        if (writeData.IsIconEventImage)
                        {
                            sb.AppendLine(string.Format(HtmlResource.EventCharacter, base64));
                        }
                        else
                        {
                            sb.AppendLine(string.Format(HtmlResource.EventImage, base64));
                        }
                    }
                    continue;
                }

                string[] nameArray = writeData.Name.Select(x => x.ToString()).ToArray();
                string convertName = StringConvert16(nameArray);

                if (writeData.Area == HtmlResource.StringMainJP || writeData.Area == HtmlResource.StringMainEN
                    || writeData.Area == HtmlResource.StringInfoJP || writeData.Area == HtmlResource.StringInfoEN)
                {
                    if (tmpUserName != writeData.Name || tmpAreaName != writeData.Area || tmpImageKey != writeData.ImageKey)
                    {
                        if (firstFlg) firstFlg = false;
                        else if (tmpAreaName != CONST_EVENT_AREA) sb.AppendLine(HtmlResource.DivEndLine);

                        tmpUserName = writeData.Name;
                        tmpImageKey = writeData.ImageKey;
                        tmpAreaName = writeData.Area;

                        sb.AppendLine(string.Format(HtmlResource.DivChatUserMain, convertName));

                        if (!string.IsNullOrEmpty(writeData.ImageKey))
                        {
                            sb.AppendLine(string.Format(HtmlResource.DivIcon, writeData.ImageKey));
                        }
                        sb.AppendLine(HtmlResource.DivChatTextArea);
                        sb.AppendLine(string.Format(HtmlResource.DivMainChat, tmpUserName, tmpAreaName));
                    }
                    sb.AppendLine(string.Format(HtmlResource.DivChatArea, writeData.Text));
                }
                else if (writeData.IsAddedMessage)
                {
                     if (string.IsNullOrEmpty(writeData.ImageKey)) continue;

                     if (firstFlg) firstFlg = false;
                     else if (tmpAreaName != CONST_EVENT_AREA) sb.AppendLine(HtmlResource.DivEndLine);

                     tmpUserName = writeData.Name;
                     tmpAreaName = writeData.Area;

                     var base64 = _imageService.GetBase64ByKey(writeData.ImageKey);
                     if (writeData.Name == NAME_EVENT) sb.AppendLine(string.Format(HtmlResource.EventImage, base64));
                     else if (writeData.Name == NAME_EVENT_CHARACTER) sb.AppendLine(string.Format(HtmlResource.EventCharacter, base64));
                }
                else
                {
                    string areaNameCheck = writeData.Area;
                    if (areaNameCheck == HtmlResource.StringOtherEN) areaNameCheck = HtmlResource.StringOtherJP;

                    if (tmpUserName != writeData.Name || tmpAreaName != areaNameCheck)
                    {
                         if(tmpUserName != writeData.Name)
                         {
                              tmpUserName = writeData.Name;
                              tmpImageKey = string.Empty;
                              tmpAreaName = areaNameCheck;
                              
                               if (firstFlg) firstFlg = false;
                               else if (tmpAreaName != CONST_EVENT_AREA) sb.AppendLine(HtmlResource.DivEndLine);

                               if (writeData.IsSecretMessage) sb.AppendLine(string.Format(HtmlResource.DivChatUserSecret, convertName));
                               else sb.AppendLine(string.Format(HtmlResource.DivChatUserETC, convertName));

                               sb.AppendLine(HtmlResource.DivChatTextArea);
                               sb.AppendLine(string.Format(HtmlResource.DivMainChatETC, tmpUserName, tmpAreaName));
                         }
                         else if (tmpAreaName != areaNameCheck)
                         {
                              tmpUserName = writeData.Name;
                              tmpImageKey = string.Empty;
                              tmpAreaName = areaNameCheck;
                              sb.AppendLine(HtmlResource.DivEndLine);
                              sb.AppendLine(string.Format(HtmlResource.DivChatUserETC, convertName));
                              sb.AppendLine(HtmlResource.DivChatTextArea);
                              sb.AppendLine(string.Format(HtmlResource.DivMainChatETC, tmpUserName, tmpAreaName));
                         }
                    }
                    sb.AppendLine(string.Format(HtmlResource.DivChatArea, writeData.Text));
                }
            }
            sb.AppendLine(HtmlResource.DivEndLine);
            sb.AppendLine(HtmlResource.HTMLFooter);

            return sb.ToString();
        }

        /// <summary>
        /// テキストに対しルビ変換や装飾変換を適用します
        /// </summary>
        private string TextHtmlEmbellishment(string input)
        {
            return EmbellishmentConverter(RubyElementConvert(input));
        }

        /// <summary>
        /// |ベース《ルビ》 形式の文字列を ruby タグに変換します
        /// </summary>
        private string RubyElementConvert(string input)
        {
            bool hasPipe = input.Contains("|") || input.Contains("｜");
            bool hasAngleBracketsBefore = input.Contains("《") && input.Contains("》");
            bool hasAngleBracketsAfter = input.Contains("≪") && input.Contains("≫");

            if (!hasPipe || (!hasAngleBracketsBefore && !hasAngleBracketsAfter)) return input;

            var pattern = @"[\|｜](.+?)(《|≪)(.+?)(》|≫)";
            var regex = new Regex(pattern);
            int matchCount = 0;
            var result = new StringBuilder();
            int lastIndex = 0;

            foreach (Match match in regex.Matches(input))
            {
                result.Append(input.Substring(lastIndex, match.Index - lastIndex));
                string baseText = match.Groups[1].Value;
                string rubyText = match.Groups[3].Value;
                result.Append($"<ruby>{baseText}<rt>{rubyText}</rt></ruby>");
                lastIndex = match.Index + match.Length;
                matchCount++;
            }
            result.Append(input.Substring(lastIndex));
            return matchCount > 0 ? result.ToString() : input;
        }

        /// <summary>
        /// 特殊記号（~~~, ###）による装飾や改行をHTMLタグに変換し、不要な空白や改行をトリムします
        /// </summary>
        private string EmbellishmentConverter(string input)
        {
            while (true)
            {
                string oldInput = input;
                input = input.Trim('\n').Trim(' ').Trim('　');
                if (input.StartsWith("\r\n", StringComparison.OrdinalIgnoreCase)) input = input.Substring(2);
                if (input.EndsWith("\r\n", StringComparison.OrdinalIgnoreCase)) input = input.Substring(0, input.Length - 2);
                if (input.StartsWith("<br>", StringComparison.OrdinalIgnoreCase)) input = input.Substring(4);
                if (input.EndsWith("<br>", StringComparison.OrdinalIgnoreCase)) input = input.Substring(0, input.Length - 4);
                if (input == oldInput) break;
            }
            
            input = Regex.Replace(input, "~~~(.*?)~~~", "<s>$1</s>", RegexOptions.Singleline);
            input = Regex.Replace(input, "###(.*?)###", "<b>$1</b>", RegexOptions.Singleline);
            input = Regex.Replace(input, @"\r?\n", "<br>");
            input = input.Replace("\"]+\"", "\"] +\"");
            return input;
        }

        /// <summary>
        /// 名前から空白を除去します
        /// </summary>
        private string NameConverter(string input)
        {
            return input.Replace(" ", "").Replace("　", "");
        }

        /// <summary>
        /// 文字列を16進数のハッシュ風文字列に変換します。主にCSSのクラス名として利用されます。
        /// </summary>
        private string StringConvert16(string[] args)
        {
            string returnData = "name";
            foreach (string arg in args)
            {
                byte[] data = Encoding.UTF8.GetBytes(arg);
                string hexText = BitConverter.ToString(data);
                string[] hexChars = hexText.Split('-');
                foreach (var byteString in hexChars)
                {
                    returnData += byteString;
                }
            }
            return returnData;
        }
    }
}
