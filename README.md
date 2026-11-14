# JourneyLanes

A 3D driving game that helps learner drivers practise for the UK DVSA theory test by putting road signs, hazards and road rules in the context of actual driving, instead of memorising them from a book.

Built in Unity 6 with C#, which I taught myself - (both C# and Unity) from scratch for this project, over roughly 50 hours.


## The problem

Most learner drivers prepare for the theory test by memorising sign meanings from a website or book and then taking a quiz. There is little practical element, so learners often fail to recognise a sign when they meet it on a real road, because they only ever learned the symbol in isolation.

JourneyLanes puts the same material inside a driving simulation. Questions and scoring are triggered by what the player encounters while driving, so a road sign is learned where it actually appears.

## Scope

This build is a single demo level. It is deliberately built as a base rather than a finished game: every system in it is written to be reused, so further levels covering different topics and situations can be assembled from the same scripts without rewriting the behaviour.

## Features

**Driving and road systems**

- **NPC traffic.** Other vehicles follow the road and obstruct the player's path, forcing the player to react to live traffic rather than an empty road.
- **Street signs** placed along the route.
- **Road scenarios.** Merging and meeting situations that mirror real driving decisions.
- **Speedometer and a capped top speed**, the base for speed limit checks.

**Assessed driving behaviour**

- **Stop sign detection.** The player must come to a stop at the sign and wait several seconds before moving off. Doing so awards points; failing to stop deducts them. This script is the reusable base for the rest of the sign and junction behaviours, since traffic lights, roundabout entry and similar checks all follow the same shape: detect the trigger, require a condition to be held, then reward or penalise.
- **Wrong lane detection.** If the player drives in the wrong lane, a timer starts and they are given a window to correct themselves. Failing to return to the correct lane within that window incurs a penalty.
- **Driving score.** Points are awarded and deducted for driving behaviour itself, not only for answering questions, so lane discipline and junction behaviour affect the level score directly.

**Theory questions**

- A multiple-choice question on a road sign, triggered when the player drives past it. The game pauses until it is answered.
- **Explanation on a wrong answer.** When the player answers incorrectly, the correct answer is shown, rather than just marking it wrong.
- **Live points feedback.** Every gain or loss appears on the HUD with a short reason as it happens.

**Accounts**

- Sign-up and login backed by a SQLite database, with passwords stored as salted scrypt hashes.

## Planned

These are not in the demo yet. Each one builds on the systems above rather than needing new ones.

- **Traffic lights**, using the same trigger and scoring pattern as the stop sign.
- **Speed limit zones and speed checks**, reading the speed the speedometer already measures.
- **More questions across more topics**, including vehicle safety and motorway rules, loaded from a data file instead of being written into the script.
- **End-of-level points breakdown**, showing where points were gained and lost across the level.
- **Saved progress per account**, so scores are kept against the logged-in user.

## Design

The project was thought out and designed extensively before it was built, with stakeholder research, decomposition and abstraction, UML class diagrams, algorithm selection and test plans. The main design decisions are below.

### NPC navigation

My original design called for A\* pathfinding over a waypoint system, with a fallback comparison against Dijkstra's algorithm. The reasoning at the time was that A\*'s heuristic reaches an optimal path in fewer node expansions than Dijkstra's, so it would cost less per frame while still producing sensible routes around obstacles. I also ruled out Unity's built-in NavMesh, since it is designed for agents that can turn sharply at speed, which is nothing like a car's turning behaviour.

What I actually implemented is a pure waypoint-following system. The NPC cars traverse an ordered set of waypoints along the road rather than searching for a path between them.

In hindsight that was the right call for this build, and it is worth being precise about why. A\* earns its cost when there is a genuine search problem: many possible routes through a graph and a real decision about which to take. In a single demo level where the road layout is fixed and vehicles are meant to stay in lane and follow the carriageway, there is no route to discover. This also applies to future levels, where the NPCs will follow the same path to set a specific obstacle for the user. The waypoints *are* the route. Running a search every frame to rediscover a path I had already defined by hand would have added cost and complexity for no behavioural gain.

A\* would start to pay off once the map has junctions where a vehicle could legitimately choose between routes, or once traffic needs to re-route dynamically around a blockage. That is the natural point to revisit it.


### Code structure

The game is built from small single-purpose `MonoBehaviour` components attached to objects in the scene, rather than an inheritance hierarchy. Unity favours composition over inheritance, so a road feature is defined by which components sit on it instead of by what it derives from.

**The trigger and scoring pattern.** Assessed road behaviours share one shape, which is what makes them reusable. A trigger volume is tagged, `OnTriggerEnter`, `OnTriggerStay` and `OnTriggerExit` check that tag and the `Player` tag, a timer accumulates while the player is inside, and the result is reported to a central `PointsManager` with a short explanation of why points moved.

`StopSignTriggerDetection` uses two trigger volumes, `StopSignInitial` and `StopSignFinal`. Time spent in the first is accumulated; leaving it after three seconds awards points, while reaching the second volume too early deducts them and shows a warning. Both triggers are then disabled so the same sign cannot be scored twice.

This pattern can transfer directly to other rules. Traffic lights, roundabout entry and junction checks are all the same thing with a different condition: detect the volume, require something to be held or true, then reward/penalise. `LaneDetection` already reuses it, warning and deducting points after two seconds in the wrong lane and restarting the level after five.

