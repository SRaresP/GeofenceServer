# GeofenceServer
The server that stores and retrieves information related to accounts and location for TargetApp and OverseerApp

## Todo

Implement TargetCodeHandler:
	-Has a private class that contains an email and the code that is used to store data using EF;
	-Has an add/set method that can set a code for an email with a timer that deletes said code after x minutes (30?)
	-Has a check method that searches the passed code in the DB. If found, it returns the user, as in the code was correct

Implement registration and login for overseers.

Implement proper hashing for unique codes.

## Done

Server scaffolding is done, now only lacks implementation and polish.

Can currently receive and send data to TargetApp.
