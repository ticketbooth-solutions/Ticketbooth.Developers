'use strict';
import { Wallets } from './storage.js';

function toTokenString(stratoshis) {
    let tokenString = stratoshis.toString();
    if (tokenString.length >= 9) {
        const insertionIndex = tokenString.length - 8;
        tokenString = tokenString.slice('0', insertionIndex).concat('.').concat(tokenString.slice(insertionIndex));
    } else {
        tokenString = '0.'.concat(tokenString.padStart(8, '0'));
    }

    return tokenString;
}

document.addEventListener('DOMContentLoaded', function () {
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('/demoHub')
        .build();

    connection.on('consensusUpdated', async function () {
        for (const wallet of await Wallets.all()) {
            var addresses = wallet.addresses.map(function (addressItem) {
                return addressItem.address;
            });

            const addressBalances = await connection.invoke('UpdateBalances', addresses);
            for (const addressBalance of addressBalances) {
                await Wallets.updateBalance(wallet.name, addressBalance.address, toTokenString(addressBalance.balance));
            }
        }
    });

    connection.start()
        .then(function () {
            // register event listeners
        })
        .catch(error => {
            console.error(error.message);
        });
});