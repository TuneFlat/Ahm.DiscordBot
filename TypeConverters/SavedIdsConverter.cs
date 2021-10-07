using Ahm.DiscordBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using System.Text;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.TypeConverters
{
    public class SavedIdsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var sameObj = objectType == typeof(SavedIdsModel);
            return (objectType == typeof(SavedIdsModel));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            SavedIdsModel savedIdsModel = new SavedIdsModel();

            savedIdsModel = (SavedIdsModel)DeserializeType(obj, serializer);
            //savedIdsModel.lfgRoleProperties.LFGRoleMessageId = ulong.Parse(obj["LFGRoleMessageId"].ToString());
            //savedIdsModel.lfgRoleProperties.LFGEmojis = (Emoji)obj["LFGEmojis"];

            return savedIdsModel;
        }
        // https://stackoverflow.com/questions/36969500/json-net-custom-jsonconverter-with-data-types
        private object DeserializeType(JObject obj, JsonSerializer serializer)
        {
            var properties = obj.Properties();//.Where(p => p.Name.StartsWith("data.")).First();
            Dictionary<Emoji, ulong> lfgEmojis = new Dictionary<Emoji, ulong>();
            Dictionary<Emoji, ulong> pronounEmojis = new Dictionary<Emoji, ulong>();
            ulong lfgMessageId = ulong.MinValue;
            ulong pronounMessageId = ulong.MinValue;
            foreach (JProperty prop in properties)
            {
                JObject child = (JObject)prop.Value;                               
                switch (prop.Name)
                {
                    case "LFGRoleProperties":
                    case "LFGEmojis":
                        var lfgEmojiTokens = child["LFGEmojis"].Children();
                        foreach (var jo in lfgEmojiTokens)
                        {
                            var unicode = (JProperty)jo;
                            var emoji = new Emoji(unicode.Name);
                            lfgEmojis.Add(emoji, ulong.Parse(unicode.Value.ToString()));
                        }
                        lfgMessageId = ulong.Parse(child["LFGRoleMessageId"].ToString());
                        break;
                    case "LFGRoleMessageId":
                        lfgMessageId = child.ToObject<ulong>(serializer);
                        break;

                    case "PronounRoleProperties":
                    case "PronounEmojis":
                        var pronounEmojiTokens = child["PronounEmojis"].Children();
                        foreach (var jo in pronounEmojiTokens)
                        {
                            var unicode = (JProperty)jo;
                            var emoji = new Emoji(unicode.Name);
                            pronounEmojis.Add(emoji, ulong.Parse(unicode.Value.ToString()));
                        }
                        pronounMessageId = ulong.Parse(child["PronounRoleMessageId"].ToString());
                        break;
                    case "PronounRoleMessageId":
                        pronounMessageId = child.ToObject<ulong>(serializer);
                        break;
                    default: 
                        throw new JsonSerializationException("Unrecognized type: " + prop.Name);

                }
            }
            var lfgProperties = new LFGRoleProperties { 
                LFGEmojis = lfgEmojis,
                LFGRoleMessageId = lfgMessageId
            };
            var pronounProperties = new PronounRoleProperties
            {
                PronounEmojis = pronounEmojis,
                PronounRoleMessageId = pronounMessageId
            };
            return new SavedIdsModel {  lfgRoleProperties = lfgProperties, pronounRoleProperties = pronounProperties };
        }

        // TODO: Test Default Write with Emojis
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

       
    }
}
