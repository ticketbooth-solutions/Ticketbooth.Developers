Title: Managing Ticket Sales
Description: Create a service to manage ticket sales
Order: 9
---
# Creating a ticketing contract

Ticketbooth smart contracts are created for a collection of _seats_, which are grouped under a certain _venue_. The terms _seats_ and _venue_ are used loosely, as they do not have to describe a physical seat or an actual venue. The _seats_ must be unique and are identified by either a number or letter, or both. A _venue_ can be given any name that identifies the _seats_ that are being sold. Individual Ticketbooth contracts can be reused an unlimited number of times, for multiple events. The Ticketbooth API comes with an endpoint to create a ticketing contract.

```http
POST /api/ticketbooth HTTP/1.1
```

The maximum number of tickets a Ticketbooth contract can handle is 65, though it is recommended to handle less per contract, so that you can offer a higher than minimum gas price when managing the smart contract. This is to allow you to start ticket sales quickly, if the network gets congested.

# Setting contract rules

Before running a ticket sale, first set the Ticketbooth rules! The Ticketbooth rules are explained in [this](../ticketbooth_api_basics#ticketbooth-contract-rules) documentation section. You can only set rules when a ticket sale is not active, though they can be changed as many times as needed.

## Ticket release fee

```http
POST /api/v1.0/ticketbooth/{address}/TicketReleaseFee HTTP/1.1
```

You can set this to as much or as little as you want. This value is in the form of strats (a 100 millionth of a full token). By default, it is set to 0.

## No release block count

```http
POST /api/v1.0/ticketbooth/{address}/NoReleaseBlocks HTTP/1.1
```

If you want to have a standard refund policy, you will only need to set this one time. By default, it is set to 0.

## Identity verification policy

```http
POST /api/v1.0/ticketbooth/{address}/IdentityVerificationPolicy HTTP/1.1
```

To absolutely prevent ticket scalping, set this to true. Scanning tickets will reveal the purchaser name, which can be matched against the attendee identity document. You will need to have a policy in place to disallow entry to those without matching names on their ticket. By default, it is set to false.

# Starting a sale

When starting a sale, you must price each ticket and provide some details about the event. Be aware that this is the most costly operation, so use a low gas price for this to succeed. A sale can be started with an endpoint on the API.

```http
POST /api/v1.0/ticketbooth/{address}/BeginSale HTTP/1.1
```

While a sale is in progress, you will not be able to change any rules on the smart contract. Event details cannot be changed and a sale cannot be extended, so make sure that the details provided are correct.

# Preparing for a new sale

A sale ends when the block height provided for the end of a sale is reached. Once the block height is reached, the purchase data persisted to the contract needs to be reset.

```http
POST /api/v1.0/ticketbooth/{address}/EndSale HTTP/1.1
```

This will deactivate the ticket sale and allow you to configure the contract rules and start another sale. You can deactivate the ticket sale before the tickets are scanned, however do not start another sale until the event has finished.