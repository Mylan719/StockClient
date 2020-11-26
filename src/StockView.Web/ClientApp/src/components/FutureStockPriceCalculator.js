import React, { Component } from 'react';
import Numeral from 'numeral';


export class FutureStockPriceCalculator extends Component {
    constructor(props) {
        super(props);

        this.calculateBasePrice = this.calculateBasePrice.bind(this);
        this.newStockCountChange = this.newStockCountChange.bind(this);
        this.newStockPriceChange = this.newStockPriceChange.bind(this);
        this.symbolChange = this.symbolChange.bind(this);

        this.state = {
            symbols: props.symbols,
            symbol: "",
            newStockCount: 0,
            newStockPrice: 0,
            newBasePrice: 0,
            tradeAmount: 0
        };
    }

    newStockCountChange(event) {
        const val = event.target.value;
        this.calculateBasePrice(this.state.symbol, val, this.state.newStockPrice);
    }

    symbolChange(event) {
        const val = event.target.value;
        this.calculateBasePrice(val, this.state.newStockCount, this.state.newStockPrice);
    }

    newStockPriceChange(event) {
        const val = event.target.value;
        this.calculateBasePrice(this.state.symbol, this.state.newStockCount, val);
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

        const symbol = this.state.symbols.filter(function (s) { return s.name == searchedSymbol; })[0];

        console.log(this.state.symbols);

        if (!symbol) {
            this.setState({
                newStockCount: newStockCountVal,
                symbol: searchedSymbol,
                newStockPrice: newStockPriceVal
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
            newStockCount: newStockCountVal,
            symbol: searchedSymbol,
            shareCount: shareCount,
            newStockPrice: currentPrice,
            newBasePrice: totalPriceAfter / totalStocksAfter,
            tradeAmount: currentPrice * newStockCountVal
        });
    }

    render() {
        return (
            <div>
                <h3>Future stock price calculator</h3>
                <ul>
                    <li>Symbol: <input type="text" value={this.state.symbol} onChange={this.symbolChange} /></li>
                    <li>Number of shares to buy: <input type="number" value={this.state.newStockCount} onChange={this.newStockCountChange} /></li>
                    <li>I own: {Numeral(this.state.shareCount).format("0,0.00")}</li>
                    <li>Current price: <input type="text" value={this.state.newStockPrice} onChange={this.newStockPriceChange} /></li>
                    <li>New base price: {Numeral(this.state.newBasePrice).format("0,0.00")}</li>
                    <li>Trade total price: {Numeral(this.state.tradeAmount).format("0,0.00")}</li>

                </ul>
            </div>);
    }
}
