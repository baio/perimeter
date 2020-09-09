dotnet publish -c Release -o published

---
```
docker build -t prr-web-admin -f web-admin.dockerfile  ../WebClients
docker run -d -p 8071:80 prr-web-admin
docker tag prr-web-admin baio/prr-web-admin
docker push baio/prr-web-admin
```
---
```
docker build -t prr-web-idp -f web-idp.dockerfile  ../WebClients
docker run -d -p 8070:80 prr-web-idp
docker tag prr-web-idp baio/prr-web-idp
docker push baio/prr-web-idp
```
---
```
docker build -t prr-api -f api.dockerfile ../IdentityServer
docker run -d -p 80:80 -e ASPNETCORE_ENVIRONMENT=STAGE prr-api
docker tag prr-api baio/prr-api
docker push baio/prr-api
```