**Scoring.** `PointsManager` is the single place the score lives. Every scoring component calls `IncreasePoints` or `DecreasePoints` with an amount and a message, so the HUD explains each change as it happens instead of showing a number that moves for no visible reason.

**NPC traffic.** `NPCmovement` holds an array of waypoint transforms and moves toward each in turn with `Vector3.MoveTowards`, rotating toward the direction of travel with `Quaternion.RotateTowards` so the car turns rather than snapping to face its target, making its movement be more natural. It advances the index once it is within 0.1 units of the current waypoint, and destroys itself after the last one to avoid leaving vehicles in the scene - a small optimisation.

**The player car.** `playermovement` drives a `Rigidbody` with physics rather than moving the transform directly: `AddForce` along the car's forward vector for acceleration, with rotation applied only while the car is actually moving, so it cannot spin on the spot. Speed is capped by normalising the velocity vector and scaling it to the maximum. `playerCollision` watches for obstacle collisions and ends the run, and `FollowPlayer` keeps the camera behind the car using a fixed offset. Another version of `FollowPlayer` is available to test out, which is a more "fluid" version - though i prefer not to use it, some other users who tested the game enjoyed it, so both are available to use for you to test.


### Database and authentication

Accounts live in a SQLite database, created automatically on first run, with a `users` table holding the username as the primary key and a hash of the password. Storing passwords in plaintext would be a serious security risk, and hashing means that even I cannot read a user's password, since the operation is one-way.

Passwords are hashed with scrypt and a random 16-byte salt per user, both from Python's standard library. The first version used plain SHA-256, which is designed to be fast, so anyone holding a copy of the database could test guesses at an enormous rate, and two users with the same password had the same hash. scrypt is deliberately slow and memory-hungry, and the salt makes every stored hash different. Accounts created under the old scheme are upgraded to scrypt the next time they log in successfully.

Authentication itself runs in a Python script that the Unity client launches as a separate process, passing credentials over standard input and receiving a JSON result. Splitting it out this way keeps the credential handling and the database schema in one place, independent of the game code.

Because the username is the primary key, SQLite indexes it, so login is a single indexed lookup rather than a scan. A duplicate sign-up is caught by the uniqueness constraint rather than by checking for the username first, which avoids a race between the check and the insert. All queries are parameterised, so user input cannot alter the SQL.

Login returns the same message whether the username or the password was wrong, so the response cannot be used to work out which usernames exist. The hash comparison uses a constant-time check rather than `==`.

Save data for level progress is not built yet. When it is, it will not use a database, because it only needs static storage rather than querying.

### Levels and scoring

Topics are intended to be separated into individual levels, so, for example, a road signs level only tests road signs. This keeps per-topic performance visible at a glance from the level select screen.

Level content is pre-made rather than randomised or procedurally generated. This saves memory and processing, and it keeps scoring consistent, since a randomised level would vary in difficulty between attempts and make grades incomparable. It also lets players repeat a level and improve on a specific topic they may be weak on

Each level has its own point total based on its objectives and obstacles. The player starts at zero and gains points for correct actions, loses for incorrect ones, and instantly fails when crashing. Points update live on the HUD with the reason for each change. A breakdown at the end of the level is planned.

### Accessibility and usability

Usability was planned as part of the design rather than added afterwards:

- Descriptive text on or above interactive elements, so their purpose is clear.
- A readable font at a size that is neither too small to read nor large enough to crowd the screen.
- Buttons that highlight or change colour on hover to signal that they are interactive.
- Centred UI on the login, sign-up and level select screens, so the eye does not have to travel across the screen.

### Testing

Testing was planned in two stages: iterative white box testing during development, and black box testing afterwards against the success criteria. Each success criterion had its own test, for example completing the sign-up form and confirming the record appeared in the database and that test data was stored against that account.

## Limitations

- No budget for assets, so the project uses free models rather than custom or purchased ones.
- Graphics and realism were deliberately kept simple, since the target audience is learner drivers rather than gamers, and the game needed to run on low-specification laptops.
- The current build is a single demo level rather than the full set of topics - though this demo contains enough to showcase all the features in which can be reused to create many different levels.

## Running the project

Requires Unity 6000.0.39f1 and Python 3 on the machine. There is no database server to install, since SQLite is part of Python's standard library and the database file is created on first run.

1. Clone the repository.
2. Open the project folder in Unity Hub and let it import.
3. If `python` is not on your PATH, set the Python path on the Authentication component in the Inspector, or set a `JOURNEYLANES_PYTHON` environment variable pointing at your Python executable.
4. Open the demo scene at Assets/Scenes/Main Menu and press Play.

Account data is written to `credentials.db` in Unity's persistent data folder, so it is kept out of the project directory and out of version control.

## Credits

3D models from [KayKit City Builder Bits 1.0](https://kaylousberg.itch.io/city-builder-bits) by Kay Lousberg
Road sign models from [Old Road Signs](https://assetstore.unity.com/packages/3d/props/exterior/old-road-signs-pbr-170952?srsltid=AYCX9vIQQBPOeWIVX_unWkowgYHn8EYxz69uZK9PKZSp25hUXZid9o53) by Aleksn09
More road sign models from [Road Signs - Big Pack](https://assetstore.unity.com/packages/3d/props/industrial/road-sign-big-pack-139858?srsltid=AYCX9vLvZOj1dtT3pZ05eTBWPYSU8FVVtFrJeJtrJBBWO5kfW7TsHYEw) by Mixall
