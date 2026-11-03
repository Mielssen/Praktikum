FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Praktikum542/Praktikum542.csproj", "Praktikum542/"]
RUN dotnet restore "Praktikum542/Praktikum542.csproj"
COPY ["Praktikum542/", "Praktikum542/"]
RUN dotnet publish "Praktikum542/Praktikum542.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
# TravelManagerUI is a content-root folder, not wwwroot.
COPY --from=build /src/Praktikum542/TravelManagerUI ./TravelManagerUI
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Praktikum542.dll"]