# Build with docker build -t tarzan/nfx-server .
FROM microsoft/dotnet:sdk AS build-env
WORKDIR /app
# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore
# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM openjdk:8
RUN apt-get update
RUN apt-get install apt-transport-https

# Install dotnet runtime:
RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
RUN mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
RUN wget -q https://packages.microsoft.com/config/debian/9/prod.list
RUN mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
RUN chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
RUN chown root:root /etc/apt/sources.list.d/microsoft-prod.list

RUN apt-get update
RUN apt-get install apt-transport-https
RUN apt-get install -y dotnet-runtime-2.0.0

# Install IGNITE
ENV IGNITE_VERSION 2.7.0

# Ignite home
ENV IGNITE_HOME /opt/ignite/apache-ignite-fabric-${IGNITE_VERSION}-bin

# Do not rely on anything provided by base image(s), but be explicit, if they are installed already it is noop then
RUN apt-get update && apt-get install -y --no-install-recommends \
        unzip \
        curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /opt/ignite

RUN curl https://dist.apache.org/repos/dist/release/ignite/${IGNITE_VERSION}/apache-ignite-fabric-${IGNITE_VERSION}-bin.zip -o ignite.zip \
    && unzip ignite.zip \
    && rm ignite.zip

WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Tarzan.Nfx.IgniteServer.dll"]

EXPOSE 10800 47100 47500 49112

LABEL image="tarzan/nfx-server" \
      maintainer="rysavy@vutbr.cz" \
      version="1.0"
