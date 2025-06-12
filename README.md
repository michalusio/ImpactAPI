# ImpactAPI

This application is solution to the coding task for the interview at IMPACT commerce.

Now, I'm going to be frank, I had too much fun with this and might have gone a bit overboard.

## Starting up

You will need:
* Visual Studio 2022 or newer (I did not test on older versions)
* .NET 8
* Docker running on your machine (for TestContainers)

## Technical Requirements

I have tested the [API](https://tenders.guru/pl/api) provided for the task and immediately I have noticed a very slow response time (~1-2 seconds).

Furthermore, their API does not provide any means of sorting or filtering over tenders.

That throws the naive implementation out of the box. (The naive implementation would load data from the 100 pages and sort/filter on the backend before responding. Very bad.).

I have decided to just download all 100 pages of data at application startup and add indexes to it so requests can be handled super-fast.

To do that, I used [TestContainers](https://testcontainers.com/) which allow me to run a dockerized database, which I run with the development build (with volume mount) as well as for tests themselves (without a volume mount).

To make the waiting time somewhat palatable the API will respond with `503 Service Unavailable` with a `retry-after` header set to the estimated time left to load all needed tenders.

The database itself is handled automatically through EFCore migrations (automatically ran in development builds), so I basically don't need to manage anything in terms of local infrastructure.

As for pagination of the API endpoint, I was thinking of using cursor-based pagination as one of the quality requirements was `Scalability`, but decided it will be much more readable to use a simple `OFFSET LIMIT` for now, even though it chokes at larger datasets.

## Functional Requirements

The user is able to use one of the endpoints in the API to load a tender by Id - they will receive all the basic data of the tender, as well as the supplier for that tender (with Id and Name).

The collection endpoint allows for filtering by Date and/or AwardedValueInEuro through specifying the From and To values, as well as for sorting by Date, AwardedValueInEuro or Id (when no sort field is provided), both ascending and descending.
You can also specify a SupplierId to get tenders only for a specific supplier.

## Quality Requirements

The application is scalable by means of few things:
1. It separates the data loading from data requests by having a hosted service do all the loading. This allows separating from a slow API and adding features on top, because now we have all the data. If the data were to change dynamically we need to add some mechanism of checking for updates, but that's not hard.
2. The database has a clustered index on Id, and nonclustered indexes on Date and AwardedValueInEuro, which allows for fast querying by those columns. We could also switch to cursor-based pagination to keep the speed on huge datasets.
3. The application has output caching and client-side caching enabled, which means responses are cache server-side, but also client-side. This means reduced load on the server for repeated requests.

The application keeps maintainability by being structured using Vertical Architecture - the domain of Tenders is kept in a separate folder, while all infrastructure-related things that may be reused are kept in a separate project. When the application gets larger and we have a need to split it out into microservices, it is trivial to just take the Tenders folder out into a separate deployable project.

For communication between vertical slices we could use a message broker - either MediatR for in-process communication, or a message bus handler like MassTransit when the application is split into separate deployments.


Testing the application, whether by unit, integration or E2E tests is not hard because of the usage of TestContainers - the same code that runs the containers for development builds can be reused for running tests. The only thing we needed to add was to not mount a volume, which means the container will be blank for each test.

As for readability, I would say using Vertical Architecture enhances it, as the developer can keep the context of their changes to one slice of the application, as opposed to e.g. Onion or Clean architectures, where the developer has to jump through 10 projects and 30 folders to add a feature because all contextually-connected classes are kept in separate folders.

I have decided to not use AutoMapper, for one because it decreases readability long-term as you have to decode the "magic" that happens with mappings, and for two because it moves a lot of the compile-time errors you could get into run-time. Boilerplate issues that could arise can be decreased by using source generators, or even AI to generate mappers for you.