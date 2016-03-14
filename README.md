# FirstPlatformer
A repository for our first platformer game

TODO:
- Pick up rocks (Player.cs)
- Throw rocks (Player.cs)
- Rock physics on sloped surfaces (Projectile.cs)
- Enemy AI (Enemy.cs)
- Level stuff (???)

Enemy.cs will be an action list for enemy decision making. Move decisions feed into Controller.cs.

Projectile.cs for projectile physics (trajectory, velocity, drop, etc.)
- Rocks currently travel from player in direction of player facing
- Respond to gravity, friction and bounce off of horizontal/vertical surfaces
- NEED TO BOUNCE CORRECTLY OFF OF SLOPED SURFACES
