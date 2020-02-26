'use strict';
import { Wallets } from './storage.js';

function isHexString(value) {
    const regex = /^[0-9A-Fa-f]+$/g;
    return regex.test(value);
}

function onSearchResult() {
    const spinner = document.getElementById('trx-spinner');
    const responseItem = document.getElementById('trx-response');
    spinner.style.display = 'none';
    responseItem.style.display = 'block';
}

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

    async function startDemoHub() {
        try {
            await connection.start();
        } catch (error) {
            console.log(error);
            setTimeout(() => startDemoHub(), 5000);
        }
    }

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

    startDemoHub();

    connection.onclose(async () => {
        await startDemoHub();
    });

    let searching = false;

    async function searchTransaction(hash) {
        let receipt = {};
        if (connection.connectionId === null) {
            receipt = { "error": "Connection error, please try again" };
        } else {
            receipt = await connection.invoke('PollTransaction', hash);
            if (receipt === null) {
                receipt = { "error": "Receipt search timed out" };
            }
        }

        return receipt;
    }

    window.actions = {
        async beginSearchTransaction() {
            // don't repeat process
            if (searching) {
                return;
            }

            // validate input
            const hash = document.getElementById('trx-hash-input').value;
            if (hash.length !== 64 || !isHexString(hash)) {
                return;
            }

            const spinner = document.getElementById('trx-spinner');
            const responseItem = document.getElementById('trx-response');
            spinner.style.display = 'block';
            responseItem.style.display = 'none';

            searching = true;
            const transactionResult = await searchTransaction(hash);
            searching = false;

            responseItem.firstElementChild.innerHTML = JSON.stringify(transactionResult, null, 2);
            onSearchResult();
        }
    };
});

// accordions
var accordions = document.getElementsByClassName("accordion");
for (var i = 0; i < accordions.length; i++) {
    accordions[i].onclick = function () {
        this.classList.toggle('is-open');

        var content = this.nextElementSibling;
        if (content.style.maxHeight !== "0px") {
            // close
            content.style.transition = "none";
            content.style.margin = "0";
            content.style.maxHeight = "0px";
        } else {
            // open
            content.style.transition = "max-height 0.2s ease-in-out";
            content.style.margin = "1em 0";
            content.style.maxHeight = content.scrollHeight + "px";
        }
    };
}