const Web3 = require('web3');
const abi = require("./abi.json");

const rpcURL = 'wss://solitary-snowy-river.bsc.quiknode.pro/16b4e8d1466a4e5c06c88145a2faed83b3661fd9/';
const web3 = new Web3(rpcURL);
var contractAddress = "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56";
const contract = new web3.eth.Contract(abi, contractAddress);

setInterval(function () {
    web3.eth.getBlockNumber().then((block) => {
        console.log(block)
    });
}, 1000);

contract.events.Transfer({})
    .on("data", function (event) {
        console.log(event.blockNumber)
    })
    .on("connected", function (subscriptionId) {
        console.log(subscriptionId);
    })
    .on("error", function (error) {
        console.log(error)
    })
    .on("changed", function (event) {
        console.log(event)
    })