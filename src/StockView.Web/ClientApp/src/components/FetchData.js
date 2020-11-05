import React, { Component } from 'react';
import Numeral from 'numeral';
import 'numeral/locales/cs';
import {
    XYPlot,
    XAxis,
    YAxis,
    VerticalGridLines,
    HorizontalGridLines,
    VerticalBarSeries,
    VerticalBarSeriesCanvas,
    LabelSeries,
    RadialChart,
    FlexibleWidthXYPlot
} from 'react-vis';
import { Credentials } from './config.js'


export class FetchData extends Component {
    displayName = FetchData.name

    constructor(props) {
        super(props);

        this.calculateBasePrice = this.calculateBasePrice.bind(this);
        this.newStockCountChange = this.newStockCountChange.bind(this);
        this.newStockPriceChange = this.newStockPriceChange.bind(this);
        this.symbolChange = this.symbolChange.bind(this);

        this.renderForecastsTable = this.renderForecastsTable.bind(this);

        this.state = {
            investmentStatistics: [],
            loading: true,
            calculator: {
                symbol: "",
                newStockCount: 0,
                newStockPrice: 0,
                newBasePrice: 0
            }
        };

        var formData = FetchData.toFormData(
            {
                login: Credentials.login,
                password: Credentials.password
            });

        fetch('api/dashboard/stats', {
            method: 'post',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                this.setState({ investmentStatistics: data, loading: false });
            });
    }

    newStockCountChange(event) {
        const val = event.target.value;
        this.calculateBasePrice(this.state.calculator.symbol, val, this.state.calculator.newStockPrice);
    }

    symbolChange(event) {
        const val = event.target.value;
        this.calculateBasePrice(val, this.state.calculator.newStockCount, this.state.calculator.newStockPrice);
    }

    newStockPriceChange(event) {
        const val = event.target.value;
        this.calculateBasePrice(this.state.calculator.symbol, this.state.calculator.newStockCount, val);
    }

    calculateBasePrice(searchedSymbol, newStockCountVal, newStockPriceVal) {
        if (searchedSymbol == "") {
            this.setState({
                calculator: {
                    newStockCount: newStockCountVal,
                    symbol: searchedSymbol,
                    newStockPrice: newStockPriceVal
                }
            });
            return;
        }

        console.log(this.state.investmentStatistics);

        const allSymbol = this.state.investmentStatistics.flatMap(is => is.statistic.symbols);

        console.log(allSymbol);

        const symbol = allSymbol.filter(function (s) { return s.name == searchedSymbol; })[0];

        console.log(symbol);

        if (!symbol) {
            this.setState({
                calculator: {
                    newStockCount: newStockCountVal,
                    symbol: searchedSymbol,
                    newStockPrice: newStockPriceVal
                }
            });
            return;
        }

        var stockPrice = Number(newStockPriceVal);

        const newStockCount = Number(newStockCountVal);

        const shareCount = Math.ceil(symbol.investedNow / symbol.basePrice);

        const totalStocksAfter = newStockCount + shareCount;

        const currentPrice = stockPrice == 0
            ? (symbol.investedNowPrice / shareCount)
            : stockPrice;

        const totalPriceAfter = symbol.investedNow + newStockCount * currentPrice;

        console.log(totalPriceAfter);
        console.log(totalStocksAfter);

        this.setState({
            calculator: {
                newStockCount: newStockCountVal,
                symbol: searchedSymbol,
                shareCount: shareCount,
                newStockPrice: currentPrice,
                newBasePrice: totalPriceAfter / totalStocksAfter,
            }
        });
    }

    static toFormData(object) {
        var formBody = [];
        for (var property in object) {
            var encodedKey = encodeURIComponent(property);
            var encodedValue = encodeURIComponent(object[property]);
            formBody.push(encodedKey + "=" + encodedValue);
        }
        return formBody.join("&");
    }


