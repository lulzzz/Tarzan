# Build with docker build -t tarzan/nfx-server .
FROM microsoft/dotnet:2.1.503-sdk AS build-env

WORKDIR /Tarzan
# Copy everything and build
COPY . ./
# Build the server
RUN dotnet publish -c Release -o out /Tarzan/src/Tarzan.Nfx.IgniteServer

#====================================================================================
# Build runtime image
FROM openjdk:8 as runtime-env
RUN apt-get update
RUN apt-get install -y apt-utils
RUN apt-get install -y apt-transport-https
RUN apt-get update

# Install dotnet runtime:
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
RUN mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
RUN wget -q https://packages.microsoft.com/config/debian/9/prod.list
RUN mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
RUN chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
RUN chown root:root /etc/apt/sources.list.d/microsoft-prod.list
RUN apt-get update

RUN apt-get install -y dotnet-runtime-2.0.0

WORKDIR /nfx-server
COPY --from=build-env /Tarzan/src/Tarzan.Nfx.IgniteServer/out .
ENTRYPOINT ["dotnet", "Tarzan.Nfx.IgniteServer.dll"]

EXPOSE 10800 47100 47500 49112

LABEL image="tarzan/nfx-server" \
      maintainer="rysavy@vutbr.cz" \
      version="1.0"
