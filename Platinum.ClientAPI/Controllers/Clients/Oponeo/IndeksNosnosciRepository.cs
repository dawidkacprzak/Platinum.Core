namespace Platinum.ClientAPI.Controllers.Clients.Oponeo
{
    public static class IndeksNosnosciRepository
    {
        public static string GetMaxIndeksNosnosci(string indeksNosnosci)
        {
            if (string.IsNullOrEmpty(indeksNosnosci) || indeksNosnosci.Equals("nullvalue"))
            {
                return "nullvalue";
            }

            string trimmedVal = indeksNosnosci.Trim();
            switch (trimmedVal)
            {
                case "60": return "250";
                case "61": return "257";
                case "62": return "265";
                case "63": return "272";
                case "64": return "280";
                case "65": return "290";
                case "66": return "300";
                case "67": return "307";
                case "68": return "315";
                case "69": return "325";
                case "70": return "335";
                case "71": return "345";
                case "72": return "355";
                case "73": return "365";
                case "74": return "375";
                case "75": return "387";
                case "76": return "400";
                case "77": return "412";
                case "78": return "425";
                case "79": return "437";
                case "80": return "450";
                case "81": return "462";
                case "82": return "475";
                case "83": return "487";
                case "84": return "500";
                case "85": return "515";
                case "86": return "530";
                case "87": return "545";
                case "88": return "560";
                case "89": return "580";
                case "90": return "600";
                case "91": return "615";
                case "92": return "630";
                case "93": return "650";
                case "94": return "670";
                case "95": return "690";
                case "96": return "710";
                case "97": return "730";
                case "98": return "750";
                case "99": return "775";
                case "100": return "800";
                case "101": return "825";
                case "102": return "850";
                case "103": return "875";
                case "104": return "900";
                case "105": return "925";
                case "106": return "950";
                case "107": return "975";
                case "108": return "1000";
                case "109": return "1030";
                case "110": return "1060";
                case "111": return "1090";
                case "112": return "1120";
                case "113": return "1150";
                case "114": return "1180";
                case "115": return "1215";
                case "116": return "1250";
                case "117": return "1285";
                case "118": return "1320";
                case "119": return "1360";
                case "120": return "1400";
                case "121": return "1450";
                case "122": return "1500";
                case "123": return "1550";
                case "124": return "1600";
                case "125": return "1650";
                case "126": return "1700";
                case "127": return "1750";
                case "128": return "1800";
                case "129": return "1850";
                case "130": return "1900";
                case "131": return "1950";
                case "132": return "2000";
                case "133": return "2060";
                case "134": return "2120";
                case "135": return "2180";
                case "136": return "2180";
                case "137": return "2240";
                case "138": return "2360";
                case "139": return "2430";
                case "140": return "2500";
                case "141": return "2575";
                case "142": return "2650";
                case "143": return "2725";
                case "144": return "2800";
                case "145": return "2900";
                case "146": return "3000";
                case "147": return "3075";
                case "148": return "3159";
                case "149": return "3250";
                case "150": return "3350";
                case "151": return "3450";
                case "152": return "3550";
                case "153": return "3650";
                case "154": return "3750";
                case "155": return "3875";
                case "156": return "4000";
                case "157": return "4125";
                case "158": return "4250";
                case "159": return "4375";
                case "160": return "4500";
                case "161": return "4625";
                case "162": return "4750";
                case "163": return "4857";
                case "164": return "5000";
                case "165": return "5150";
                case "166": return "5300";
                case "167": return "5450";
                case "168": return "5600";
                case "169": return "5800";
                case "170": return "6000";

                default: return "nullvalue";
            }
        }
    }
}