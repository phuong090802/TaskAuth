FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

WORKDIR /app

EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src

COPY ["TaskAuth/TaskAuth.csproj", "TaskAuth/"]

RUN dotnet restore "TaskAuth/TaskAuth.csproj"

COPY . .

WORKDIR "/src/TaskAuth"

RUN dotnet build "TaskAuth.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "TaskAuth.csproj" -c Release -o /app/publish


FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TaskAuth.dll"]	