# AutonoWar

A game focused on realtime competition between user-programmed robots.

**Note to alpha-testers**: This is an alpha! It's buggy, and it's unstable. Things may break, things may disappear. Expect breaking changes in the future.

Installation
------------

Currently, AutonoWar is separated into two separate pieces of software: The game itself, which can be found on the [release page][1] as a zip file, and the python package allowing you to write code to control your robot. In the documentation, the game is referred to as "AutonoWar" and the python package as "awconnection."

To install AutonoWar (the game), head to the release page and download the latest zip. Right now, nothing is that stable, so don't worry about choosing a too-recent release. Double click on the zip file, and run the .exe/.app inside. Windows users should have no problem installing; mac users might. If mac is giving you problems, head to the **mac security issues** section below.

To install the python package, 
+ Make sure you have python >3.6 installed.
+ Make sure you can run python from your shell or in an ide.

Then run the following command in your shell:

    pip install awconnection
    
Or if that doesn't work:

    pip3 install awconnection
    
If you want to upgrade, run:

    pip install --upgrade awconnection
    
Mac Installation/Security Issues
--------------------------------

MacOS (and more specifically macOS Catalina) doesn't always play well with apps from unknown developers. There's a very good chance you will run into a screen saying:

    "AutonoWar-x.y.z" cannot be opened because it is from an unidentified developer.
    
If this happens, go System Preferences-->Security and click "Open Anyway." 

You may also see an error saying:

    The application "AutonoWar-x.y.z" cannot be opened.
    
On recent macOS versions, apps not built on windows don't always get flagged as executable. If this happens,
+ Open terminal
+ `cd` **into** the app itself (e.g. `cd "/Users/John Doe/Desktop/AutonoWar-x.y.z.app"`)
+ Make the app executable: `chmod +x Contents/MacOS/*`

Documentation
-------------

Documentation for the python API can be found on this repo's [wiki page](https://github.com/griffinteller/AutonoWar/wiki).

Contribution
------------
**Found a bug?** Go ahead and open an [issue][2]. **Got an idea?** Go create a card on our [trello page](https://trello.com/invite/b/nCJ2ejBj/5024f944fcaaff7247377fa953e70b05/autonowar-asterisk-autochampions-features-and-bugs). If you want to jump in and get your hands dirty, feel free to fork this repo and submit a pull request (just make sure you're using unity==2019.3).

[1]: https://github.com/griffinteller/AutonoWar/releases
[2]: https://github.com/griffinteller/AutonoWar/issues
