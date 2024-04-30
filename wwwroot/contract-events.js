const Web3 = require('web3');
const abi = require("./abi.json");

const rpcURL = 'https://solitary-snowy-river.bsc.quiknode.pro/16b4e8d1466a4e5c06c88145a2faed83b3661fd9/';
let _provider = new Web3.providers.WebsocketProvider(rpcURL);
const web3 = new Web3(rpcURL);
var address="0xFbbBAe5753f525963d4465ef5Dc57E502655d88A"
var contractAddress = "0xa1b6c05E943355ec581bd3f08302f62aE4871184";
var topics = ['0xc6976c700a2c7401b2081a99cd2d214616445c37d92f421292fdd362beec6ac2']



const contract = new web3.eth.Contract(abi, contractAddress);

setInterval(function () {
    web3.eth.getBlockNumber().then((bn) => {
        console.log(bn)
    });
}, 1000);

// contract.events.LendingAdded({})
//     .on("data", function (data) {
//         console.log(data)
//     })
//     .on("connected", function (subscriptionId) {
//         console.log(subscriptionId);
//     })
//     .on("error", function (error) {
//         console.log(error)
//     })
//     .on("changed", function (event) {
//         console.log(event)
//     })

