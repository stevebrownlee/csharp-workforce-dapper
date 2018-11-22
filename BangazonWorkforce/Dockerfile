FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY *.sln .
COPY BangazonWorkforce/*.csproj ./BangazonWorkforce/
COPY BangazonWorkforce.IntegrationTests/*.csproj ./BangazonWorkforce.IntegrationTests/
RUN dotnet restore

# copy everything else and build app
COPY BangazonWorkforce/. ./BangazonWorkforce/
COPY BangazonWorkforce.IntegrationTests/. ./BangazonWorkforce.IntegrationTests/
WORKDIR /app/BangazonWorkforce
RUN dotnet publish -c Release -o out


FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /app
COPY BangazonWorkforce/BangazonWorkforce.db ./
COPY --from=build /app/BangazonWorkforce/out ./

# Configure the listening port
ARG APP_PORT=5051
ENV ASPNETCORE_URLS http://+:$APP_PORT
EXPOSE $APP_PORT

ENTRYPOINT ["dotnet", "BangazonWorkforce.dll"]