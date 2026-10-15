(function () {
  'use strict';

  angular.module('quillingApp')
    .constant('CATEGORIES', [
      'Animal',
      'Baby',
      'Birthday',
      'Congratulation',
      // "Father's Day",
      'Graduation',
      'Holidays',
      // "Mother's Day",
      'Nature',
      'Scenery',
      'Thank you',
      // 'Valetine'
    ])
    .constant('API_CONFIG', {
      // Public, read-only catalog. No token: anything shipped to the browser is
      // public, and /selectAll (which needs an admin JWT) stays back-office only.
      productsUrl: '/api/products/catalog',
    })
    .config(['$locationProvider', '$compileProvider', function ($locationProvider, $compileProvider) {
      // Clean URLs (/cart, /product/detail/12) like react-router's BrowserRouter.
      // The server must fall back to index.html for unknown paths (see nginx.conf).
      $locationProvider.html5Mode(true);
      $compileProvider.debugInfoEnabled(false);
      $compileProvider.commentDirectivesEnabled(false);
      $compileProvider.cssClassDirectivesEnabled(false);
    }])
    .config(['$routeProvider', function ($routeProvider) {
      // Pages that render products wait for the catalog via route resolve and
      // receive it as a one-way binding.
      const withProducts = {
        products: ['ProductService', function (ProductService) {
          return ProductService.getAll();
        }],
      };

      $routeProvider
        .when('/', {
          template: '<product-list-page products="$resolve.products"></product-list-page>',
          resolve: withProducts,
        })
        .when('/category/:cat', {
          template: '<product-list-page products="$resolve.products"></product-list-page>',
          resolve: withProducts,
        })
        .when('/product/detail/:id', {
          template: '<product-detail-page products="$resolve.products"></product-detail-page>',
          resolve: withProducts,
        })
        .when('/searchResult', { template: '<search-result-page></search-result-page>' })
        .when('/cart', { template: '<cart-page></cart-page>' })
        .when('/checkout', { template: '<checkout-page></checkout-page>' })
        .when('/checkout/success', { template: '<checkout-success-page></checkout-success-page>' })
        .when('/documentation', { template: '<documentation-page></documentation-page>' })
        .when('/contact-us', { template: '<contact-us-page></contact-us-page>' })
        .when('/support', { template: '<support-page></support-page>' })
        .when('/return', { template: '<return-policy-page></return-policy-page>' })
        .when('/privacy', { template: '<privacy-page></privacy-page>' })
        .when('/term', { template: '<term-use-page></term-use-page>' })
        .when('/security', { template: '<security-page></security-page>' })
        .when('/500', { template: '<server-error-page></server-error-page>' })
        .otherwise({ template: '<not-found-page></not-found-page>' });
    }]);
})();
