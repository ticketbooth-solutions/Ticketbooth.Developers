'use strict';
import { openDB } from 'https://unpkg.com/idb?module';

if (!('indexedDB' in window)) {
    console.log('This browser doesn\'t support IndexedDB.');
}

const dbPromise = openDB("Ticketbooth.Demo", 1,
    {
        upgrade(db) {
            const contractsStore = db.createObjectStore('Contracts', {
                keyPath: 'address'
            });

            contractsStore.createIndex('block', 'block');

            db.createObjectStore('Wallets', {
                keyPath: 'name'
            });
        }
    });

const Contracts = {
    async add(address, owner, saleActive, block) {
        return (await dbPromise).add('Contracts', {
            address,
            owner,
            saleActive,
            block
        });
    },
    async setSaleActive(address) {
        const db = await dbPromise;
        let contract = await db.get('Contracts', address);
        return (await dbPromise).put('Contracts', {
            address,
            owner: contract.owner,
            saleActive: true,
            block: contract.block
        });
    },
    async setSaleInactive(address) {
        const db = await dbPromise;
        let contract = await db.get('Contracts', address);
        return (await dbPromise).put('Contracts', {
            address,
            owner: contract.owner,
            saleActive: false,
            block: contract.block
        });
    },
    async all() {
        return (await dbPromise).getAllFromIndex('Contracts', 'block');
    }
};

export const Wallets = {
    async create(walletName, accountName, password) {
        return (await dbPromise).add('Wallets', {
            name: walletName,
            account: accountName,
            password: password,
            addresses: []
        });
    },
    async addAddress(walletName, address) {
        const db = await dbPromise;
        let wallet = await db.get('Wallets', walletName);
        wallet.addresses.push({
            address,
            balance: 0
        });
        return (await dbPromise).put('Wallets', {
            name: walletName,
            account: wallet.account,
            password: wallet.password,
            addresses: wallet.addresses
        });
    },
    async removeAddress(walletName, address) {
        const db = await dbPromise;
        let wallet = await db.get('Wallets', walletName);
        for (let x = 0; x < wallet.addresses.length; x++) {
            if (wallet.addresses[x].address === address) {
                wallet.addresses.splice(x, 1); // remove item
                break;
            }
        }
        return (await dbPromise).put('Wallets', {
            name: walletName,
            account: wallet.account,
            password: wallet.password,
            addresses: wallet.addresses
        });
    },
    async updateBalance(walletName, address, balance) {
        onBalanceUpdated(address, balance);
        const db = await dbPromise;
        let wallet = await db.get('Wallets', walletName);
        for (let addressItem of wallet.addresses) {
            if (addressItem.address === address) {
                addressItem.balance = balance;
                break;
            }
        }
        return (await dbPromise).put('Wallets', {
        name: walletName,
        account: wallet.account,
        password: wallet.password,
            addresses: wallet.addresses
        });
    },
    async all() {
        const db = await dbPromise;
        return await db.getAll('Wallets');
    }
};

export async function checkForWallets() {
    const wallets = await Wallets.all();
    if (wallets.length !== 0) {
        await fillWalletDetails(wallets);
    } else {
        // show create form
        const walletCreateForm = document.getElementById('wallet-create-form');
        walletCreateForm.removeAttribute('hidden');
    }
}

function onBalanceUpdated(address, balance) {
    const addressLists = document.getElementsByClassName('address-list');
    for (const addressList of addressLists) {
        for (const addressItem of addressList.children) {
            if (addressItem.firstElementChild.innerText === address) {
                addressItem.lastElementChild.innerText = formatTokenBalance(balance);
                return;
            }
        }
    }
}

async function fillWalletDetails(wallets) {
    const walletList = document.getElementById('wallet-list');

    if (walletList.childElementCount > 0) {
        return; // already filled
    }

    for (const wallet of wallets) {
        const walletItem = document.createElement('li');
        walletItem.classList.add('wallet-details');

        // wallet details
        const walletDetailsItem = document.createElement('dl');

        const walletNameLabel = document.createElement('dt');
        walletNameLabel.innerText = 'Name';
        const walletNameItem = document.createElement('dd');
        walletNameItem.innerText = wallet.name;

        const walletAccountLabel = document.createElement('dt');
        walletAccountLabel.innerText = 'Account';
        const walletAccountItem = document.createElement('dd');
        walletAccountItem.innerText = wallet.account;

        const walletPasswordLabel = document.createElement('dt');
        walletPasswordLabel.innerText = 'Password';
        const walletPasswordItem = document.createElement('dd');
        walletPasswordItem.innerText = wallet.password;

        walletDetailsItem.appendChild(walletNameLabel);
        walletDetailsItem.appendChild(walletNameItem);
        walletDetailsItem.appendChild(walletAccountLabel);
        walletDetailsItem.appendChild(walletAccountItem);
        walletDetailsItem.appendChild(walletPasswordLabel);
        walletDetailsItem.appendChild(walletPasswordItem);

        // wallet addresses
        const walletAddressList = document.createElement('ul');
        walletAddressList.classList.add('address-list');

        for (const addressDetails of wallet.addresses) {
            const walletAddressItem = document.createElement('li');

            const walletAddress = document.createElement('strong');
            walletAddress.innerText = addressDetails.address;

            const walletBalance = document.createElement('span');
            walletBalance.innerText = formatTokenBalance(addressDetails.balance.toString());

            walletAddressItem.appendChild(walletAddress);
            walletAddressItem.appendChild(walletBalance);

            walletAddressList.appendChild(walletAddressItem);
        }

        walletItem.appendChild(walletDetailsItem);
        walletItem.appendChild(walletAddressList);

        walletList.appendChild(walletItem);
    }

    walletList.removeAttribute('hidden');

    // remove create form
    const walletCreateForm = document.getElementById('wallet-create-form');
    walletCreateForm.remove();
}

function formatTokenBalance(input) {
    const parts = input.split('.');
    let fractional = '00000000';
    if (parts[1] !== undefined) {
        fractional = parts[1].padEnd(8, '0');
    }

    return parts[0].concat('.', fractional, ' CRS');
}

async function fillUserContracts() {
    const contracts = await Contracts.all();
    const userContractList = document.getElementById('user-contracts');
    for (const userContract of contracts) {
        const userContractItem = document.createElement('li');

        const contractAddressItem = document.createElement('strong');
        contractAddressItem.innerText = userContract.address;
        const contractStateItem = document.createElement('span');
        const contractStateValue = userContract.saleActive ? 'Active' : 'Inactive';
        contractStateItem.innerText = contractStateValue;
        contractStateItem.classList.add(contractStateValue.toLowerCase());

        userContractItem.appendChild(contractAddressItem);
        userContractItem.appendChild(contractStateItem);

        userContractList.appendChild(userContractItem);
    }
}

fillUserContracts();