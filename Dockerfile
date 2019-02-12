FROM microsoft/dotnet:2.1-sdk

WORKDIR /tarzan/nfx

COPY . /tarzan/nfx

RUN dotnet --info

RUN dotnet build 

CMD ["uname" "-a"]
