FROM mcr.microsoft.com/dotnet/core/aspnet:3.0 AS base
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://*:80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /src
COPY ["TestProject.WebAPI/TestProject.WebAPI.csproj", "TestProject.WebAPI/"]
RUN dotnet restore "TestProject.WebAPI/TestProject.WebAPI.csproj"
COPY . .
WORKDIR "/src/TestProject.WebAPI"
RUN dotnet build "TestProject.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TestProject.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TestProject.WebAPI.dll"]