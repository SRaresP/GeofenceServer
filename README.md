# GeofenceServer
The server that stores and retrieves information related to accounts and location for TargetApp and OverseerApp. The server is now usable.

## Todo

Allow targets and overseers to delete their accounts. When they do so, delete all data associated with them.

Give the targets their user name when they log in. Consider the location history too.

## Done

Each overseer - target pair can now have an associated set of settings. The set currently only includes the location update interval.

Overseers can now register and log in. They get their tracked users when they log in as well.

Separated Target handling from Overseer handling

Implemented unique code deletion upon successful usage.

Server scaffolding is done, now only lacks implementation and polish.

Can currently receive and send data to TargetApp.

Implemented unique code generation, automatic removal and validation.

Implemented proper hashing for unique codes. I forgot I could just use the code I wrote like a year ago...