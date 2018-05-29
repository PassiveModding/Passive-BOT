using System;
using System.Collections.Generic;
using System.Text;

namespace PassiveBOT.Models
{
    public class LanguageMap
    {
        public static List<GuildModel.gsettings.translate.TObject> Map = new List<GuildModel.gsettings.translate.TObject>
        {
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇦🇺",
                "🇺🇸",
                "🇪🇺",
                "🇳🇿"
            }, Language = languagecode.en},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇭🇺"
            }, Language = languagecode.hu},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
            "🇫🇷"
            }, Language = languagecode.fr},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇫🇮"
            }, Language = languagecode.fi},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇲🇽",
                "🇪🇸",
                "🇨🇴",
                "🇦🇷"
            }, Language = languagecode.es},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇧🇷",
                "🇵🇹",
                "🇲🇿",
                "🇦🇴"
            }, Language = languagecode.pt},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇩🇪",
                "🇦🇹",
                "🇨🇭",
                "🇧🇪",
                "🇱🇺",
                "🇱🇮"
            }, Language = languagecode.de},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇮🇹",
                "🇨🇭",
                "🇸🇲",
                "🇻🇦"
            }, Language = languagecode.it},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇨🇳",
                "🇸🇬",
                "🇹🇼"
            }, Language = languagecode.zh_CN},
            new GuildModel.gsettings.translate.TObject{EmoteMatches = new List<string>
            {
                "🇯🇵"
            }, Language = languagecode.ja},
        };

        public enum languagecode
        {
            af,
            sq,
            am,
            ar,
            hy,
            az,
            eu,
            be,
            bn,
            bs,
            bg,
            ca,
            ceb,
            zh_CN,
            zh_TW,
            co,
            hr,
            cs,
            da,
            nl,
            en,
            eo,
            et,
            fi,
            fr,
            fy,
            gl,
            ka,
            de,
            el,
            gu,
            ht,
            ha,
            haw,
            iw,
            hi,
            hmn,
            hu,
            _is,
            ig,
            id,
            ga,
            it,
            ja,
            jw,
            kn,
            kk,
            km,
            ko,
            ku,
            ky,
            lo,
            la,
            lv,
            lt,
            lb,
            mk,
            mg,
            ms,
            ml,
            mt,
            mi,
            mr,
            mn,
            my,
            ne,
            no,
            ny,
            ps,
            fa,
            pl,
            pt,
            pa,
            ro,
            ru,
            sm,
            gd,
            sr,
            st,
            sn,
            sd,
            si,
            sk,
            so,
            es,
            su,
            sw,
            sv,
            tl,
            tg,
            ta,
            te,
            th,
            tr,
            uk,
            ur,
            uz,
            vi,
            cy,
            xh,
            yi,
            yo,
            zu
        }
    }
}
