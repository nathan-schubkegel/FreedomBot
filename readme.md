FreedomBot
==========

About
-----
This project is (going to be) a crypto day trading bot for the exchange at https://pro.coinbase.com

Trivia:
 - It's named "Freedom" in pursuit of financial freedom, which is attained once your bot is earning enough money that you can quit your day job. :D
 - ~~It's a java project so I can get my dad to collaborate on it with me.~~ It's a C# project because I needed to make progress faster - I only have an hour or so per evening to peck at this.
 - It's a desktop application (not a Single Page App) because my target audience is folks who don't want their money accessible via a TCP port. Duh.
 - ~~It doesn't have any unit tests because I don't pay me enough to write them for my personal projects.~~ It totally has unit tests, because this project is about money, and bugs cost money.

Building
--------
Download .NET 6 SDK from https://dotnet.microsoft.com/en-us/download/dotnet/6.0
 - I downloaded SDK version 6.0.200

From a command prompt
 - type "dotnet build" to see if everything compiles
 - type "dotnet run --project FreedomBot" to compile and run it (or change directory to FreedomBot and then just "dotnet run")
 - type "dotnet publish FreedomBot --runtime win-x64 --self-contained" to produce a distributable exe (and bazillion other necessary files). Consider adding "--output <dirPath>" to control where the files are generated.

Testing
-------
From a command prompt
 - type "dotnet test" to run the xunit tests
 - type "dotnet run --project FreedomBot.ManualTests" to run query tests that hit coinbase's API. This will prompt for an API key.

References
----------
TODO

Licensing
---------
Except for the components noted below, the contents of this repo are free and unencumbered software released into the public domain under The Unlicense. You have complete freedom to do anything you want with the software, for any purpose. Please refer to <http://unlicense.org/> .

Twofish Block Cypher
 - Algorithm by Bruce Schneier; Twofish is unpatented, and the original C source code is uncopyrighted and license-free; it is free for all uses. See https://www.schneier.com/academic/twofish/
 - Code by Josip Medved <jmedved@jmedved.com>, www.medo64.com; MIT License 