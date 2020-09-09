import React, { Component } from 'react';
import {
    XYPlot,
    XAxis,
    YAxis,
    LineSeries,
    VerticalGridLines,
    HorizontalGridLines,
    VerticalBarSeries,
    VerticalBarSeriesCanvas,
    LabelSeries,
    RadialChart,
    FlexibleWidthXYPlot,
} from 'react-vis';
import '../../node_modules/react-vis/dist/style.css';

export class Counter extends Component {
    displayName = Counter.name

    constructor(props) {
        super(props);

        this.state = {
            symbol: "",
            fundamentals: {},
            revenueGrowth: {},
            equityGrowth: {},
            loading: true
        };
        this.loadData = this.loadData.bind(this);
        this.handleInputSymbolChange = this.handleInputSymbolChange.bind(this);
        this.renderGraphs = this.renderGraphs.bind(this);
        this.renderGraph = this.renderGraph.bind(this);
    }

    loadData() {
        const inputSymbol = this.state.symbol;
        fetch('api/fundamentals/' + inputSymbol, {
            method: 'get'
        })
            .then(response => response.json())
            .then(data => {
                this.setState({ fundamentals: data, loading: false });
            });

        fetch('api/fundamentals/' + inputSymbol + '/growth/revenue', {
            method: 'get'
        })
            .then(response => response.json())
            .then(data => {
                console.log(data);
                this.setState({ revenueGrowth: data });
            });

        fetch('api/fundamentals/' + inputSymbol + '/growth/equity', {
            method: 'get'
        })
            .then(response => response.json())
            .then(data => {
                console.log(data);
                this.setState({ equityGrowth: data });
            });
    }

    handleInputSymbolChange(event) {
        const inputSymbol = event.target.value;
        this.setState({ symbol: inputSymbol });
    }

    static toData(statisticByYear, years) {
        return years.map(function (e, i) {
            return { x: e, y: statisticByYear[i] };
        });
    }

    renderGraph(statisticByYear, name) {
        const max = Math.max(Math.max(...statisticByYear), 0);
        const min = Math.min(Math.min(...statisticByYear), 0);

        return (
            <XYPlot yDomain={[min, max]} width={300} height={300}>
                <HorizontalGridLines style={{ stroke: '#B7E9ED' }} />
                <VerticalGridLines style={{ stroke: '#B7E9ED' }} />
                <XAxis
                    title="Year"
                    style={{
                        width: "10em",
                        line: { stroke: '#ADDDE1' },
                        ticks: { stroke: '#ADDDE1' },
                        text: { stroke: 'none', fill: '#6b6b76', fontWeight: 600 }
                    }}
                />
                <YAxis title={name}  />
                <LineSeries
                    className="first-series"
                    data={Counter.toData(statisticByYear, this.state.fundamentals.years)}
                    style={{
                        strokeLinejoin: 'round',
                        strokeWidth: 4
                    }}
                />
            </XYPlot>);
    }

    renderRevenueGraph(fundamentals) {
        const max = Math.max(Math.max(...fundamentals.revenue), Math.max(...fundamentals.netIncome), 0);
        const min = Math.min(Math.min(...fundamentals.revenue), Math.min(...fundamentals.netIncome), 0);

        return (
            <XYPlot yDomain={[min,max]} width={300} height={300}>
                <HorizontalGridLines style={{ stroke: '#B7E9ED' }} />
                <VerticalGridLines style={{ stroke: '#B7E9ED' }} />
                <XAxis
                    title="Year"
                    style={{
                        width: "10em",
                        line: { stroke: '#ADDDE1' },
                        ticks: { stroke: '#ADDDE1' },
                        text: { stroke: 'none', fill: '#6b6b76', fontWeight: 600 }
                    }}
                />
                <YAxis />
                <LineSeries
                    title="Revenue"
                    className="first-series"
                    data={Counter.toData(fundamentals.revenue, this.state.fundamentals.years)}
                    style={{
                        strokeLinejoin: 'round',
                        color: 'green',
                        strokeWidth: 4
                    }}
                />
                <LineSeries
                    title="Net incomne"
                    className="first-series"
                    data={Counter.toData(fundamentals.netIncome, this.state.fundamentals.years)}
                    style={{
                        strokeLinejoin: 'round',
                        color: 'blue',
                        strokeWidth: 4
                    }}
                />
            </XYPlot>);
    }

    renderGrowthGraph(growthModel, name) {

        if (!growthModel.growth) {
            return (<p>{name} - no data</p>)
        }

        const max = Math.max(Math.max(...growthModel.growth), 1);
        const min = Math.min(Math.min(...growthModel.growth), 0);

        console.log([min, max]);

        return (
            <XYPlot yDomain={[min, max]} width={300} height={300}>
                <HorizontalGridLines style={{ stroke: '#B7E9ED' }} />
                <VerticalGridLines style={{ stroke: '#B7E9ED' }} />
                <XAxis
                    title="Year"
                    style={{
                        width: "10em",
                        line: { stroke: '#ADDDE1' },
                        ticks: { stroke: '#ADDDE1' },
                        text: { stroke: 'none', fill: '#6b6b76', fontWeight: 600 }
                    }}
                />
                <YAxis title={name} />
                <LineSeries
                    className="first-series"
                    data={Counter.toData(growthModel.growth.slice(1), growthModel.years.slice(1))}
                    style={{
                        strokeLinejoin: 'round',
                        strokeWidth: 4
                    }}
                />
            </XYPlot>);
    }

    renderGraphs() {
        console.log(this.state.revenueGrowth.growth);

        return (
            <div className="graph-holder">
                {this.renderGraph(this.state.fundamentals.equity, 'Equity')}
                {this.renderRevenueGraph(this.state.fundamentals, 'Revenue')}
                {this.renderGraph(this.state.fundamentals.eps, 'ESP')}

                {this.renderGrowthGraph(this.state.revenueGrowth, 'Revenue growth')}
                {this.renderGrowthGraph(this.state.equityGrowth, 'Equity growth')}
            </div>);
    }

    render() {
        let contents = this.state.loading
            ? <p><em>Pick symbol</em></p>
            : this.renderGraphs();

        return (
            <div>
                <h1>Value</h1>
                <p>Here is some valuation stuff</p>
                <input type="text" value={this.state.symbol} onChange={this.handleInputSymbolChange} />
                <input type="button" value="Get" onClick={this.loadData} />
                {contents}
            </div>
        );
    }
}
