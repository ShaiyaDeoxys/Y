# Imgeneus

Imgeneus is a simple and elegant Shaiya EP8 socket server over the TCP/IP protocol, built with C# 10 and .NET 6.0.

This project has been created for learning purposes about the network and game logic problematics on the server-side.
We choose [Shaiya](https://shaiya.fandom.com/wiki/Main_Page) because  it is a simple game, but enough complex to learn the basic functions of an MMO game architecture.
It's not about playing a game or competing with any services provided by Aeriagames or its partners, and we don't endorse such actions.

This repo also uses the best parts of these repos: [Drakkus/ShaiyaGenesis](https://github.com/Drakkus/ShaiyaGenesis), [Origin](https://github.com/aosyatnik/Origin) and original Imgeneus (removed by creator).

## Build and run
1. Build the solution.
2. Create 3 files `appsettings.Development.json` in Imgeneus.Login & Imgeneus.World & Imgeneus.Database projects. Override default configs with your development config. E.g. you can provide your password as following:
```
{
  "Database": {
    "Password": "your_password"
  }
}
```
These files are added to the ignore list, so you can be sure, that you won't commit any of your credentials.

3. Add to folder `config\SData` next files: `DBItemData.SData`, `NpcQuest.SData`. You can find these files in os game data.saf. If you can not open EP 8 data.saf file, please check [Parsec project](https://github.com/matigramirez/Parsec).

4. Run Imgeneus.Login & Imgeneus.World projects. For this right click on solution => Properties => Startup Project => Multiple startup projects => Imgeneus.Login - Start; Imgeneus.World - Start.

__You are not dev, feel lost but still want to start?__  Check out our [wiki page](https://github.com/aosyatnik/Imgeneus/wiki/Setup-for-non-devs).

## Solution description

##### Imgeneus.Core
Core contains some common helpers, extensions etc.

##### Imgeneus.Database
We are using EF Core for database connections and migrations. You can read more about EF [here](https://docs.microsoft.com/en-us/ef/core/).

##### Imgeneus.Network
Includes packet definition from client to server and cryptography implementation.

##### Imgeneus.Logs
In-game logs, that are saved via SQLite in file.

##### Imgeneus.InterServer
Communication between servers (login and game) is done with the help of SignalR. You can read more about SignalR [here](https://docs.microsoft.com/en-us/aspnet/core/tutorials/signalr).

##### Imgeneus.Game
Game logic.

##### Imgeneus.Login
Login server, that handles all packets in the login screen. Redirects to the selected game server.

##### Imgeneus.World
Game server. As well as login server, it's TCP server running on top of ASP.Net Core. You can read more about ASP.Net [here](https://docs.microsoft.com/en-us/aspnet/core).

##### Unit tests
Any game feature must have a corresponding unit test.

### Database migrations
* Migrations are done automatically as soon as you run world server.
* You can also migrate manually via Package console or .NET cli. You can read more about migrations [here](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/).
* To fulfill the database with example data open `src\Imgeneus.Database\Migrations\sql` folder and run `setup.bat` file (Don't forget to set your password there). This will populate your database with ep6 original data.

## Details
- Language:  `C# 10`
- Framework:  `.NET 6.0`
- Application type:  `Console`
- Database type:  `MySQL`, Version `8.0.26`
- Configuration files type:  `JSON`
- Environment: `Visual Studio 2022`
- OS: `Win 11`

#### Client versions support
Our main goal is learning, clients are used only for testing. We are not trying to harm any server or dev team. That's why we are not exposing any clients or server name, where these clients are used. Next versions supported:
* EP 8, you can find it on e**rs.com. In code marked as `EP8_V1`. __No longer supported__
* EP 8, the newest version of private server. In code marked as `EP8_V2`.
* EP 8, the latest version of the Shaiya US client. You can download the client from the official publisher. Marked as `SHAIYA_US` in the build configuration.

## Results
![image1](images/image1.JPG?raw=true "Title")
![image2](images/image2.JPG?raw=true "Title")

## Hall of fame :star:
* __KSExtrez__ - firstly started the project and setup DI containers and web socket pool.
* __Cups__ - the first person, that created a dummy emulator, with his source code we could understand packet structure.
* __anton1312__ - helped with packet encryption algorithm implementation in C#.
* __Juuf__ - helped with Dye system packet structure.
* __matigramirez__ - helped with the implementation of different features.
* __Bowie__ - provided solid asm support.
* __ZheinGlitch__ - helped with Chaotic square packet structure.
* __Eastrall__ - helped with project structure and provider awesome libraries [LiteNetwork](https://github.com/aosyatnik/LiteNetwork) and [Sylver.HandlerInvoker](https://github.com/aosyatnik/Sylver.HandlerInvoker).
* __Kreon__ - helped with enchant packet structure.
* __Razor__ & __Kegi__ - created guides for beginners.
* __UZC__ - answered a lot of ep 8 questions.
* maybe __YOU__?

## Known issues
Issues are usually marked as `// TODO: <some comment>` throughout the code base.

Find the current state of development [here](https://trello.com/b/lHvyQDuH/shaiya-imgeneus).

Project website [here](https://shaiya.imgeneus.online/).
