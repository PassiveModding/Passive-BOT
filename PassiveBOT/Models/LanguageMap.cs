namespace PassiveBOT.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The language map.
    /// </summary>
    public class LanguageMap
    {
        /// <summary>
        /// The language code list.
        /// </summary>
        public enum LanguageCode
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

        /// <summary>
        /// Gets or sets the default map.
        /// </summary>
        public static List<GuildModel.GuildSetup.TranslateSetup.TranslationSet> DefaultMap { get; set; } = new List<GuildModel.GuildSetup.TranslateSetup.TranslationSet>
        {
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇦🇺",
                    "🇺🇸",
                    "🇪🇺",
                    "🇳🇿"
                },
                Language = LanguageCode.en
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇭🇺"
                },
                Language = LanguageCode.hu
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇫🇷"
                },
                Language = LanguageCode.fr
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇫🇮"
                },
                Language = LanguageCode.fi
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇲🇽",
                    "🇪🇸",
                    "🇨🇴",
                    "🇦🇷"
                },
                Language = LanguageCode.es
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇧🇷",
                    "🇵🇹",
                    "🇲🇿",
                    "🇦🇴"
                },
                Language = LanguageCode.pt
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇩🇪",
                    "🇦🇹",
                    "🇨🇭",
                    "🇧🇪",
                    "🇱🇺",
                    "🇱🇮"
                },
                Language = LanguageCode.de
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇮🇹",
                    "🇨🇭",
                    "🇸🇲",
                    "🇻🇦"
                },
                Language = LanguageCode.it
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇨🇳",
                    "🇸🇬",
                    "🇹🇼"
                },
                Language = LanguageCode.zh_CN
            },
            new GuildModel.GuildSetup.TranslateSetup.TranslationSet
            {
                EmoteMatches = new List<string>
                {
                    "🇯🇵"
                },
                Language = LanguageCode.ja
            }
        };
    }
}