﻿using Jint;

namespace Azuria.Utilities.Web
{
    internal class JsEval
    {
        #region

        internal static string Eval(string input)
        {
            return new Engine().Execute(input).GetCompletionValue().AsNumber().ToString();
        }

        #endregion
    }
}