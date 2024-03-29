﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using StockViewer.Fio;
using StockViewer.Fio.Trading;
using StockViewer.Statistics;
using StockViewer.Statistics.Data;

namespace StockView.Api.Controllers
{
    [Route("api/[controller]")]
    public class DashboardController : Controller
    {
        private readonly Func<FioClient> clientFactory;

        public DashboardController(Func<FioClient> clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        [HttpPost]
        [Route("stats")]
        public async Task<ActionResult<CurrencyInvestmentStatistic>> GetStatisticsForCurrencyAsync([FromForm]string login, [FromForm]string password)
        {
            using (var fioClient = clientFactory())
            {
                var tradingItemProvider = new TradingItemProvider(fioClient);
                var tradingStatisticsProvider = new TradingStatisticsProvider();

                try
                {
                    await fioClient.LoginAsync(login, password);

                    //not to get banned
                    await Task.Delay(100);
                    var portfolioData = await fioClient.GetPortfolioDataAsync();

                    //not to get banned
                    await Task.Delay(100);
                    var tradingItems = await tradingItemProvider.GetAllItemsAsync();

                    //not to get banned
                    await Task.Delay(100);
                    var monetaryData = await fioClient.GetMonetaryDataAsync();

                    var currencyBalanceSheets = tradingStatisticsProvider.GetInvestmentsByCurrency(portfolioData, tradingItems, monetaryData)
                        .Select(s => new PortfolioDashBoardDto
                        {
                            Statistic = s,
                            Balance = tradingStatisticsProvider.ComputeInvestmentBalance(s),
                            Total = tradingStatisticsProvider.ComputeTotalInvestment(s)
                        });

                    return Ok(currencyBalanceSheets);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex);
                }
            }
        }
    }
}