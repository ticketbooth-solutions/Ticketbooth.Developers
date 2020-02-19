Title: Ticketbooth API Basics
Description: Things you should know before working with Ticketbooth
Order: 6
---

# Understanding smart contracts

## Syncing your full node

It is important that your full node is fully synced before interacting with the blockchain or with Ticketbooth API, to ensure that transactions are successful and data returned by the API is up to date.

## Usage costs

Blockchain transactions have an implicit cost associated with them, as maintaining and executing code on the network needs to be rewarded. Therefore, any interaction with the Ticketbooth smart contract that involves a transaction has a gas fee.

Fortunately, reading data from the blockchain has no cost associated. Therefore, all _GET_ endpoints on the Ticketbooth API are free to consume. The cost of using Ticketbooth comes with using the _POST_ endpoints, all of which create a transaction on the network. Costs can vary depending on the position of the network and the endpoint, though it's generally very minimal and in most cases arbitrarily small.

## Developing around transactions

Smart contract transactions are not instant and are dependent on being picked up by masternode operators on the network. Each transaction requires a gas fee to be specified. A transaction with a higher gas fee is more likely to be picked up sooner. Cirrus network has a block time of 16 seconds, meaning that generally a transaction will be taken up by the network, after it has been broadcast, within that time frame.

Transactions can fail and so it is important to understand that even if the web API returns a success response, until you read the transaction receipt, it is uncertain that the operation is successful. A success response for a _POST_ request on Ticketbooth API only indicates that a transaction has been broadcast on the network. Upon a success response from the Ticketbooth API, the network must be polled to view the transaction result.

# Ticketbooth contract rules

The Ticketbooth contract comes with a set of configurable rules, which can be configured by the owner of the contract. These rules are publicly available to any consumer. It is important that the rules are made clear to the user of your application. Rules can be obtained from the following endpoints.

## Ticket release fee

```http
GET /api/v1.0/ticketbooth/{address}/TicketReleaseFee HTTP/1.1
```

The ticket release fee is the cost of retrieving a refund for a ticket. When a refund is requested, this fee is subtracted from the price of the ticket. This value is in the form of strats (a 100 millionth of a full token).

## No release block count

```http
GET /api/v1.0/ticketbooth/{address}/NoReleaseBlocks HTTP/1.1
```

The no release block count is the number of blocks, before the end of the contract, where tickets cannot be released back to the contract.

## Identity verification policy

```http
GET /api/v1.0/ticketbooth/{address}/IdentityVerificationPolicy HTTP/1.1
```

The identity verification policy indicates whether the venue requires an identification document to enter, along with the ticket. Some event organisers might opt to require this to prevent ticket scalping or ticketing fraud.
