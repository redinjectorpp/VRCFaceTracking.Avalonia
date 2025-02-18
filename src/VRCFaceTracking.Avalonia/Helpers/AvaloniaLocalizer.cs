using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Platform;
using Jeek.Avalonia.Localization;

namespace VRCFaceTracking.Avalonia.Helpers
{
  public class AvaloniaLocalizer(Uri languageJsonDirectory = null) : BaseLocalizer
  {
      private readonly Uri _languageJsonDirectory = languageJsonDirectory != null
          ? languageJsonDirectory
          : new Uri("avares://VRCFaceTracking.Avalonia/Assets/Languages/");

        private Dictionary<string, string> _languageStrings = new();

        public override void Reload()
        {
          _languages.Clear();
          _languages.AddRange(AssetLoader.GetAssets(_languageJsonDirectory, null).
              Where(x => Path.GetExtension(x.LocalPath) == ".json").
              Select(x => Path.GetFileNameWithoutExtension(x.LocalPath)));

          ValidateLanguage();

          Uri uri = new Uri(_languageJsonDirectory, $"{_language}.json");
          _languageStrings = AssetLoader.Exists(uri) ?
              JsonSerializer.Deserialize<Dictionary<string, string>>(AssetLoader.Open(uri)) :
              throw new FileNotFoundException("No language file $" + uri.AbsoluteUri);
          _hasLoaded = true;

          UpdateDisplayLanguages();
        }

        protected override void OnLanguageChanged() => Reload();

        public override string Get(string key)
        {
          if (!_hasLoaded) Reload();
          return _languageStrings.TryGetValue(key, out var str) ?
              str.Replace("\\n", "\n") :
              Language + ":" + key;
        }
    }
}
