$Env:ASPNETCORE_ENVIRONMENT = "STAGE"
dotnet publish -c Release -o published

---
```
docker build -t prr-web-admin -f web-admin.dockerfile  ../WebClients
docker run -d -p 8071:80 prr-web-admin
docker tag prr-web-admin baio/prr-web-admin:v0.11
docker push baio/prr-web-admin:v0.11
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
docker run -p 5000:80 -e ASPNETCORE_ENVIRONMENT=STAGE prr-api
docker tag prr-api baio/prr-api:v0.19
docker push baio/prr-api:v0.19
```

## AKS Cluster

```
az aks get-versions --location northeurope -o table

az aks create --resource-group "prr7aks" --generate-ssh-keys --name prr7aks --node-count 1

az aks get-credentials --resource-group "prr7aks" --name prr7aks

kubectl config get-contexts

kubectl config use-context docker-desktop

kubectl get nodes

kubectl get pods --all-namespaces
```