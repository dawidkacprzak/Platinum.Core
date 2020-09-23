FROM mcr.microsoft.com/dotnet/core/sdk:3.1
RUN apt-get update
WORKDIR /home/app
COPY bin/Release/netcoreapp3.1/publish/ .

ENTRYPOINT ["dotnet","Platinum.Service.OfferDetailsFetcher.dll"] 