# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /App

# copy csproj and restore as distinct layers
COPY . ./

RUN dotnet restore

RUN dotnet publish -c release -o /out --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build /App/out .
ENTRYPOINT ["dotnet", "harmonify-api.dll"]