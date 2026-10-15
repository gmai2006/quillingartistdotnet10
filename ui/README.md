# QuillingArtist UI (AngularJS 1.8)

AngularJS port of the React + Tailwind + Lucide storefront in `../../quillingartistv2/ui`.
It calls the .NET API in `../src/ApiService` on the same `/api/...` routes.

- **AngularJS 1.8.3** with component architecture: `.component()`, `controllerAs` (`$ctrl`), one-way
  `<` bindings, `&` callbacks, `$onInit`/`$onChanges`/`$onDestroy`, and strict DI (`ng-strict-di`).
- **ngRoute** in `html5Mode` for clean URLs. Pages that need the catalog get it through route `resolve`.
- **Component templates in separate files**: each component has its own folder holding `<name>.component.js`
  and `<name>.template.html`, and points at the HTML with `templateUrl`. The production build puts every
  template in `$templateCache`, so templates are never fetched at runtime (see [Templates](#templates)).
- **Tailwind CSS v4**, compiled by the Tailwind CLI from the class names in the `*.template.html` files.
- **Lucide** through the vanilla `lucide` package and a small `<lucide-icon>` directive.

## Layout

```
src/
  index.html                 shell; script order is also the bundle order
  styles.css                 Tailwind entry
  app/
    app.module.js            angular.module('quillingApp', ['ngRoute'])
    app.config.js            CATEGORIES / API_CONFIG constants, html5Mode, routes
    app.component.js         <quilling-app>: header, menu, ng-view, footer   (App.jsx)
    app.template.html        its template
    services/                ProductService, CartService, CheckoutService, ArrayUtils
    directives/              lucideIcon (the only directive; it does DOM work)
    components/<name>/       header, top menu, footer, search box, forms, ...
      <name>.component.js
      <name>.template.html
    pages/<name>/            one routed component per page                   (pages/*.jsx)
      <name>.component.js
      <name>.template.html
public/                      images and favicons, copied to the site root as-is
scripts/                     build.mjs, dev-server.mjs, vendor.mjs
```

## Run

```sh
npm install
npm run dev        # http://localhost:5173, Tailwind in watch mode, /api -> http://localhost:9000
npm run build      # dist/: hashed app.<hash>.js bundle + app.<hash>.css, vendor/, public assets
npm run preview    # serve dist/ the same way
```

`PORT` and `API_TARGET` override the dev-server port and backend, for example
`API_TARGET=http://localhost:5000 npm run dev`.

The catalog comes from `GET /api/products/catalog`, which is public and read-only (at most 200 products per
page). The UI sends no token; `selectAll` and `export` still need an admin JWT.

## Templates

Components reference their template by a path from the site root (`<base href="/">`):

```js
angular.module('quillingApp')
  .component('searchBox', {
    templateUrl: 'app/components/search-box/search-box.template.html',
    controller: [...],
  });
```

Inside a template, use the `$ctrl` alias (`{{ $ctrl.user.name }}`, `ng-click="$ctrl.save()"`).

- **`npm run dev`** serves `src/` as-is, so each template is fetched once over HTTP and then cached by AngularJS.
- **`npm run build`** finds every `src/app/**/*.template.html` and appends a `run` block to the bundle that calls
  `$templateCache.put('<templateUrl>', '<html>')`. AngularJS checks `$templateCache` before making a request, so the
  built app makes no template requests, and `dist/` doesn't contain the `.html` files.

To add a component, create `src/app/components/<name>/` with both files and add the `.component.js` file to
`index.html`. The template is picked up automatically.

## Docker

`docker compose up --build` builds the bundle and serves it with nginx at `http://<host-ip>:8080/` (`UI_PORT`
overrides the port). `nginx.conf` falls back to `index.html` for client-side routes and proxies `/api/` to the
`quillingartistdotnet:8003` container on the external `proxy-net` network, so start `../docker-compose.yml` too.
Host setup (the `proxy-net` network, Postgres access) is in `../README.md`, "Docker".

## Icons

```html
<lucide-icon name="ShoppingCart" icon-class="h-6 w-6"></lucide-icon>
<lucide-icon name="{{ $ctrl.isOpen ? 'X' : 'Menu' }}" size="18"></lucide-icon>
```

`name` is the PascalCase name used by `lucide-react`. `icon-class` goes on the generated `<svg>`.