    renderForecastsTable(portfolio) {

        var investmentStatistic = portfolio.statistic;
        var total = portfolio.total;
        var symbolRatios = portfolio.balance.symbols.map(s => {
            return {
                name: s.name,
                angle: s.percentage,
                color: s.riskColor
            };
        });


        const graphInvestedData = investmentStatistic.symbols.map(s => {
            return {
                x: s.name,
                y: s.investedNow
            };
        });

        const graphInvestedPriceData = investmentStatistic.symbols.map(s => {
            return {
                x: s.name,
                y: s.investedNowPrice
            };
        });

        const labelData = graphInvestedData.map((d, idx) => ({
            x: d.x,
            y: Math.max(graphInvestedData[idx].y, graphInvestedPriceData[idx].y)
        }));

        return (
            <div>
                <table className='table'>
                    <thead>
                        <tr>
                            <th>Symbol</th>
                            <th>Invested</th>
                            <th>Investment price now</th>
                            <th>Base price</th>
                            <th>Realized gain</th>
                            <th>Gain</th>
                            <th>Gain %</th>
                        </tr>
                    </thead>
                    <tbody>
                        {investmentStatistic.symbols.map(symbol => {
                            var gainsCss = symbol.gain < 0
                                ? 'fin-bad'
                                : 'fin-good';

                            Numeral.locale('cs');


                            return (<tr key={symbol.name}>
                                <td>{symbol.name}</td>
                                <td>{symbol.investedNow}</td>
                                <td>{symbol.investedNowPrice}</td>
                                <td>{Numeral(symbol.basePrice).format("0,0.00")}</td>
                                <td>{symbol.realizedGains}</td>
                                <td className={gainsCss}>{Numeral(symbol.gain).format("0,0.00")}</td>
                                <td className={gainsCss}>{Numeral(symbol.gainPercentage).format("0,0.00")}%</td>
                            </tr>);
                        })}
                        <tr>
                            <th>Total:</th>
                            <th>{Numeral(total.investedNow).format("0,0.00")}</th>
                            <th>{Numeral(total.investedNowPrice).format("0,0.00")}</th>
                            <th />
                            <th>{Numeral(total.realizedGains).format("0,0.00")}</th>
                            <th className={total.gain < 0 ? 'fin-bad' : 'fin-good'}>{Numeral(total.gain).format("0,0.00")}</th>
                            <th className={total.gainPercentage < 0 ? 'fin-bad' : 'fin-good'}>{Numeral(total.gainPercentage).format("0,0.00")}%</th>
                        </tr>
                        <tr>
                            <th>Cash: </th>
                            <th />
                            <th>{Numeral(investmentStatistic.cash).format("0,0.00")}</th>
                            <th colSpan={3} />
                        </tr>
                    </tbody>
                </table>
                <ul>
                    <li>Symmbol: <input type="text" value={this.state.calculator.symbol} onChange={this.symbolChange} /></li>
                    <li>New stocks: <input type="number" value={this.state.calculator.newStockCount} onChange={this.newStockCountChange} /></li>
                    <li>Owned share count: {Numeral(this.state.calculator.shareCount).format("0,0.00")}</li>
                    <li>Current price: <input type="text" value={this.state.calculator.newStockPrice} onChange={this.newStockPriceChange} /></li>
                    <li>New base price: {Numeral(this.state.calculator.newBasePrice).format("0,0.00")}</li>
                </ul>

                <XYPlot xType="ordinal" width={600} height={400} xDistance={100}>
                    <VerticalGridLines />
                    <HorizontalGridLines />
                    <XAxis />
                    <YAxis />
                    <VerticalBarSeries data={graphInvestedData} />
                    <VerticalBarSeries data={graphInvestedPriceData} />
                    <LabelSeries data={labelData} getLabel={d => d.x} />
                </XYPlot>
                <RadialChart
                    colorType={'literal'}
                    colorDomain={[0, 100]}
                    colorRange={[0, 10]}
                    margin={{ top: 100 }}
                    getLabel={d => d.name}
                    data={symbolRatios}
                    labelsRadiusMultiplier={1.1}
                    labelsStyle={{ fontSize: 16, fill: '#222' }}
                    showLabels
                    style={{ stroke: '#fff', strokeWidth: 2 }}
                    width={400}
                    height={300}
                />
            </div>
        );
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Loading...</em></p>
            : this.state.investmentStatistics.map(cs => this.renderForecastsTable(cs));

        return (
            <div>
                <h1>Weather forecast</h1>
                <p>Welth data here</p>
                {contents}
            </div>
        );
    }
}
