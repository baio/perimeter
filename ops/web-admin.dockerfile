FROM node:12.10.0-alpine AS build-image
WORKDIR /app
RUN npm install -g yarn
COPY decorate-angular-cli.js package.json yarn.lock ./
RUN yarn install

ENV PATH="./node_modules/.bin:$PATH" 

COPY . ./
RUN ng build admin --prod

FROM nginx
COPY .config/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build-image /app/dist/apps/admin /usr/share/nginx/html