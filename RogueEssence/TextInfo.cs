namespace RogueEssence
{
	public static class TextInfo
	{
		public static string[] SUPPORTED_LANGUAGES = {"en","fr","de","es","it","pt","ko","ja","ja-jp","zh-hans","zh-hant"};
		public static string ToName(this string lang)
		{
			switch (lang)
			{
				case"en": return "English";
				case"fr": return "Français";
				case"de": return "Deutsch";
				case"es": return "Español";
                case "it": return "Italiano";
                case "pt": return "Português";
				case"ko": return "한국어";
				case"ja": return "にほんご";
				case"ja-jp": return "日本語 (漢字)";
				case"zh-hans": return "简体中文";
				case"zh-hant": return "繁體中文";
				default: return "";
			}
		}

	}
}
