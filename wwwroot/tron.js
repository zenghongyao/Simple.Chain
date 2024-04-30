const TronWeb = require('tronweb');
const setting = require("./setting.json")

async function transfer(privateKey, contract_address, to_address, amount) {
    const tronWeb = new TronWeb(setting.tron.mainnet, setting.tron.mainnet, setting.tron.mainnet, privateKey);
    const parameter = [{
        type: 'address',
        value: to_address
    }, {
        type: 'uint256',
        value: amount
    }];

    const issuerAddress = tronWeb.address.fromPrivateKey(privateKey)
    const transaction = await tronWeb.transactionBuilder.triggerSmartContract(contract_address, "transfer(address,uint256)", {}, parameter, issuerAddress);
    //签名
    const signedtxn = await tronWeb.trx.sign(transaction, privateKey);
    //广播
    const receipt = await tronWeb.trx.sendRawTransaction(
        signedtxn
    ).then(output => {
        console.log('- Output:', output, '\n');
        return output;
    });
    console.log(receipt)
}

transfer("c0c265f34225fc220b729957502d9a3803a344ab62da9ecf7dc7b21c795cec3c", setting.tron.contract_address, setting.tron.collect_address, 2)