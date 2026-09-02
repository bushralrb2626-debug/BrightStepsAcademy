FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY BrightStepsAcademy.csproj ./
RUN dotnet restore BrightStepsAcademy.csproj
COPY . .
RUN dotnet publish BrightStepsAcademy.csproj -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV USE_SQLITE=1
ENV ConnectionStrings__DefaultConnection=Data Source=/app/data/brightsteps.db
RUN mkdir -p /app/data /app/data/EmailOutbox /app/wwwroot/uploads
EXPOSE 10000
ENTRYPOINT ["dotnet", "BrightStepsAcademy.dll"]
