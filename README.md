# Project Documentation

-   Name: Nathan Wildofsky
-   Section: 06
-   Assignment: PROJECT 3: Humans vs. Zombies A


## Description

This project is the first "milestone" of a Unity game that features autonomous agents moving around a scene due to certain goals and desires. In this game a number of humans and zombies, usually more humans at the start than zombies, and one treasure item are created at the start of the game and randomly positioned within a platform. From then on, everything is controlled by the scripts on the human and zombie game objects. Humans will try to seek out the treasure while also running away from any zombies that come near them. Zombies are always chasing the nearest human to them and if a zombie gets close enough it will convert the human into a zombie. If a human gets close enough to the treasure, the treasure is then moved randomly within the platform again. Both humans and zombies move with forces that are results of goals and desires they have, including a force that will always keep them within the boundary of the platform's edges. If all humans are converted to zombies then all of the zombies convene in the center of the platform. The user can also toggle debug lines on and off to see what human each zombie is targeting and the forward and right vectors on both humans and zombies.

## User Responsibilities

The user can press the 'D' key to toggle on and off the debug lines of humans and zombies

## Above and Beyond <kbd>OPTIONAL</kbd>

No above and beyond features were implemented

## Known Issues

When zombies approach a human from both sides at once, the human's steering forces no longer move the human and it gets stuck in between the zombies until it is converted

## Requirements not completed

Every requirement on the rubrik was completed

## Sources

No external assets were used

## Notes

