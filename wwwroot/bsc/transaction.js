const Web3 = require('web3');
const abi = require("./abi.json");

const rpcURL = 'wss://solitary-snowy-river.bsc.quiknode.pro/16b4e8d1466a4e5c06c88145a2faed83b3661fd9/';
const web3 = new Web3(rpcURL);
var contractAddress = "0xe9e7CEA3DedcA5984780Bafc599bD69ADd087D56";
const contract = new web3.eth.Contract(abi, contractAddress);
const txId='0xf4f4d09d598a101ec408f19184bd98d22ed7391999aa4f9cba132cbac5089e18'
async function getTransactionInfo(){
    //var receipt = web3.eth.getTransactionReceipt(txId).then(console.log);
    var receipt = web3.eth.getBlock(20235717).then(console.log);
}
getTransactionInfo()