const Web3 = require('web3');
const abi = require("./abi.json");
const TX = require("ethereumjs-tx").Transaction

const rpcURL = 'wss://solitary-snowy-river.bsc.quiknode.pro/16b4e8d1466a4e5c06c88145a2faed83b3661fd9/';
const web3 = new Web3(rpcURL);
var contractAddress = "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56";
const contract = new web3.eth.Contract(abi, contractAddress);

const privateKey = "ca1b328f4db2ef55d82cf84b53fd501dda732ea181f65b6e265a444526dbb2a0"
const to_address = "0x30d457334E67B23f7d013fbeB88Cf5CC577FE44B"
const from_address = "0xFbbBAe5753f525963d4465ef5Dc57E502655d88A"
const amount = web3.utils.toWei(`${1}`)

async function transfer() {
    let nonce = await web3.eth.getTransactionCount(from_address)
    var transaction = {
        "to": to_address,
        "value": amount,
        "gas": 21000,
        "gasPrice": 20000000000,
        "nonce": nonce
    }
    let txn = await web3.eth.accounts.signTransaction(transaction, privateKey)
    web3.eth.sendSignedTransaction(txn.rawTransaction).on('transactionHash', function (hash) {
        console.log("发送成功，获取交易hash：", hash)
    }).on('receipt', function (receipt) {
        console.log("链上结果返回，返回数据：", receipt)
    }).on('confirmation', function (confirmationNumber, receipt) {
        console.log("链上confirmation结果返回，确认数：", confirmationNumber)
        console.log("链上confirmation结果返回，返回数据：", receipt)
    }).on('error', console.error);
}

async function contract_transfer() {
    let balance = await contract.methods.balanceOf(from_address).call()
    console.log(`合约金额：${balance}`)
    let data = await contract.methods.transfer(to_address, amount).encodeABI()
    let nonce = await web3.eth.getTransactionCount(from_address)
    let gasPrice = await web3.eth.getGasPrice()
    var transaction = {
        nonce: nonce,
        gasPrice: gasPrice,
        to: contractAddress,
        from: from_address,
        data: data
    }
    let gas = await web3.eth.estimateGas(transaction)
    transaction.gas = gas

    let txn = await web3.eth.accounts.signTransaction(transaction, privateKey)
    await web3.eth.sendSignedTransaction(txn.rawTransaction)
        .on('transactionHash', function (hash) {
            console.log("发送成功，获取交易hash：", hash)
        }).on('receipt', function (receipt) {
            console.log("链上结果返回，返回数据：", receipt)
        }).on('confirmation', function (confirmationNumber, receipt) {
            console.log("链上confirmation结果返回，确认数：", confirmationNumber)
            console.log("链上confirmation结果返回，返回数据：", receipt)
        }).on('error', console.error);

}