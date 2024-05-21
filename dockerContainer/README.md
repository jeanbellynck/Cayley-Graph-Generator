# Docker-GAP-Server
 This is the code for a docker container that contains the GAP-System and a simple Server that serves on port 63910
```[bash]
docker build -f dockerfile -t gap-server:1.0 .
docker run --name gap_server -p 63910:63910 -d --restart=unless-stopped gap-server:1.0
```
The build process takes a long time (the compilation of GAP's packages takes > 1000 s on my PC) but only needs to be done once.
If you want to be able to update the server files add `--mount type=bind,source=<path to GAP-Server Files>,target=/files`, i.e. 
```[bash]
docker run --name gap_server -p 63910:63910 -d --restart=unless_stopped --mount type=bind,source="/root/GAP-Server Files",target=/files gap-server:1.0 
```

I uploaded two versions to the docker hub, one serving a built version of v. 1.4.6 as static files: https://hub.docker.com/r/johannesheissler/gap-server/tags