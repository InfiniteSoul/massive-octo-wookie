﻿using Azuria.Api.Enums.Info;

namespace Azuria.Api.Helpers.Extensions
{
    internal static class LanguageExtensions
    {
        #region Methods

        internal static string ToShortString(this Language language)
        {
            switch (language)
            {
                case Language.English:
                    return "en";
                case Language.German:
                    return "de";
                default:
                    return string.Empty;
            }
        }

        #endregion
    }
}