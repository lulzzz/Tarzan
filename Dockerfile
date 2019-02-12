FROM microsoft/dotnet:sdk

WORKDIR /tarzan/nfx

COPY . /tarzan/nfx

RUN dotnet build 

CMD ["uname" "-a"]
