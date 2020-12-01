# Project Documentation

-   Name: Nathan Wildofsky
-   Section: 06
-   Assignment: PROJECT 3: Humans vs. Zombies B


## Description

This project is the final milestone of a Unity game which features autonomous agents moving around a scene due to certain goals and desires. The game starts with 10 humans, 5 zombies and 4 obstacles. Both the humans and zombies movements throughout the entire game are controlled by scripts. All beginning game objects are given a random position but additional objects can be placed by the user. Humans will wander aimlessly when they do no detect a zombie, and when a zombie enters a human's detection range it will evade any zombie that comes near it. Zombies are always pursuing the nearest human to them and if a zombie gets close enough it will convert the human into a zombie. Both humans and zombies move with forces that are results of goals and desires they have, including a force that will always keep them within the boundary of the platform's edges, a force that steers them away from any obstacles in their path, and a force that keeps all objects from overlapping with one another. If all humans are converted to zombies then all of the zombies will wander until more humans are added to the scene. There are 3 separate camera views in the game including a camera that is able to smoothly follow any vehicle. The user is able to add objects to the scene, switch camera views, toggle degub lines, and reset the scene at any time.

## User Responsibilities

* The 'D' key will toggle on and off the debug lines of humans and zombies.
* The 'C' key will switch to the next camera view in the scene.
* The 'R' key will reset the scene.
* There are 3 buttons each with the words "Add -----" with the blank being a certain type of game object. Pressing any of these will bring the user to the top down camera and allow the user to place that type of object into the scene by left-clicking on the mouse anywhere inside the platform. Press the same button again, press 'escape', change camera views, or reset the scene to exit out of placing objects mode.
* The third camera view will follow the first vehicle in the scene by default but when in this camera view, press left-click on the mouse to move to the next vehicle or right-click to go back to the previous vehicle.

## Above and Beyond <kbd>OPTIONAL</kbd>

* Multiple camera views were added including a smooth follow camera that can follow any vehicle in the scene with interactivity.
* During runtime the user is able to place additional humans, zombies, and obstacles into the scene.
* UI shows the number of humans, zombies, and obstacles in the scene at all times.
* When adding new objects into the scene there is a sprite marker that follows your mouse and lets you know where you are placing that object.

## Known Issues

* Humans are susceptible to being squished of both sides by zombies still and then getting stuck.
* If a human wanders into a zombie it basically gets pulled in by the zombie since the future position used in evade is at that point on the opposite side of the zombie.
* If the user knows how, then they can easily make core mechanics work not as well through placement of obstacles

## Requirements not completed

No 3D models are used in this project and the background scene is the default

## Sources

Placement marker sprite made by me

## Notes

I would like to use my grace period on this project
