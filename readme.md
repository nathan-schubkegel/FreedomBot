FreedomBot
==========

About
-----
This project is (going to be) a crypto day trading bot for the exchange at https://pro.coinbase.com

Trivia:
 - It's named "Freedom" in pursuit of financial freedom, which is attained once your bot is earning enough money that you can quit your day job. :D
 - It's a java project so I can get my dad to collaborate on it with me.
 - It's a desktop application (not a website) because my target audience is folks who don't want their money accessible via a TCP port. Duh.
 - It doesn't have any unit tests because I don't pay me enough to write them for my personal projects.
 - This project is built with gradle because (grumble grumble) 3rd party libraries are a necessary thing, and gradle prioritizes convention over configuration (read: simple by default).

References:
 - Gradle (the build system); 15 minutes to get familiar with it here: https://spring.io/guides/gs/gradle/
 - Gson (the JSON library, because Java doesn't come with one by default); 2 minutes to be delighted with how simple it is here: https://github.com/google/gson
 - Guice (the dependency injector); it could be a lot clumsier, but it's not. https://github.com/google/guice

Building
--------
Download OpenJDK from https://openjdk.java.net/projects/jdk/16/
 - I downloaded version 16.0.1 from https://jdk.java.net/archive/
 - I extracted to C:\Program Files\Java
 - I added "C:\Program Files\Java\jdk-16\bin" to my PATH environment variable
 - I set my JAVA_HOME environment variable to "C:\Program Files\Java\jdk-16" (though maybe I did this for some other project... not sure)

Download Gradle from https://gradle.org/install/
 - I downloaded version 7.3.3 from https://gradle.org/releases/
 - I extracted to c:\gradle
 - I added "C:\gradle\gradle-7.3.3\bin" to my PATH environment variable

From a command prompt, type "gradle build" or "gradle run"
 - FUTURE: add a gradle task to produce an executable exe

Licensing
---------
The contents of this repo are free and unencumbered software released into the public domain under The Unlicense. You have complete freedom to do anything you want with the software, for any purpose. Please refer to <http://unlicense.org/> .