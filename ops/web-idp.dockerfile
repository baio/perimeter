FROM node:12.10.0-alpine AS build-image
WORKDIR /app
RUN npm install -g yarn
COPY decorate-angular-cli.js package.json yarn.lock ./
RUN yarn install

ENV PATH="./node_modules/.bin:$PATH" 

COPY . ./
RUN ng build ip --prod

FROM nginx
COPY .config/default.conf /etc/nginx/conf.d/default.conf
COPY --from=build-image /app/dist/apps/ip /usr/share/nginx/html