<img width="424" height="338" alt="UmlDijagram" src="https://github.com/user-attachments/assets/e707ad70-3fc2-4292-8118-0945e44e690b" />






# OrganizacijaDogadjajaApp

Sistem za organizaciju događaja implementiran kroz mikroservisnu arhitekturu. Projekat demonstrira resilience patterne, asinhronu komunikaciju i message-driven pristup korišćenjem RabbitMQ message brokera.

---

# Tehnologije

| Oblast | Tehnologija |
|---|---|
| Backend | ASP.NET Core 8, C# |
| Baza podataka | SQL Server LocalDB, Entity Framework Core |
| Message Broker | RabbitMQ |
| Resilience | Polly, Custom Circuit Breaker |
| Frontend | ASP.NET Core MVC |

---

# Arhitektura

```text
MVC App (7100)
    ├── HTTP ──► DogadjajiAPI   (7101)
    ├── HTTP ──► PredavanjaAPI  (7102)
    └── HTTP ──► UcesniciAPI    (7103)

DogadjajiAPI ──► RabbitMQ Fanout ──► PredavanjaAPI
                                 └──► UcesniciAPI

UcesniciAPI ──► RabbitMQ Request-Reply ──► DogadjajiAPI
```

MVC aplikacija služi za prikaz podataka korisniku, dok se komunikacija između mikroservisa odvija putem RabbitMQ sistema poruka.

---

# Implementirani paterni i koncepti

## Retry Pattern

Korišćen Polly `WaitAndRetryAsync` mehanizam sa:

- 2 pokušaja
- 250ms pauze između pokušaja

Retry je primenjen samo na GET zahteve u MVC aplikaciji kako bi se izbeglo kreiranje duplih podataka kod POST zahteva.

---

## Timeout Pattern

Na svakom `HttpClient` named client-u postavljen je:

```csharp
HttpClient.Timeout = TimeSpan.FromSeconds(10);
```

Pri isteku vremena baca se `TaskCanceledException`.

---

## Circuit Breaker

Custom implementacija registrovana kao Singleton servis.

Logika:

- 3 uzastopne greške → stanje OPEN
- OPEN traje 10 sekundi
- nakon toga prelazi u HALF-OPEN
- uspešan zahtev vraća stanje u CLOSED

---

## Outbox Pattern

Dogadjaj i `OutboxMessage` čuvaju se unutar iste SQL transakcije.

`OutboxMessagePublisher` (`BackgroundService`) proverava bazu svakih 5 sekundi i šalje neobjavljene poruke na RabbitMQ.

---

## Idempotent Consumer

Tabela `ProcessedMessages` u:

- PredavanjaAPI
- UcesniciAPI

sprečava višestruku obradu iste RabbitMQ poruke u slučaju at-least-once delivery modela.

---

## Dead Letter Queue

Neuspešno obrađene poruke završavaju u:

```text
dead.letter.queue
```

Korišćenjem:

```csharp
BasicNack(requeue: false)
```

Queue koristi:

- quorum queue tip
- `x-delivery-limit: 10`

---

## Request-Reply Pattern

Pre kreiranja prijave, `UcesniciAPI` proverava da li događaj postoji slanjem RabbitMQ zahteva ka `DogadjajiAPI`.

Korišćeno:

- `CorrelationId`
- `TaskCompletionSource`
- timeout od 5 sekundi

---

## Email Queue i Rate Limiting

Email poruke se šalju u:

```text
email.outbox
```

`EmailWorkerService` obrađuje maksimalno:

- 10 emailova po minuti
- Fixed Window rate limiting

Svaki email se čuva kao `.txt` fajl u:

```text
/Outbox
```

---

# Pokretanje projekta

## Preduslovi

- .NET 8 SDK
- SQL Server LocalDB
- RabbitMQ na `localhost:5672`

---

## Pokretanje servisa

```bash
# Pokrenuti redom u posebnim terminalima

cd OrganizacijaDogadjajaApp.DogadjajiAPI01 && dotnet run   # :7101
cd OrganizacijaDogadjajaApp.PredavanjaAPI  && dotnet run   # :7102
cd OrganizacijaDogadjajaApp.UcesniciAPI    && dotnet run   # :7103
cd OrganizacijaDogadjajaApp                && dotnet run   # :7100
```

Baze podataka se kreiraju automatski pri prvom pokretanju.

RabbitMQ Management UI:

```text
http://localhost:15672
```

Login:

```text
guest / guest
```

---

# RabbitMQ konfiguracija

| Exchange / Queue | Tip | Opis |
|---|---|---|
| `dogadjaji.events` | Fanout Exchange | Event pri kreiranju događaja |
| `dogadjaji.events.predavanja` | Quorum Queue | Consumer: PredavanjaAPI |
| `dogadjaji.events.ucesnici` | Quorum Queue | Consumer: UcesniciAPI |
| `dogadjaji.info.request` | Queue | Request-Reply zahtevi |
| `dogadjaji.info.reply.ucesnici` | Queue | Request-Reply odgovori |
| `email.outbox` | Queue | Email poruke |
| `dead.letter.queue` | Queue | Neuspešne poruke |
