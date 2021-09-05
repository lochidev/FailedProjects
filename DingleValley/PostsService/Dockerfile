#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PostsService/PostsService.csproj", "PostsService/"]
RUN dotnet restore "PostsService/PostsService.csproj"
COPY . .
WORKDIR "/src/PostsService"
RUN dotnet build "PostsService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PostsService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PostsService.dll"]