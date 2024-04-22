# Docker-GAP-Server
 This is the code for a docker container that contains the GAP-System and a simple Server that serves on port 63910 
```[bash]
docker build -f dockerfile -t gap-server:1.0 .
docker run --name gap_server -p 63910:63910 gap_server:1.0
```
The build process takes a long time (the compilation of GAP's packages takes > 1000 s on my PC) but only needs to be done once.
This built container can be run on a server, like Kamatera for example.