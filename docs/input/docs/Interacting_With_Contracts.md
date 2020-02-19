Title: Interacting with Contracts
Description: Building a service that consumes existing Ticketbooth smart contracts
Order: 8
---
# Customer integration

## Purchasing tickets

Be aware that purchasing of a ticket reveals your wallet address, so if you want more anonymity, you will want to manage multiple wallets for purchases.

### Viewing ticket availability

```http
GET /api/v1.0/ticketbooth/{address}/Tickets HTTP/1.1
```

Available tickets have an address which is equal to the default ```Address.Zero```. This is different depending on the blockchain network. For Cirrus testnet, this value is _t6vc3nrbAurGs3i17HJUavZuw4ioKTiFCE_ and for Cirrus mainnet _CGTta3M4t3yXu8uRgkKvaWd2d8DQvDPnpL_. Any ticket which has a different address is not available for purchase.

### Generating a ticket

| ![Digital QR ticket](../images/e86e2e05-b0c8-4dd5-a0a4-ee006021c509.png) |
|:--:| 
| *Example ticket* |

A digital ticket is a QR code that contains JSON data for one or more tickets. This data is formatted in a particular way, so that it can be parsed by the ticket scanner.

```json
[
    {
        "seat": {
            "number": 1,
            "letter": "A"
        },
        "secret": "1vP-b8JwlL0ae4B",
        "secretKey": "894be55a951fee6c269d91ab77fa2fa7e48c56dd71d1e39c43b5a31ec16f5afe",
        "secretIV": "54b3a9a6743a775b923c90f3fb86d1c6",
        "nameKey": "4d38a395594a93e64783744eae669478de1a60be372ed24fdd42a82182eb502d",
        "nameIV": "7d4984f0bda1d5bbb341edf26553e896"
    }
]
```

Key and IV data must be provided as a serialized hex string. Keys are 32 bytes in size and IVs are 16 bytes in size. The plaintext secret is a randomly generated string with a length of 15. The necessary data is returned in the response body of the call to the endpoint used for reserving a ticket.

```http
POST /api/v1.0/ticketbooth/{address}/ReserveTicket HTTP/1.1
```

It is important that the resulting QR code is only shared with the customer over an encrypted communication channel. The customer should be advised to not share their QR code with anyone, to avoid replication of their ticket.

## Refunding a ticket

To successfully refund a ticket, it must be done before the no release block count limit is reached. If this limit is reached, a ticket will be unable to be refunded.

```http
POST /api/v1.0/ticketbooth/{address}/ReleaseTicket HTTP/1.1
```

A ticket refund request must be transacted using the same address that was used to purchase the ticket, otherwise it will fail. The amount refunded to the address will be the price of the ticket, with the ticket release fee negated.