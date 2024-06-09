# DUALITY

## The Game

Duality is a two-player LAN online co-op game, where one player plays as Strawberry Boy and another plays as Banana Boy. Each player's goal through the levels is to collect fruits of their own type and reach the winning spot with their partner without dying (stepping into the other's territory or colliding with traps).

The game makes use of Unity Netcode for Gameobjects for connectivity, and Sqlite for the profile database.

## Structure

### Login

Upon start of the game, the players are presented with the option to:
- Login
- Signup
- Play as a Guest

Choosing to ___Login___ will lookup the input put by the player in the *Username* and *Password* fields on the Sqlite database. If the data matches any entry, then the profile information is sent back and sends the player to the main menu successfully.

Choosing to ___Signup___ will take the player to the Signup screen, where they can input an *Username* and *Password*. The game will check that the Username does not already exist; if this check passes, the account is created successfully and the player is sent to the main menu, already logged in.

Both these options will make it so that a profile icon in the main menu is shown. Clicking on that icon will take the player into a screen where they're shown their progression and stats through the game.

Choosing to ___Play as a Guest___ will simply take the player to the main menu. The player won't have access into the profile screen, and their progress will not be saved.

### Server, Client, Host

On the main menu, the player can choose to connect using one of three modes:
- Server
- Host
- Client

If selecting *Client* without a server or host already running, nothing will happen.
