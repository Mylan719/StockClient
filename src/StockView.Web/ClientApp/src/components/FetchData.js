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
import { FutureStockPriceCalculator } from './FutureStockPriceCalculator.js'


export class FetchData extends Component {
    displayName = FetchData.name

    constructor(props) {
        super(props);

        this.renderForecastsTable = this.renderForecastsTable.bind(this);
        this.renderContent = this.renderContent.bind(this);

        this.state = {
            investmentStatistics: [],
            loading: true
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

    renderContent() {
        if (this.state.loading) {
            return (<p><em>Loading...</em></p>);
        }

        const symbols = this.state.investmentStatistics.flatMap(is => is.statistic.symbols);

        return (
            <div>
                <FutureStockPriceCalculator symbols={symbols} />
                {this.state.investmentStatistics.map(cs => this.renderForecastsTable(cs))}
            </div>
        );
    }

    render() {
        return (
            <div>
                <h1>Weather forecast</h1>
                <p>Welth data here</p>
                {this.renderContent()}
            </div>
        );
    }
}
