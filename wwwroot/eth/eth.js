const Web3 = require('web3');
const abi = require("./abi.json");

const network = 'wss://bsc-ws-node.nariox.org';
let _provider = new Web3.providers.WebsocketProvider(url);
const web3 = new Web3(_provider);
var contractAddress = "0xa1b6c05E943355ec581bd3f08302f62aE4871184";
var topics = ['0xc6976c700a2c7401b2081a99cd2d214616445c37d92f421292fdd362beec6ac2']

const contract = new web3.eth.Contract(abi, contractAddress);