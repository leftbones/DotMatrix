# Dot Matrix Engine

While working on a cellular automata particle simulation, I realized the engine could be used for a lot of different things, and decided to move it to it's own repo.

Over time, it's become more geared towards game development than it's original intention as just a simulation tool, but with the integration of ECS, it can be molded into basically anything.

## Major Features
* Chunk-based matrix processing for a limitless matrix size
* Multithreading to dramatically increase performance (about 300%)
* An extensible input/event system with support for keyboard, mouse, gamepad, and touch
* A custom extensible GUI system built from scratch
* Physics integration with Box2D for rigidbody entities
* Entity Component System (ECS) for manageable yet scalable growth
* Limitless potential for new particle elements and element interactions

## Some Planned Features
* Modding API integration using Lua scripting
* Destructible terrain/objects using the already implemented "PixelMap" component
* Element interactions such as burning, explosions, rust, electrification, dissolving, and a lot more
