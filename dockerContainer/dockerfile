# Use Alpine Linux as the base image
FROM alpine:latest

# Install dependencies
RUN apk add --no-cache build-base autoconf gmp-dev readline-dev zlib-dev bash python3 py3-pip wget

# Install the server
COPY ["GAP-Server Files", "/files/"]
RUN python3 -m venv /python_venv
RUN /python_venv/bin/pip install -r /files/requirements.txt

# Download and extract GAP
WORKDIR /gap
RUN wget https://github.com/gap-system/gap/releases/download/v4.13.0/gap-4.13.0.tar.gz 
RUN tar -xzf gap-4.13.0.tar.gz
RUN rm gap-4.13.0.tar.gz
# COPY ["GAP-installation-files", "/gap/"]

# Compile GAP
WORKDIR /gap/gap-4.13.0
RUN ./configure
RUN make

# Compile GAP packages (we don't need all of them, but hey). This will take a while (> 1000 s is normal!)
WORKDIR /gap/gap-4.13.0/pkg
RUN bash ../bin/BuildPackages.sh

ENV GAP_PATH=/gap/gap-4.13.0/gap

# Clean up
# RUN apk del build-base autoconf zlib-dev py3-pip
# RUN rm -rf /var/cache/apk/*

# Still need to do this manually when running the container, with -p 63910:63910
EXPOSE 63910

# Set the startup command
CMD ["/python_venv/bin/python", "/files/GAPRunner.py"]