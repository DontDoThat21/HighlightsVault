# Use the official ASP.NET Core runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Install network tools for troubleshooting
RUN apt-get update && apt-get install -y curl telnet

# Copy the published output to the container
COPY . .

# Set up the entry point for the application
ENTRYPOINT ["dotnet", "HighlightsVault.dll"]
