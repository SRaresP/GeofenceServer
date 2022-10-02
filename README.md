# GeofenceServer
The server that stores and retrieves information related to accounts and location for TargetApp and OverseerApp

## Todo

Give the targets their user name when they log in. Consider the location history too.

Fix the problem that causes the server to stop working in certain cases.

## Done.

Overseers can now register and log in. They get their tracked users when they log in as well.

Separated Target handling from Overseer handling

Implemented unique code deletion upon successful usage.

Server scaffolding is done, now only lacks implementation and polish.

Can currently receive and send data to TargetApp.

Implemented unique code generation, automatic removal and validation.

Implemented proper hashing for unique codes. I forgot I could just use the code I wrote like a year ago...