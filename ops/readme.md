docker build -t web-admin -f web-admin.dockerfile  ../WebClients
docker run web-admin -d -p 8080:80