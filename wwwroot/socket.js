const TronWeb = require('tronweb');
const axios = require('axios')
const FormData = require('form-data')

const rpcURL = 'https://api.trongrid.io';
const tronWeb = new TronWeb(rpcURL,rpcURL,rpcURL,'');
async function main() {
    const trc20ContractAddress = "TR7NHqjeKQxGTCi8q8ZY4pL8otSzgjLj6t"; //mainnet USDT contract
    let contract = await tronWeb.contract().at(trc20ContractAddress);
    await contract && contract.Transfer().watch((err, event) => {
        if (err) {
            //写入日志 
            console.log(err);
        } else {
            console.log(event.block)
        }
    });
}
main()