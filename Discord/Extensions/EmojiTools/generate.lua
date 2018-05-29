local json = require("cjson")

local template, item, items = [[
using System.Collections.Generic;

namespace Discord.Addons.EmojiTools
{
    public static class EmojiMap
    {
        public static IReadOnlyDictionary<string, string> Map = new Dictionary<string, string>
        {
%s
        };
    }
}
]], "           [%q] = %q", {}

local emojis do
    local f = assert(io.open("emojis.json", "r"))
    local text = f:read("*a")
    f:close()
    emojis = json.decode(text)
end

for section, groups in pairs(emojis) do
    for _, group in ipairs(groups) do
        for _, name in ipairs(group.names) do
            items[#items+1] = item:format(name, group.surrogates)
        end
    end
end

do
    local code = template:format(table.concat(items,",\n"))
    local f = assert(io.open("src/Discord.Addons.EmojiTools/EmojiMap.cs", "w"))
    f:write(code)
    f:close()
end