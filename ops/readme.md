```
docker build -t prr-web-admin -f web-admin.dockerfile  ../WebClients
docker run -d -p 8081:80 prr-web-admin
```
---
```
docker build -t prr-web-idp -f web-idp.dockerfile  ../WebClients
docker run -d -p 8080:80 prr-web-idp
```
---
```
docker build -t prr-api -f api.dockerfile  ../IdentityServer
docker run -d -p 8082:80 prr-api
```
