﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Azuria.Main;
using Azuria.Main.User.ControlPanel;
using Azuria.Test.Attributes;
using Azuria.Utilities.ErrorHandling;
using NUnit.Framework;

namespace Azuria.Test
{
    [TestFixture, LoginRequired]
    public class UcpTest
    {
        private UserControlPanel _controlPanel;

        [Test, Order(1)]
        public async Task AnimeTest()
        {
            this._controlPanel = new UserControlPanel(SenpaiTest.Senpai);

            ProxerResult<IEnumerable<AnimeMangaUcpObject<Anime>>> lFetchAnimeResult =
                await this._controlPanel.Anime.GetObject();
            Assert.IsTrue(lFetchAnimeResult.Success);
            Assert.IsNotNull(lFetchAnimeResult.Result);
            Assert.IsNotEmpty(lFetchAnimeResult.Result);
        }
    }
}